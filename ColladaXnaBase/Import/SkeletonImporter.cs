using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Util;

namespace ColladaXna.Base.Import
{
    /// <summary>
    /// This class imports the joint structure of the model.
    /// If no joints are found there will be still one root joint
    /// with Identity transformation added to the model's joint
    /// collection.
    /// </summary>
    public class SkeletonImporter : IColladaImporter
    {
        private readonly Dictionary<string,bool> knownJoints = new Dictionary<string, bool>(50);

        #region IColladaImporter Member

        /// <summary>
        /// Imports all joint nodes referenced by &lt;skeleton&gt; nodes and their
        /// children. Stores the found joints in the given model's joint collection.
        /// </summary>
        /// <param name="xmlRoot">XML root node</param>
        /// <param name="model">model to store joints in</param>
        public void Import(XmlNode xmlRoot, ColladaModel model)
        {            
            knownJoints.Clear();

            // Add root joint that can be used to transform the model alltogether
            Joint root = new Joint("__root")
                             {
                                 Parent = null,
                                 Transform = Matrix.Identity,                                 
                                 Index = 0,
                                 ScopedID = "__root",
                                 GlobalID = "__root"
                             };            

            ReadJoints(root, FindJointNodes(xmlRoot), model);

            // Last joint is the root
            model.Joints.Add(root);
            root.Index = model.Joints.Count - 1;
        }

        #endregion        

        /// <summary>
        /// Reads the given joints and all of their children recursively. Newly created joints
        /// are appended to the Children collection of the parent (if not null) and to the
        /// Joints collection of the given model.
        /// </summary>
        /// <param name="parent">Parent joint of joints to read</param>
        /// <param name="xmlNodes">XML joint nodes</param>
        /// <param name="model">Model instance with non-null joint collection</param>
        void ReadJoints(Joint parent, IEnumerable<XmlNode> xmlNodes, ColladaModel model)
        {                        
            bool jointWithoutId = false;

            foreach (XmlNode xmlNode in xmlNodes)
            {
                // Create joint from XML
                Joint joint = new Joint(GetNodeName(xmlNode))
                              {
                                  Parent = parent,
                                  Transform = CreateNodeTransform(xmlNode),          
                                  GlobalID = xmlNode.GetAttributeString("id"),
                                  ScopedID = xmlNode.GetAttributeString("sid")                                  
                              };                

                // Check if this joint was already added, only possible if ID is set);
                if (!String.IsNullOrEmpty(joint.GlobalID))
                {
                    if (knownJoints.ContainsKey(joint.GlobalID)) continue;
                    knownJoints.Add(joint.GlobalID, true);    
                }
                else
                {
                    // There was a joint without ID 
                    jointWithoutId = true;
                }

                // Append this joint to parents' children collection
                if (parent != null)
                {
                    if (parent.Children == null)
                        parent.Children = new JointList { joint };
                    else 
                        parent.Children.Add(joint);
                }

                // Add joint to model's joint collection
                joint.Index = model.Joints.Count;
                model.Joints.Add(joint);

                // Read child nodes of this joint
                XmlNodeList xmlChildren = xmlNode.SelectNodes("node");
                if (xmlChildren != null)
                {                    
                    ReadJoints(joint, xmlChildren.OfType<XmlNode>(), model);
                }                
            }

            if (jointWithoutId)
            {
                // There were joints without IDs, these bones could be contained
                // in model's joint collection multiple times
                // TODO: Remove double joints in case of Joint without Id
            }                                 
        }

        /// <summary>
        /// Returns a name of the given XML node.
        /// If the name attribute is defined its value is returned.
        /// Otherwise the id attribute is used instead. If no id
        /// is defined null is returned.
        /// </summary>
        /// <param name="xmlNode">XML node</param>
        /// <returns>name, id or null</returns>
        static string GetNodeName(XmlNode xmlNode)
        {
            if (xmlNode.Attributes["name"] != null)
            {
                return xmlNode.Attributes["name"].Value;
            }
            else if (xmlNode.Attributes["id"] != null)
            {
                return xmlNode.Attributes["id"].Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds root joint nodes in the document.
        /// If there are none an empty list is returned.
        /// </summary>
        /// <param name="xmlRoot"></param>
        /// <returns></returns>
        static List<XmlNode> FindJointNodes(XmlNode xmlRoot)
        {            
            var skeletonNodes = xmlRoot.SelectNodes(".//skeleton");

            if (skeletonNodes == null || skeletonNodes.Count == 0)
            {
                // Some exporters (like the one used by Autodesk 3ds Max 2012)
                // doesn't export skeleton reference; therefore just look for the first JOINT node
                try
                {
                    XmlNode root = xmlRoot.SelectNodes(".//node[@type='JOINT'][1]")[0];

                    var list = new List<XmlNode>();

                    if(root != null)
                        list.Add(root);

                    return list;
                }
                catch (Exception)
                {
                    return new List<XmlNode>();
                }
            }

            // Fetch referenced nodes
            var nodes = new List<XmlNode>();

            foreach (XmlNode skeletonNode in skeletonNodes)
            {
                string nodeId = skeletonNode.InnerText.Substring(1);
                XmlNode xmlNode = xmlRoot.SelectSingleNode(".//node[@id='" + nodeId + "']");
                
                if (xmlNode == null)
                {
                    throw new Exception("node with ID '" + nodeId + "' could " + 
                        " not be found in the document");
                }

                nodes.Add(xmlNode);
            }

            return nodes;
        }

        /// <summary>
        /// Looks for transformation elements (matrix, rotate, scale, translate, lookat) within
        /// a node and creates a transform matrix from it. If no transformations are
        /// found Identity is returned.
        /// </summary>
        /// <remarks>skew transformations are not supported right now</remarks>
        /// <param name="xmlNode">XML node</param>
        /// <returns>Transform Matrix</returns>
        static Matrix CreateNodeTransform(XmlNode xmlNode)
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
    }

}
