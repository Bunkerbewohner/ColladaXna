using ColladaXna.Base.Import;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using System.Linq;
using ColladaXna.Base;
using Microsoft.Xna.Framework.Graphics;

namespace Base_Tests
{
    
    
    /// <summary>
    ///This is a test class for GeometryImporterTest and is intended
    ///to contain all GeometryImporterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class GeometryImporterTest
    {
        static XmlDocument apcDocument;
        static ColladaModel apcModel;
        
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; protected set; }

        [ClassInitialize]
        public static void Init(TestContext context)
        {            
            apcDocument = TestDataLoader.LoadDocument("APC_animation.DAE");
            XmlNode xmlRoot = apcDocument.DocumentElement;

            apcModel = new ColladaModel();

            // Geometry Importer needs skeleton data (joints) for joint weight & index streams
            SkeletonImporter skeletonImporter = new SkeletonImporter();
            skeletonImporter.Import(xmlRoot, apcModel);

            // Import actual geometry
            GeometryImporter importer = new GeometryImporter();            
            importer.Import(xmlRoot, apcModel);            
        }
        
        /// <summary>
        /// General test of mesh import. Just checks superficially whether all 
        /// meshes were found.
        /// </summary>
        [TestMethod]
        public void ImportMeshesTest()
        {
            var meshes = apcModel.Meshes;

            // Proper number of meshes imported?
            Assert.AreEqual<int>(3, meshes.Count, "Number of meshes");

            // Correct names?
            Assert.AreEqual("apc", meshes[0].Name, "Name of mesh[0]");
            Assert.AreEqual("turret", meshes[1].Name, "Name of mesh[1]");
            Assert.AreEqual("gun", meshes[2].Name, "Name of mesh[2]");                        
        }

        /// <summary>
        /// Tests for correct import of a simple static mesh with 5 vertex channels
        /// based on the sample model "APC_animation.DAE".
        /// </summary>
        [TestMethod]
        public void StaticMeshImportTest()
        {
            var meshes = apcModel.Meshes;

            // Number of vertex containers
            Assert.AreEqual(1, meshes[0].VertexContainers.Length, "Number of Vertex Channels (apc)");

            // Check vertex container contents
            var container0 = meshes[0].VertexContainers[0];

            // Correct vertex channels?
            var channels = meshes[0].VertexContainers[0].VertexChannels.All(c =>
                c.Description.VertexElementUsage == VertexElementUsage.Position ||
                c.Description.VertexElementUsage == VertexElementUsage.Normal ||
                c.Description.VertexElementUsage == VertexElementUsage.TextureCoordinate ||
                c.Description.VertexElementUsage == VertexElementUsage.Tangent ||
                c.Description.VertexElementUsage == VertexElementUsage.Binormal);

            Assert.IsTrue(channels, "All Vertex Channels (apc)");

            // 85 triangles, 3 vertices each should be referenced by indices
            Assert.AreEqual(85 * 3, container0.Indices.Length, "Number of vertices (apc)");

            // Each vertex consists of 15 floats
            Assert.AreEqual(15, container0.VertexSize, "Vertex Size (apc)");
            Assert.AreEqual(15 * 85 * 3, container0.Vertices.Length);
        }

        /// <summary>
        /// Tests for correct import of a skinned mesh including vertex channels for
        /// joint weights and indices, based on the sample model "APC_animation.DAE".
        /// </summary>
        [TestMethod]        
        public void SkinnedMeshImportTest()
        {
            var meshes = apcModel.Meshes;

            // Check vertex container contents
            var container1 = meshes[1].VertexContainers[0];

            // Correct vertex channels?
            var channels1 = container1.VertexChannels.All(c =>
                c.Description.VertexElementUsage == VertexElementUsage.Position ||
                c.Description.VertexElementUsage == VertexElementUsage.Normal ||
                c.Description.VertexElementUsage == VertexElementUsage.TextureCoordinate ||
                c.Description.VertexElementUsage == VertexElementUsage.Tangent ||
                c.Description.VertexElementUsage == VertexElementUsage.Binormal ||
                c.Description.VertexElementUsage == VertexElementUsage.BlendWeight ||
                c.Description.VertexElementUsage == VertexElementUsage.BlendIndices);

            Assert.IsTrue(channels1, "All Vertex Channels (apc-turret)");

            // 85 triangles, 3 vertices each should be referenced by indices
            Assert.AreEqual(112 * 3, container1.Indices.Length, "Number of vertices (apc-turret)");

            // Each vertex consists of 15 floats
            Assert.AreEqual(22, container1.VertexSize, "Vertex Size (apc-turret)");
            Assert.AreEqual(22 * 112 * 3, container1.Vertices.Length);
        }
    }
}
