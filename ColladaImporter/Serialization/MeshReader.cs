using Microsoft.Xna.Framework.Content;
using Seafarer.Xna.Collada.Importer.Geometry;
// TODO: replace this with the type you want to read.

namespace Seafarer.Xna.Collada.Importer.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class MeshReader : ContentTypeReader<Mesh>
    {
        protected override Mesh Read(ContentReader input, Mesh existingInstance)
        {
            Mesh mesh = new Mesh();
            mesh.Name = input.ReadString();

            mesh.VertexContainers = input.ReadObject<VertexContainer[]>();

            int numParts = input.ReadInt32();

            mesh.MeshParts = new MeshPart[numParts];

            for (int i = 0; i < numParts; i++)
            {
                mesh.MeshParts[i] = new MeshPart();
                mesh.MeshParts[i].MaterialName = input.ReadString();
                mesh.MeshParts[i].Indices = input.ReadObject<int[]>();

                int vertexContainerIndex = input.ReadInt32();
                mesh.MeshParts[i].Vertices = mesh.VertexContainers[vertexContainerIndex];
            }

            return mesh;
        }
    }
}
