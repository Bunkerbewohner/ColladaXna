﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using System;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Util;

namespace ColladaXna.Base.Import
{
    public class SceneImporter : IColladaImporter
    {
        private ColladaModel _model;

        #region IColladaImporter Member

        public void Import(XmlNode xmlRoot, ColladaModel model)
        {
            _model = model;
            XmlNode xmlScene = xmlRoot.SelectSingleNode("scene");

            model.MeshInstances = ImportMeshInstances(xmlScene, model);            
        }

        #endregion

        #region Scene Importing

        List<MeshInstance> ImportMeshInstances(XmlNode xmlScene, ColladaModel model)
        {
            if (model.Meshes.Any() == false)
            {
                throw new Exception("No meshes found");
            }

            List<MeshInstance> meshes = new List<MeshInstance>();

            XmlNode xmlVisualScene = GetVisualScene(xmlScene);

            // Look for geometry instances and determine transformation from bottom up
            XmlNodeList xmlGeometries = xmlVisualScene.SelectNodes(".//instance_geometry");

            foreach (XmlNode xmlGeom in xmlGeometries)
            {
                string geometryId = xmlGeom.Attributes["url"].Value.Substring(1);
                string key = GetMeshLibKey(xmlGeom, geometryId);

                // Check whether the referenced mesh exists
                if (!model.Meshes.Any(m => m.Name == key)) continue;

                MeshInstance mesh = new MeshInstance();
                mesh.MeshName = key;                
                mesh.AbsoluteTransform = CreateAbsoluteTransform(xmlGeom);
                mesh.ParentJoint = GetParentJoint(xmlGeom, model);

                meshes.Add(mesh);
            }

            // Look for controller instances
            // TODO: Load controller instances and joints correctly!
            XmlNodeList xmlControllerInstances = xmlVisualScene.SelectNodes(".//instance_controller");

            foreach (XmlNode xmlControllerInstance in xmlControllerInstances)
            {
                string meshId = GetMeshId(xmlControllerInstance);
                string key = GetMeshLibKey(xmlControllerInstance, meshId);

                // Check whether the referenced mesh exists
                if (!model.Meshes.Any(m => m.Name == key)) continue;

                MeshInstance mesh = new MeshInstance();
                mesh.MeshName = key;
                mesh.AbsoluteTransform = CreateAbsoluteTransformFromSkeleton(xmlControllerInstance);
                mesh.ParentJoint = GetParentJoint(xmlControllerInstance, model);

                meshes.Add(mesh);
            }

            // Check if any meshes have been found
            if (meshes.Count == 0)
            {
                throw new Exception("No mesh instances have been found");
            }

            return meshes;
        }

        #endregion

        #region Helper Methods and XML Parsing

        Joint GetParentJoint(XmlNode xmlNode, ColladaModel model)
        {
            XmlNode xmlParent = xmlNode.ParentNode;
            if (xmlParent == null) return null;
            if (xmlParent.Attributes == null || xmlParent.Attributes.Count == 0) return null;
        
            string name = xmlParent.Attributes["name"] != null
                              ? xmlParent.Attributes["name"].Value
                              : xmlParent.Attributes["id"].Value;

            return model.Joints.Where(j => j.Name.Equals(name)).FirstOrDefault();            
        }

        string GetMeshLibKey(XmlNode xmlNode, string id)
        {
            XmlNode xmlRoot = xmlNode.OwnerDocument.DocumentElement;
            XmlNode nameNode = xmlRoot.SelectSingleNode(".//geometry[@id='" + id + "']/@name");
            return nameNode != null ? nameNode.Value : id;
        }

        Matrix CreateAbsoluteTransformFromSkeleton(XmlNode xmlControllerInstance)
        {            
            XmlNode skeleton = xmlControllerInstance.SelectSingleNode("skeleton");
            string nodeId = skeleton != null
                ? skeleton.InnerText.Trim().Substring(1)
                : _model.RootJoint.GlobalID;                        

            XmlNode xmlRoot = xmlControllerInstance.OwnerDocument.DocumentElement;
            XmlNode xmlNode = xmlRoot.SelectSingleNode("id('" + nodeId + "')");

            XmlNode xmlParent = xmlNode;
            Matrix transform = Matrix.Identity;

            while (xmlParent != null && xmlParent.Name == "node")
            {
                Matrix matrix = CreateNodeTransform(xmlParent);
                transform = matrix * transform;

                xmlParent = xmlParent.ParentNode;
            }

            return transform;
        }

