﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Animation;
using ColladaXna.Base.Lighting;
using ColladaXna.Base.Materials;
using ColladaXna.Base.Import;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base
{
    /// <summary>
    /// ColladaModel is a container class for all Model related contents.
    /// The contents are stored in a way that is easily accessible.
    /// </summary>
    public class ColladaModel
    {
        /// <summary>
        /// Name of the COLLADA file that this model is based on
        /// </summary>
        public String SourceFilename = String.Empty;

        /// <summary>
        /// Collection of different meshes (geometry only)
        /// </summary>
        public List<Mesh> Meshes = new List<Mesh>();

        /// <summary>
        /// Collection of individual mesh instances whose
        /// geometry is based on one of the meshes (see Model.Meshes)
        /// but who have different values for transformation etc.
        /// </summary>
        public List<MeshInstance> MeshInstances = new List<MeshInstance>();

        /// <summary>
        /// Collection of all used materials
        /// </summary>
        public List<Material> Materials = new List<Material>();

        /// <summary>
        /// Lighting information
        /// </summary>
        public List<Light> Lights = new List<Light>();

        /// <summary>
        /// Collection of all joints
        /// </summary>
        public JointList Joints = new JointList();

        /// <summary>
        /// List of joint animations for skeletal / skinned animation
        /// </summary>
        public JointAnimationList JointAnimations = new JointAnimationList();

        /// <summary>
        /// List of animation clips 
        /// </summary>
        public List<JointAnimationClip> JointAnimationClips = new List<JointAnimationClip>();

        /// <summary>
        /// Returns the Root joint of the model, which by convention is the last joint
        /// in the Joints collection.
        /// </summary>
        public Joint RootJoint
        {
            get
            {
                if (Joints != null && Joints.Any())
                {
                    return Joints[Joints.Count - 1];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Creates an empty ColladaModel instance
        /// </summary>
        public ColladaModel()
        {

        }

        /// <summary>
        /// Creates a new ColladaModel instance and directly loads
        /// the given .DAE file.
        /// </summary>
        /// <param name="filename">.DAE file path</param>
        public ColladaModel(String filename)
        {
            SourceFilename = filename;
            Load(filename);
        }

        /// <summary>
        /// Loads the contents of given .DAE file into this ColladaModel
        /// instance. Existing contents are overriden. This method is
        /// automatically called by the constructor that accepts a filename.
        /// </summary>
        /// <param name="filename">.DAE file path</param>
        public void Load(String filename)
        {
            List<IColladaImporter> importers = new List<IColladaImporter>();
            importers.Add(new SkeletonImporter());
            importers.Add(new GeometryImporter());
            importers.Add(new MaterialImporter());
            importers.Add(new SceneImporter());
            importers.Add(new LightImporter());
            importers.Add(new AnimationImporter());

            // Namespaces are handled wrongly by XPath 1.0 and also we don't need
            // them anyway, so all namespaces are simply removed
            string xmlWithoutNamespaces = Regex.Replace(File.ReadAllText(SourceFilename),
                @"xmlns="".+?""", "");

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlWithoutNamespaces);
            XmlNode xmlRoot = xml.DocumentElement;

            if (xmlRoot == null)
            {
                throw new Exception("XML Root not found");
            }            

            foreach (IColladaImporter importer in importers)
            {
                importer.Import(xmlRoot, this);
            }            
        }
    }
}
