using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework.Graphics;

namespace Omi.Xna.Collada.Model
{
    using EffectMaterial = Omi.Xna.Collada.Model.Materials.EffectMaterial;

    public class ModelMeshPart
    {
        public VertexBuffer VertexBuffer { get; set; }
        public VertexDeclaration VertexDeclaration { get; set; }
        public int VertexStride { get; set; }
        public int NumVertices { get; set; }

        public IndexBuffer IndexBuffer { get; set; }

        public EffectMaterial Material { get; set; }        
    }
}
