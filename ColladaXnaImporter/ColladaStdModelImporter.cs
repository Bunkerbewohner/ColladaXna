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

namespace ColladaXnaImporter
{
    using CVertexChannel = ColladaXna.Base.Geometry.VertexChannel;
    using ColladaXna.Base.Materials;
    using System.Diagnostics;

    /// <summary>
    /// This class imports a COLLADA ".dae" file into the XNA default content model for models.
    /// As a result models imported with this class can be processed and loaded with all the
    /// default content processors just like FBX models.
    /// </summary>
    [ContentImporter(".dae", CacheImportedData = false, DisplayName="COLLADA Standard Importer", DefaultProcessor="ModelProcessor")]
    public class ColladaStdModelImporter : ContentImporter<NodeContent>
    {
        ColladaModel collada;

        ContentImporterContext importerContext;

        NodeContent rootNode;

        MeshBuilder meshBuilder;

        Dictionary<String, MaterialContent> materials;

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

                    bool normals = part.Vertices.VertexChannels.Any(c =>
                        c.Description.VertexElementUsage == VertexElementUsage.Normal);

                    bool texcoords = part.Vertices.VertexChannels.Any(c =>
                        c.Description.VertexElementUsage == VertexElementUsage.TextureCoordinate);

                    int normalIndex = !normals ? 0 :
                        meshBuilder.CreateVertexChannel<Vector3>(VertexChannelNames.Normal());

                    int texCoordIndex = !texcoords ? 0 :
                        meshBuilder.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(0));

                    // Positions
                    CVertexChannel posChannel = part.Vertices.VertexChannels.Where(c =>
                            c.Description.VertexElementUsage == VertexElementUsage.Position).
                            FirstOrDefault();

                    // Normals?
                    CVertexChannel normalChannel = part.Vertices.VertexChannels.Where(c =>
                            c.Description.VertexElementUsage == VertexElementUsage.Normal).
                            FirstOrDefault();

                    CVertexChannel texCoordChannel = part.Vertices.VertexChannels.Where(c =>
                            c.Description.VertexElementUsage == VertexElementUsage.TextureCoordinate).
                            FirstOrDefault();

                    VertexContainer container = part.Vertices;
                    float[] data = container.Vertices;

                    int posOffset = posChannel.Source.Offset;
                    int normalOffset = normalChannel.Source.Offset;   
                    int texOffset = texCoordChannel.Source.Offset;

                    for (int i = 0; i < container.Vertices.Length; i += container.VertexSize)
                    {                        
                        Vector3 pos = new Vector3(data[i + posOffset + 0], 
                            data[i + posOffset + 1], data[i + posOffset + 2]);
                        meshBuilder.CreatePosition(pos);
                    }

                    // Triangles
                    for (int i = 0; i < part.Indices.Length; i += 3)
                    {
                        for (int j = i; j < i + 3; j++)
                        {
                            int k = part.Indices[j] * container.VertexSize;                                            

                            if (normals)
                            {
                                Vector3 normal = new Vector3(data[k + normalOffset + 0],
                                    data[k + normalOffset + 1], data[k + normalOffset + 2]);

                                meshBuilder.SetVertexChannelData(normalIndex, normal);
                            }

                            if (texcoords)
                            {
                                // Y axis of texture coordinates in collada is inverse of that in XNA
                                Vector2 coord = new Vector2(data[k + texOffset + 0],
                                    1 - data[k + texOffset + 1]);

                                meshBuilder.SetVertexChannelData(texCoordIndex, coord);
                            }

                            meshBuilder.AddTriangleVertex(part.Indices[j]);            
                        }
                    }

                    MeshContent meshContent = meshBuilder.FinishMesh();
                    rootNode.Children.Add(meshContent);
                }
            }            
        }
    }
}