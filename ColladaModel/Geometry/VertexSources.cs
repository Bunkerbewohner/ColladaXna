using Microsoft.Xna.Framework;

namespace Omi.Xna.Collada.Model.Geometry
{
    /// <summary>
    /// Vertex sources corresponding to sources in the COLLADA file
    /// </summary>
    public class VertexSources
    {
        /// <summary>
        /// Distinct positions
        /// </summary>
        public Vector3[] Positions;

        /// <summary>
        /// Distinct colors
        /// </summary>
        public Color[] Colors;

        /// <summary>
        /// Distinct normals
        /// </summary>
        public Vector3[] Normals;

        /// <summary>
        /// Distinct tangents
        /// </summary>
        public Vector3[] Tangents;

        /// <summary>
        /// Distinct Binormals
        /// </summary>
        public Vector3[] Binormals;

        /// <summary>
        /// Distinct texture coordinates
        /// </summary>
        public Vector2[] TexCoords;

        /// <summary>
        /// Up to four joint indices.         
        /// </summary>
        public Vector4[] JointIndices;

        /// <summary>
        /// Normalized weights for up to
        /// four joints. Fourth weight is implicitly
        /// defined as (1 - X - Y - Z).
        /// A weight of 0 means that no joint is used.
        /// </summary>
        public Vector3[] JointWeights;
    }
}