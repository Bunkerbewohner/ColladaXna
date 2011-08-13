using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ColladaXna.Base;
using System.IO;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Materials;

namespace ColladaXnaImporter
{
    using CVertexChannel = ColladaXna.Base.Geometry.VertexChannel;        

    /// <summary>
    /// This class imports a COLLADA ".dae" file into the XNA default content model for models.
    /// As a result models imported with this class can be processed and loaded with all the
    /// default content processors just like FBX models.
    /// </summary>
    [ContentImporter(".dae", CacheImportedData = true, DisplayName="COLLADA Standard Importer", DefaultProcessor="ModelProcessor")]
    public class ColladaStdModelImporter : ContentImporter<NodeContent>
    {
        ColladaModel collada;
        ContentImporterContext importerContext;
        NodeContent rootNode;
        MeshBuilder meshBuilder;
        Dictionary<String, MaterialContent> materials;        

        /// <summary>
        /// Imports the COLLADA Model from the given .DAE file into the XNA Content Model.
        /// </summary>
        /// <param name="filename">Path to .DAE file</param>
        /// <param name="context">Context (is not used)</param>
        /// <returns></returns>
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            importerContext = context;

            // Load the complete collada model which is to be converted / imported
            collada = new ColladaModel(filename);

            //Debugger.Launch();
            //Debugger.Break();
            
            rootNode = new NodeContent();
            rootNode.Name = Path.GetFileNameWithoutExtension(filename);
            rootNode.Identity = new ContentIdentity(filename);            

            CreateMaterials();
            CreateMeshes();

            return rootNode;
        }       

        /// <summary>
        /// Creates BasicMaterialContent instances for each COLLADA material and stores
        /// them in the "materials" dictionary of this class. 
        /// </summary>
        void CreateMaterials()
        {
            materials = new Dictionary<string, MaterialContent>();

            for (int i = 0; i < collada.Materials.Count; i++)
            {
                BasicMaterialContent material = new BasicMaterialContent();                                
                material.Name = collada.Materials[i].Name;

                var diffuse = collada.Materials[i].Properties.OfType<DiffuseColor>().FirstOrDefault();
                if (diffuse != null) material.DiffuseColor = diffuse.Color.ToVector3();

                var texture = collada.Materials[i].Properties.OfType<DiffuseMap>().FirstOrDefault();
                if (texture != null)
                {
                    String dir = Path.GetDirectoryName(collada.SourceFilename) + "/";
                    material.Texture = new ExternalReference<TextureContent>(dir + texture.Texture.Filename);
                }

                var specular = collada.Materials[i].Properties.OfType<SpecularColor>().FirstOrDefault();
                if (specular != null) material.SpecularColor = specular.Color.ToVector3();

                var specpow = collada.Materials[i].Properties.OfType<SpecularPower>().FirstOrDefault();
                if (specpow != null) material.SpecularPower = specpow.Value;

                var alpha = collada.Materials[i].Properties.OfType<Opacity>().FirstOrDefault();
                if (alpha != null) material.Alpha = alpha.Value;

                var emissive = collada.Materials[i].Properties.OfType<EmissiveColor>().FirstOrDefault();
                if (emissive != null) material.EmissiveColor = emissive.Color.ToVector3();                                       

                materials.Add(material.Name, material);
            }            
        }
            