        string GetMeshId(XmlNode xmlControllerInstance)
        {
            string controllerId = xmlControllerInstance.Attributes["url"].Value.Substring(1);
            XmlNode xmlRoot = xmlControllerInstance.OwnerDocument.DocumentElement;
            XmlNode xmlController = xmlRoot.SelectSingleNode(".//controller[@id='" + controllerId + "']");
            XmlNode xmlSkin = xmlController.SelectSingleNode("skin");

            return xmlSkin.Attributes["source"].Value.Substring(1);
        }

        /// <summary>
        /// Traverses the node hierarchy up from a geometry node
        /// to accumulate all transformations of the geometry instance.
        /// </summary>
        /// <param name="xmlGeometry">XML geometry node</param>
        /// <returns>Absolute Transformation</returns>
        Matrix CreateAbsoluteTransform(XmlNode xmlGeometry)
        {
            XmlNode xmlParent = xmlGeometry.ParentNode;
            Matrix transform = Matrix.Identity;

            while (xmlParent != null && xmlParent.Name == "node")
            {
                Matrix matrix = CreateNodeTransform(xmlParent);
                transform = matrix * transform;

                xmlParent = xmlParent.ParentNode;
            }

            return transform;
        }

        /// <summary>
        /// Looks for transformation elements (matrix, rotate, scale, translate, lookat) within
        /// a node and creates a transform matrix from it. If no transformations are
        /// found Identity is returned.
        /// </summary>
        /// <remarks>skew transformations are not supported right now</remarks>
        /// <param name="xmlNode">XML node</param>
        /// <returns>Transform Matrix</returns>
        Matrix CreateNodeTransform(XmlNode xmlNode)
        {
            Matrix transform = Matrix.Identity;

            // Read transformation elements in order and apply them accordingly
            foreach (XmlNode xmlChild in xmlNode.ChildNodes)
            {
                Matrix matrix;
                float[] values;

                switch (xmlChild.Name)
                {
                    // transformation in form of a matrix
                    case "matrix":
                        matrix = XmlUtil.ParseMatrix(xmlChild.InnerText);
                        break;

                    // rotation as Vector4
                    case "rotate":
                        values = XmlUtil.ParseFloats(xmlChild.InnerText);
                        Vector3 axis = new Vector3(values[0], values[1], values[2]);
                        float angle = MathHelper.ToRadians(values[3]);

                        matrix = Matrix.CreateFromAxisAngle(axis, angle);
                        break;

                    // translation as Vector3
                    case "translate":
                        Vector3 translate = XmlUtil.ParseVector3(xmlChild.InnerText);
                        matrix = Matrix.CreateTranslation(translate);
                        break;

                    // scaling 
                    case "scale":
                        matrix = Matrix.CreateScale(XmlUtil.ParseVector3(xmlChild.InnerText));
                        break;

                    // lookat as 3x3 matrix 
                    case "lookat":
                        values = XmlUtil.ParseFloats(xmlChild.InnerText);
                        Vector3 eye = new Vector3(values[0], values[1], values[2]);
                        Vector3 target = new Vector3(values[3], values[4], values[5]);
                        Vector3 up = new Vector3(values[6], values[7], values[8]);

                        matrix = Matrix.CreateLookAt(eye, target, up);
                        break;

                    // no transformation element or not supported (skew)
                    default:
                        continue;
                }

                transform = matrix * transform;
            }

            return transform;
        }

        /// <summary>
        /// Gets the visual scene node (&lt;visual_scene&gt;) from a scene node (&lt;scene&gt;).
        /// For this the instance_visual_scene element is processed to look up the actual
        /// visual scene id.
        /// </summary>
        /// <param name="xmlScene">XML scene node</param>
        /// <returns>XML visual scene node</returns>
        XmlNode GetVisualScene(XmlNode xmlScene)
        {
            XmlNode xmlSceneInstance = xmlScene.SelectSingleNode("instance_visual_scene");
            if (xmlSceneInstance == null)
                throw new Exception("No visual scene instance found");

            string sceneId = xmlSceneInstance.Attributes["url"].Value.Substring(1);
            XmlNode xmlRoot = xmlScene.OwnerDocument.DocumentElement;
            XmlNode xmlVisualScene = xmlRoot.SelectSingleNode("library_visual_scenes/" +
                "visual_scene[@id='" + sceneId + "']");
            if (xmlVisualScene == null)
                throw new Exception("Visual Scene '" + sceneId + "' not found");

            return xmlVisualScene;
        }

        #endregion
    }
}
