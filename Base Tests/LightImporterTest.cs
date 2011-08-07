using ColladaXna.Base.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using ColladaXna.Base;
using ColladaXna.Base.Lighting;
using Microsoft.Xna.Framework;

namespace Base_Tests
{
    
    
    /// <summary>
    ///This is a test class for LightImporterTest and is intended
    ///to contain all LightImporterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LightImporterTest
    {
        static ColladaModel apcModel = new ColladaModel();

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            // Loads COLLADA document for testing
            XmlDocument apcDocument = TestBase.LoadDocument("APC_animation.DAE");

            LightImporter importer = new LightImporter();
            importer.Import(apcDocument.DocumentElement, apcModel);
        }

        /// <summary>
        /// Checks whether lights have been imported
        ///</summary>
        [TestMethod]
        public void LightImportTest()
        {
            Assert.AreEqual(1, apcModel.Lights.Count, "Number of Lights");
            Assert.AreEqual("AmbientLight", apcModel.Lights[0].Name);

            AmbientLight ambient = apcModel.Lights[0] as AmbientLight;
            Assert.IsNotNull(ambient);

            Assert.AreEqual(new Color(0, 0, 0), ambient.Color);
        }
    }
}
