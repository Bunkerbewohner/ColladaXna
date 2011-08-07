using ColladaXna.Base.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using ColladaXna.Base;

namespace Base_Tests
{
    
    
    /// <summary>
    /// Tests the functionality of the skeleton importer, whose task it is to 
    /// find all joint definitions referenced by the "skeleton" tag in the
    /// COLLADA file.
    ///</summary>
    [TestClass]
    public class SkeletonImporterTest : TestBase
    {
        static ColladaModel apcModel = new ColladaModel();

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            // Loads COLLADA document for testing
            XmlDocument apcDocument = TestBase.LoadDocument("APC_animation.DAE");

            SkeletonImporter importer = new SkeletonImporter();
            importer.Import(apcDocument.DocumentElement, apcModel);
        }

        /// <summary>
        /// Checks whether joints have been imported correctly
        ///</summary>
        [TestMethod]
        public void SkeletonImportTest()
        {
            // 2 actual joints and the additional root joint
            Assert.AreEqual(3, apcModel.Joints.Count, "Number of Joints");

            // Joint names
            Assert.AreEqual("bone_turret", apcModel.Joints[0].Name, "Joint Name");
            Assert.AreEqual("bone_gun", apcModel.Joints[1].Name, "Joint Name");

            // Joint connections
            Assert.AreEqual(apcModel.Joints[0], apcModel.Joints[2].Children[0], "Root Joint connected");
            Assert.AreEqual(apcModel.Joints[1], apcModel.Joints[0].Children[0], "First Joint connected");
        }
    }
}
