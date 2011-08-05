using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

// TODO: replace this with the type you want to write out.
using TWrite = Seafarer.Xna.Collada.Importer.Geometry.Mesh;
using Seafarer.Xna.Collada.Importer.Geometry;

namespace Seafarer.Xna.Collada.Content.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class MeshWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {
            // 1. Name of the mesh
            output.Write(value.Name);

            // 2. Vertices
            output.WriteObject<VertexContainer[]>(value.VertexContainers);

            // 3. Mesh Parts
            WriteMeshParts(output, value);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Seafarer.Xna.Collada.Importer.Serialization.MeshReader, ColladaImporter";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Mesh).AssemblyQualifiedName;
        }

        void WriteMeshParts(ContentWriter output, Mesh mesh)
        {
            List<VertexContainer> vertexContainers = mesh.VertexContainers.ToList();

            // Anzahl Mesh Parts
            output.Write(mesh.MeshParts.Length);

            foreach (MeshPart part in mesh.MeshParts)
            {                
                output.Write(part.MaterialName);
                output.WriteObject<int[]>(part.Indices);

                // Only write the index of the used vertex container,
                // since MeshPart.Vertices is only a reference to 
                // an entry in Mesh.VertexContainers
                output.Write(vertexContainers.IndexOf(part.Vertices));
            }
        }
    }
}