        /// <summary>
        /// Creates MeshContent instances for each mesh in the COLLADA model and attaches
        /// them to the NodeContent root.
        /// </summary>
        void CreateMeshes()
        {
            foreach (Mesh mesh in collada.Meshes)
            {
                foreach (MeshPart part in mesh.MeshParts)
                {
                    meshBuilder = MeshBuilder.StartMesh(mesh.Name);
                    meshBuilder.SwapWindingOrder = false;
                    meshBuilder.MergeDuplicatePositions = false;                    
                    meshBuilder.SetMaterial(materials[part.MaterialName]);                  

                    // Positions
                    CVertexChannel posChannel = part.Vertices.VertexChannels.Where(c =>
                            c.Description.VertexElementUsage == VertexElementUsage.Position).
                            FirstOrDefault();

                    VertexContainer container = part.Vertices;
                    float[] data = container.Vertices;
                    int posOffset = posChannel.Source.Offset;                    

                    for (int i = 0; i < container.Vertices.Length; i += container.VertexSize)
                    {                        
                        Vector3 pos = new Vector3(data[i + posOffset + 0], 
                            data[i + posOffset + 1], data[i + posOffset + 2]);
                        meshBuilder.CreatePosition(pos);
                    }

                    // Vertex channels other than position
                    List<XnaVertexChannel> channels = new List<XnaVertexChannel>();
                    foreach (CVertexChannel cvChannel in part.Vertices.VertexChannels)
                        if (cvChannel.Description.VertexElementUsage != VertexElementUsage.Position)
                            channels.Add(new XnaVertexChannel(meshBuilder, cvChannel));

                    // Triangles
                    for (int i = 0; i < part.Indices.Length; i += 3)
                    {
                        for (int j = i; j < i + 3; j++)
                        {                            
                            // Set channel components (other than position)
                            foreach (var channel in channels)                            
                                channel.SetData(j);                            

                            meshBuilder.AddTriangleVertex(part.Indices[j]);            
                        }
                    }

                    MeshContent meshContent = meshBuilder.FinishMesh();
                    rootNode.Children.Add(meshContent);
                }
            }            
        }
    }

    /// <summary>
    /// Helper class for creating vertex channels with MeshBuilder
    /// from Collada VertexChannels.
    /// </summary>
    class XnaVertexChannel
    {
        private MeshBuilder _meshBuilder;
        private CVertexChannel _colladaVertexChannel;
        private int _channelIndex;
        private int _vertexSize;
        private int _offset;        

        /// <summary>
        /// Creates a new vertex channel using the given MeshBuilder based on a
        /// Collada VertexChannel. No data is initially added to the vertex channel!
        /// For this, use the SetData method.
        /// </summary>
        /// <param name="meshBuilder">MeshBuilder instance to store vertex channel in</param>
        /// <param name="colladaVertexChannel">Original COLLADA Vertex Channel</param>
        public XnaVertexChannel(MeshBuilder meshBuilder, CVertexChannel colladaVertexChannel)
        {
            _colladaVertexChannel = colladaVertexChannel;
            _meshBuilder = meshBuilder;
            _vertexSize = colladaVertexChannel.Source.Stride;
            _offset = colladaVertexChannel.Source.Offset;

            Create();
        }

        /// <summary>
        /// Creates the vertex channel using the MeshBuilder (no data is added yet).
        /// </summary>
        protected void Create()
        {
            var usage = _colladaVertexChannel.Description.VertexElementUsage;
            int usageIndex = _colladaVertexChannel.Description.UsageIndex;

            String usageString = VertexChannelNames.EncodeName(usage, usageIndex);

            switch (_colladaVertexChannel.Description.VertexElementFormat)
            {
                case VertexElementFormat.Vector4:
                    _channelIndex = _meshBuilder.CreateVertexChannel<Vector4>(usageString);
                    break;

                case VertexElementFormat.Vector3:
                    _channelIndex = _meshBuilder.CreateVertexChannel<Vector3>(usageString);
                    break;

                case VertexElementFormat.Vector2:
                    _channelIndex = _meshBuilder.CreateVertexChannel<Vector2>(usageString);
                    break;

                case VertexElementFormat.Single:
                    _channelIndex = _meshBuilder.CreateVertexChannel<Single>(usageString);
                    break;

                default:
                    throw new Exception("Unexpected vertex element format");
            }
        }

        /// <summary>
        /// Sets vertex channel data for the current vertex (controlled externally through the
        /// MeshBuilder instance) to the value found at given index in the original COLLADA
        /// Vertex Channel.
        /// </summary>
        /// <param name="index">Index of element in the original COLLADA Vertex Channel</param>
        public void SetData(int index)
        {
            int i = _colladaVertexChannel.Indices[index] * _vertexSize + _offset;
            float[] data = _colladaVertexChannel.Source.Data;

            object value;

            switch (_colladaVertexChannel.Description.VertexElementFormat)
            {
                case VertexElementFormat.Vector4:
                    value = new Vector4(data[i], data[i + 1], data[i + 2], data[i + 3]);
                    break;

                case VertexElementFormat.Vector3:
                    value = new Vector3(data[i], data[i + 1], data[i + 2]);
                    break;

                case VertexElementFormat.Vector2:
                    value = new Vector2(data[i], data[i + 1]);
                    break;

                case VertexElementFormat.Single:
                    value = data[i];
                    break;

                default:
                    throw new Exception("Unexpected vertex element format");
            }

            _meshBuilder.SetVertexChannelData(_channelIndex, value);            
        }
    }
}