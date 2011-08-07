using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ColladaXna.Base.Geometry
{
    /// <summary>
    /// A stream of vertex elements for the rendering pipeline
    /// </summary>
    public class VertexChannel
    {
        /// <summary>
        /// Underlying vertex source containing all unique values (independent of element type)
        /// </summary>
        public VertexSource Source;

        /// <summary>
        /// Description of the elements of this channel
        /// </summary>
        public VertexElement Description;

        /// <summary>
        /// Indices of this channel's elements refering into the associated vertex source.
        /// These indices are only local to the referenced vertex source.
        /// Without indices it is assumed, that all elements are given in propery order
        /// within the vertex source.
        /// </summary>
        public int[] Indices;

        /// <summary>
        /// Creates a new Vertex Channel
        /// </summary>
        /// <param name="source">Used vertex source</param>
        /// <param name="description">Vertex element description</param>
        /// <param name="indices">Indices</param>
        public VertexChannel(VertexSource source, VertexElement description)
        {
            Source = source;
            Description = description;            
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
        public void CopyIndices(int[] sourceIndices, int sourceOffset, int count)
        {
            Indices = new int[count];

            if (sourceOffset >= 0)
            {
                int stride = sourceIndices.Length / count;

                for (int i = 0, j = sourceOffset; i < count; i++, j += stride)
                {
                    Indices[i] = sourceIndices[j];
                }
            }
        }

        public void GetValue(int atIndex, ref float[] dest, int destIndex)
        {
            Array.Copy(Source.Data, Indices[atIndex] * Source.Stride, dest, destIndex, Source.Stride);
        }        

        public void GetValue(int atIndex, out Vector3 result)
        {
            result = new Vector3(Source.Data[Indices[atIndex] + 0],
                                 Source.Data[Indices[atIndex] + 1],
                                 Source.Data[Indices[atIndex] + 2]);
        }
    }
}
