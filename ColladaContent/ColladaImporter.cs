using System;
using System.Collections.Generic;
using Seafarer.Xna.Collada.Importer.Import;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;
using Seafarer.Xna.Collada.Importer;
using System.Text.RegularExpressions;
using System.IO;

namespace COLLADA.ContentPipeline
{
    /// <summary>
    /// Imports a COLLADA file into a class representation of
    /// this files properties. Only supported elements are
    /// imported and can be accessed through the resulting
    /// ColladaData class.
    /// </summary>
    [ContentImporter(".dae", DisplayName = "COLLADA Importer", DefaultProcessor = "ColladaProcessor")]
    public class ColladaImporter : ContentImporter<Seafarer.Xna.Collada.Importer.IntermediateModel>
    {
        /// <summary>
        /// List of importer classes. Every importer takes care of different
        /// parts of the file, like geometry, materials or light.
        /// </summary>
        List<IColladaImporter> importers = new List<IColladaImporter>();

        public ColladaImporter()
        {
            // Note: GeometryImporter needs Skeleton to create vertex channels for joint weights/indices
            importers.Add(new SkeletonImporter());
            importers.Add(new GeometryImporter());            

            importers.Add(new MaterialImporter());
            importers.Add(new SceneImporter());
            importers.Add(new LightImporter());
            
            importers.Add(new AnimationImporter());
        }

        public override Seafarer.Xna.Collada.Importer.IntermediateModel Import(string filename, ContentImporterContext context)
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
