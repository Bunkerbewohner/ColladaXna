using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

using Omi.Xna.Collada.Importer.Import;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Animation;

namespace Omi.Xna.Collada.Importer
{
    /// <summary>
    /// This class imports COLLADA (.dae) models for use with the standard XNA Model class.
    /// </summary>
    //[ContentImporter(".dae", CacheImportedData=true, DisplayName = "COLLADA Standard Importer", DefaultProcessor = "ModelProcessor")]
    public class ColladaStandardImporter : ContentImporter<NodeContent>
    {
        /// <summary>
        /// List of importer classes. Every importer takes care of different
        /// parts of the file, like geometry, materials or light.
        /// </summary>
        List<IColladaImporter> importers = new List<IColladaImporter>();

        IntermediateModel model;
        NodeContent rootNode;
        ContentImporterContext context;
        Dictionary<Joint, NodeContent> jointNodes = new Dictionary<Joint, NodeContent>();

        public ColladaStandardImporter()
        {
            // Note: GeometryImporter needs Skeleton to create vertex channels for joint weights/indices
            importers.Add(new SkeletonImporter());
            importers.Add(new GeometryImporter());            

            importers.Add(new MaterialImporter());
            importers.Add(new SceneImporter());
            importers.Add(new LightImporter());
            
            importers.Add(new AnimationImporter());
        }

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            this.context = context;
            model = LoadIntermediateModel(filename);

            rootNode = new NodeContent();
            rootNode.Identity = new ContentIdentity(filename);
            rootNode.Name = Path.GetFileNameWithoutExtension(filename);

            // Create nodes for joints
            NodeContent rootJointNode = new NodeContent();
            rootJointNode.Name = model.RootJoint.Name != null ? model.RootJoint.Name : model.RootJoint.GlobalID;
            rootJointNode.Transform = model.RootJoint.Transform;
            jointNodes[model.RootJoint] = rootJointNode;
            AddJointNodes(rootJointNode, model.RootJoint.Children);
            rootNode.Children.Add(rootJointNode);

            // Add Mesh nodes
            AddMeshNodes(rootNode);

            return rootNode;
        }   
     
        void AddMeshNodes(NodeContent node)
        {
            MeshBuilder meshBuilder;

            foreach (var mesh in model.MeshInstances)
            {
                if (mesh.ParentJoint == null) mesh.ParentJoint = model.RootJoint;

                MeshContent meshContent = new MeshContent();

                meshBuilder = MeshBuilder.StartMesh(mesh.MeshName);
                
                // TODO: Finish Standard Importer implementation
            }
        }

        void AddJointNodes(NodeContent node, JointList joints)
        {
            if (joints == null || joints.Count == 0) return;

            foreach (var joint in joints)
            {
                NodeContent child = new NodeContent();
                child.Name = joint.Name != null ? joint.Name : joint.GlobalID;
                child.Transform = joint.Transform;

                jointNodes[joint] = child;

                AddJointNodes(child, joint.Children);
            }           
        }

        IntermediateModel LoadIntermediateModel(string filename)
        {
            // Namespaces are handled wrongly by XPath 1.0 and also we don't need
            // them anyway, so all namespaces are simply removed
            string xmlWithoutNamespaces = Regex.Replace(File.ReadAllText(filename),
                @"xmlns="".+?""", "");

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlWithoutNamespaces);
            XmlNode xmlRoot = xml.DocumentElement;

            if (xmlRoot == null)
            {
                throw new ApplicationException("XML Root not found");
            }

            IntermediateModel importedData = new IntermediateModel();
            importedData.SourceFilename = filename;

            foreach (IColladaImporter importer in importers)
            {
                importer.Import(xmlRoot, ref importedData);
            }

            return importedData;
        }
    }
}
