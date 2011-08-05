
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace ColladaXna.Base.Geometry
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
        /// Indices for different types of data
        /// </summary>
        public Dictionary<VertexDataType, int[]> Indices;       

        /// <summary>
        /// Determines whether this set of vertex instructions
        /// contains indices for the given vertex data type.
        /// </summary>
        /// <param name="type">Type of vertex data (Position, Color, ...)</param>
        /// <returns>True, if this type of vertex data is contained</returns>
        public bool Contains(VertexDataType type)
        {
            return Indices.ContainsKey(type) && Indices[type].Length > 0 && 
                   Source.Contains(type);
        }        

        /// <summary>
        /// The number of described vertices. This is equivalent to the number
        /// of indices
        /// </summary>
        public int NumVertices { get { return Indices[VertexDataType.Position].Length; } }

        /// <summary>
        /// Creates a new VertexInstructions instance with given vertex source
        /// and initially no indices
        /// </summary>
        /// <param name="dataSource"></param>
        public VertexInstructions(VertexSources dataSource)
        {
            Source = dataSource;
            Indices = new Dictionary<VertexDataType, int[]>();
        }                

        /// <summary>
        /// Copies indices for a given vertex data type into this vertex instruction set.
        /// The source array is assumed to be an array of consecutive indices like found
        /// in COLLADA files where every i-th entry is for some data type A (e.g. positions), 
        /// every (i+1)-th for B (e.g. color) etc.
        /// </summary>
        /// <param name="type">Type of vertex data that is referenced</param>
        /// <param name="source">Source array of all indices</param>
        /// <param name="sourceOffset">Element-wise offset within source array for each index</param>        
        /// <param name="count">Number of indices to copy</param>
        public void CopyIndices(VertexDataType type, int[] sourceIndices, int sourceOffset, int count)
        {
            if (!Indices.ContainsKey(type))
                Indices.Add(type, new int[count]);

            int[] dest = Indices[type];
            
            if (sourceOffset >= 0)
            {
                int stride = sourceIndices.Length / count;

                for (int i = 0, j = sourceOffset; i < count; i++, j += stride)
                {
                    dest[i] = sourceIndices[j];
                }
            }
        }        

        /// <summary>
        /// Generates the indices of joint indices and joint weights
        /// according to the position indices, since these indices
        /// are congruent. If the source data contains no joint 
        /// information this method has no effect.
        /// </summary>
        public void GenerateJointIndicesAndWeights()
        {
            if (!Source.Contains(VertexDataType.JointIndices))
            {
                return;
            }

            if (!Indices.ContainsKey(VertexDataType.Position))
            {
                throw new Exception("Position data is required to generate joint weights and indices");
            }

            Indices.Add(VertexDataType.JointIndices, new int[NumVertices]);
            Indices.Add(VertexDataType.JointWeights, new int[NumVertices]);

            Array.Copy(Indices[VertexDataType.Position], Indices[VertexDataType.JointIndices], NumVertices);
            Array.Copy(Indices[VertexDataType.Position], Indices[VertexDataType.JointWeights], NumVertices);
        }

        public T FetchData<T>(VertexDataType type, int i)
        {
            return (T)Source.GetElement(type, i);
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