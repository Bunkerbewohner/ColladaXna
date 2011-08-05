using Microsoft.Xna.Framework;

namespace ColladaXna.Base.Geometry
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

        /// <summary>
        /// Determines whether this set of vertex sources contains given data type.
        /// </summary>
        /// <param name="type">Vertex data type</param>
        /// <returns>True if any data of given type is contained</returns>
        public bool Contains(VertexDataType type)
        {
            switch (type)
            {
                case VertexDataType.Position:
                    return Positions != null && Positions.Length > 0;
                case VertexDataType.Color:
                    return Colors != null && Colors.Length > 0;
                case VertexDataType.Normal:
                    return Normals != null && Normals.Length > 0;
                case VertexDataType.Tangent:
                    return Tangents != null && Tangents.Length > 0;
                case VertexDataType.Binormal:
                    return Binormals != null && Binormals.Length > 0;
                case VertexDataType.TexCoord:
                    return TexCoords != null && TexCoords.Length > 0;
                case VertexDataType.JointIndices:
                    return JointIndices != null && JointIndices.Length > 0;
                case VertexDataType.JointWeights:
                    return JointWeights != null && JointWeights.Length > 0;
                default: 
                    return false;
            }
        }

        public object GetElement(VertexDataType type, int i)
        {
            switch (type)
            {
                case VertexDataType.Position:
                    return Positions[i];
                case VertexDataType.Color:
                    return Colors[i];
                case VertexDataType.Normal:
                    return Normals[i];
                case VertexDataType.Tangent:
                    return Tangents[i];
                case VertexDataType.Binormal:
                    return Binormals[i];
                case VertexDataType.TexCoord:
                    return TexCoords[i];
                case VertexDataType.JointIndices:
                    return JointIndices[i];
                case VertexDataType.JointWeights:
                    return JointWeights[i];
                default:
                    return null;
            }
        }
    }
}