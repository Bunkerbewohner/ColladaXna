using ColladaXna.Base.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using ColladaXna.Base;

namespace Base_Tests
{
    
    
    /// <summary>
    ///This is a test class for SceneImporterTest and is intended
    ///to contain all SceneImporterTest Unit Tests
    ///</summary>
    [TestClass]
    public class SceneImporterTest
    {
        static ColladaModel apcModel = new ColladaModel();

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            // Loads COLLADA document for testing
            XmlDocument apcDocument = TestBase.LoadDocument("APC_animation.DAE");

            // Requires geometry to create mesh instances
            GeometryImporter geometryImporter = new GeometryImporter();
            geometryImporter.Import(apcDocument.DocumentElement, apcModel);

            SceneImporter importer = new SceneImporter();
            importer.Import(apcDocument.DocumentElement, apcModel);
        }

        /// <summary>
        /// Checks whether joints have been imported correctly
        ///</summary>
        [TestMethod]
        public void SceneImportTest()
        {
            Assert.Inconclusive("TODO: Import scene import test");
        }
    }
}
