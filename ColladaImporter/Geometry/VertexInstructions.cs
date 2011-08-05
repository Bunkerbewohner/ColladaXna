
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Seafarer.Xna.Collada.Importer.Geometry
{
    /// <summary>
    /// Instructions for building a vertex buffer.
    /// This class holds a reference to a vertex source,
    /// which contains vertex channels (Positions, Normals, etc.)
    /// and index lists which in conjunction can be used to
    /// construct a vertex buffer and index buffer.
    /// </summary>
    public class VertexInstructions
    {
        /// <summary>
        /// Vertex source containing all distinct vertex components
        /// which can be referenced by the corresponding indices.
        /// </summary>
        public VertexSources Source;

        /// <summary>
        /// Position indices refering to Source
        /// </summary>
        public int[] PositionIndices;

        /// <summary>
        /// Normal indices refering to Source
        /// May be null or empty
        /// </summary>
        public int[] NormalIndices;

        /// <summary>
        /// Tangent indices refering to Source
        /// May be null or empty
        /// </summary>
        public int[] TangentIndices;

        /// <summary>
        /// Texture coordinate indices refering to Source
        /// </summary>
        public int[] TexCoordIndices;

        /// <summary>
        /// Indices of joint indices refering to Source
        /// </summary>
        public int[] JointIndices;

        /// <summary>
        /// Indices of joint weights refering to Source
        /// </summary>
        public int[] JointWeightIndices;

        /// <summary>
        /// Is true if there are position indices
        /// </summary>
        public bool HasPositions
        {
            get { return PositionIndices != null && PositionIndices.Length > 0; }
        }

        /// <summary>
        /// Is true if there are normal indices
        /// </summary>
        public bool HasNormals
        {
            get { return NormalIndices != null && NormalIndices.Length > 0; }
        }

        /// <summary>
        /// Is true if there are tangent indices
        /// </summary>
        public bool HasTangents
        {
            get { return TangentIndices != null && TangentIndices.Length > 0; }
        }

        /// <summary>
        /// Is true if there are texture coordinate indices
        /// </summary>
        public bool HasTextureCoordinates
        {
            get { return TexCoordIndices != null && TexCoordIndices.Length > 0; }
        }

        /// <summary>
        /// Is true if there are joints defined
        /// </summary>
        public bool HasJoints
        {
            get 
            { 
                return Source.JointWeights.Length > 0 && Source.JointIndices.Length > 0 &&
                       JointWeightIndices != null && JointWeightIndices.Length > 0 &&
                       JointIndices != null && JointIndices.Length > 0; 
            }
        }

        /// <summary>
        /// The number of described vertices. This is equivalent to the number
        /// of indices
        /// </summary>
        public int NumVertices { get { return PositionIndices.Length; } }

        /// <summary>
        /// Creates a new VertexInstructions instance with given vertex source
        /// and initially no indices
        /// </summary>
        /// <param name="dataSource"></param>
        public VertexInstructions(VertexSources dataSource)
        {
            Source = dataSource;
        }                

        /// <summary>
        /// Copies position indices into the instruction set.
        /// The source indices array must be the array of all indices
        /// combined just as found in the COLLADA file (&lt;p&gt).
        /// </summary>
        /// <param name="sourceIndices">Index list as in COLLADA file</param>
        /// <param name="sourceOffset"></param>
        /// <param name="count"></param>
        public void CopyPositions(int[] sourceIndices, int sourceOffset, int count)
        {
            CopyIndices(sourceIndices, sourceOffset, out PositionIndices, count);
        }

        /// <summary>
        /// Copies normal indices into the instruction set.
        /// The source indices array must be the array of all indices
        /// combined just as found in the COLLADA file (&lt;p&gt).
        /// </summary>
        /// <param name="sourceIndices">Index list as in COLLADA file</param>
        /// <param name="sourceOffset"></param>
        /// <param name="count"></param>
        public void CopyNormals(int[] sourceIndices, int sourceOffset, int count)
        {
            CopyIndices(sourceIndices, sourceOffset, out NormalIndices, count);
        }

        /// <summary>
        /// Copies tangent indices into the instruction set.
        /// The source indices array must be the array of all indices
        /// combined just as found in the COLLADA file (&lt;p&gt).
        /// </summary>
        /// <param name="sourceIndices">Index list as in COLLADA file</param>
        /// <param name="sourceOffset"></param>
        /// <param name="count"></param>
        public void CopyTangents(int[] sourceIndices, int sourceOffset, int count)
        {
            CopyIndices(sourceIndices, sourceOffset, out TangentIndices, count);
        }

        /// <summary>
        /// Copies texture coordinate indices into the instruction set.
        /// The source indices array must be the array of all indices
        /// combined just as found in the COLLADA file (&lt;p&gt).
        /// </summary>
        /// <param name="sourceIndices">Index list as in COLLADA file</param>
        /// <param name="sourceOffset"></param>
        /// <param name="count"></param>
        public void CopyTexCoords(int[] sourceIndices, int sourceOffset, int count)
        {
            CopyIndices(sourceIndices, sourceOffset, out TexCoordIndices, count);
        }

        /// <summary>
        /// Generates the indices of joint indices and joint weights
        /// according to the position indices, since these indices
        /// are congruent. If the source data contains no joint 
        /// information empty arrays are assigned.
        /// </summary>
        public void GenerateJointIndicesAndWeights()
        {
            if (Source.JointIndices.Length == 0)
            {
                JointIndices = new int[0];
                JointWeightIndices = new int[0];
            }

            JointIndices = new int[PositionIndices.Length];
            JointWeightIndices = new int[PositionIndices.Length];

            Array.Copy(PositionIndices, JointIndices, PositionIndices.Length);
            Array.Copy(PositionIndices, JointWeightIndices, PositionIndices.Length);
        }

        /// <summary>
        /// Copies indexStream from source to dest if the sourceOffset >= 0.
        /// </summary>
        /// <param name="source">Source array of all indices</param>
        /// <param name="sourceOffset">Offset within source array for each index</param>
        /// <param name="dest">Destination array for chosen indexStream</param>
        /// <param name="count">Number of indices to copy</param>
        protected void CopyIndices(int[] source, int sourceOffset, out int[] dest, int count)
        {
            if (sourceOffset < 0)
            {
                dest = new int[0];
            }
            else
            {
                dest = new int[count];

                int stride = source.Length / count;

                for (int i = 0, j = sourceOffset; i < count; i++, j += stride)
                {
                    dest[i] = source[j];
                }
            }
        }

        /// <summary>
        /// Returns the position from the vertex source referenced
        /// by the i-th position index (PositionIndices[i]).
        /// If no position indices were defined a Vector3 containing
        /// only NaN is returned.
        /// </summary>
        /// <param name="i">index of the index in PositionIndices</param>
        /// <returns>The referenced vector or a Vector3 of NaNs</returns>
        public Vector3 FetchPosition(int i)
        {
            if (Source.Positions == null || PositionIndices == null)
                return new Vector3(float.NaN, float.NaN, float.NaN);
            else
            {
                return Source.Positions[PositionIndices[i]];
            }
        }

        /// <summary>
        /// Returns the normal from the vertex source referenced
        /// by the i-th normal index (NormalIndices[i]).
        /// If no position indices were defined a Vector3 containing
        /// only NaN is returned.
        /// </summary>
        /// <param name="i">index of the index in NormalIndices</param>
        /// <returns>The referenced normal or a Vector3 of NaNs</returns>
        public Vector3 FetchNormal(int i)
        {
            if (Source.Normals == null || NormalIndices == null)
                return new Vector3(float.NaN, float.NaN, float.NaN);
            else
                return Source.Normals[NormalIndices[i]];
        }

        /// <summary>
        /// Returns the tangent from the vertex source referenced
        /// by the i-th tangent index (TangentIndices[i]).
        /// If no tangent indices were defined a Vector3 containing
        /// only NaN is returned.
        /// </summary>
        /// <param name="i">index of the index in TangentIndices</param>
        /// <returns>The referenced tangent or a Vector3 of NaNs</returns>
        public Vector3 FetchTangent(int i)
        {
            if (Source.Tangents == null || TangentIndices == null)
                return new Vector3(float.NaN, float.NaN, float.NaN);
            else
                return Source.Tangents[TangentIndices[i]];
        }

        /// <summary>
        /// Returns the texture coordinate from the vertex source referenced
        /// by the i-th texture coordinate index (TexCoordIndices[i]).
        /// If no texture coordinate indices were defined a Vector2 containing
        /// only NaN is returned.
        /// </summary>
        /// <param name="i">index of the index in TexCoordIndices</param>
        /// <returns>The referenced texture coordinate or a Vector2 of NaNs</returns>
        public Vector2 FetchTextureCoordinate(int i)
        {
            if (Source.TexCoords == null || TexCoordIndices == null)
                return new Vector2(float.NaN, float.NaN);
            else
                return Source.TexCoords[TexCoordIndices[i]];
        }

        /// <summary>
        /// Returns the joint indices from the vertex source referenced
        /// by the i-th joint indices index (JointIndices[i]).
        /// If no indices of joint indices were defined a Vector4 containing
        /// only NaN is returned.
        /// </summary>
        /// <param name="i">index of the index in JointIndices</param>
        /// <returns>The referenced joint indices or a Vector4 of NaNs</returns>
        public Vector4 FetchJointIndices(int i)
        {
            if (Source.JointIndices == null || JointIndices == null)
                return new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
            else
            {
                CheckRange(JointIndices, i);
                CheckRange(Source.JointIndices, JointIndices[i]);
                return Source.JointIndices[JointIndices[i]];
            }
        }

        /// <summary>
        /// Returns the joint weights from the vertex source referenced
        /// by the i-th joint weight index (JointWeightIndices[i]).
        /// If no joint weight indices were defined a Vector3 containing
        /// only NaN is returned.
        /// </summary>
        /// <param name="i">index of the index in JointWeightIndices</param>
        /// <returns>The referenced joint weights or a Vector3 of NaNs</returns>
        public Vector3 FetchJointWeights(int i)
        {
            if (Source.JointWeights == null || JointWeightIndices == null)
                return new Vector3(float.NaN, float.NaN, float.NaN);
            else
            {
                CheckRange(JointWeightIndices, i);
                CheckRange(Source.JointWeights, JointWeightIndices[i]);
                return Source.JointWeights[JointWeightIndices[i]];
            }
        }

        [Conditional("DEBUG")]
        void CheckRange<T>(T[] array, int index)
        {
            if (index < 0 || index >= array.Length)
                throw new IndexOutOfRangeException("Invalid index " + index + " in array " + 
                    "with " + array.Length + " elements");
        }
    }
}