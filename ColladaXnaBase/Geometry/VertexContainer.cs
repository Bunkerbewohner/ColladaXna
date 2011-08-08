using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

namespace ColladaXna.Base.Geometry
{
    /// <summary>
    /// Flexible container for vertex data
    /// </summary>
    public class VertexContainer
    {
        //=====================================================================
        #region Private fields

        // Individual vertex channels in this container
        [ContentSerializer]
        List<VertexChannel> _vertexChannels = new List<VertexChannel>();

        // Float data stream which can be used to create a vertex buffer
        float[] _data;

        // Indices into the data stream 
        int[] _indices;

        // Number of Floats used per Vertex
        int _vertexSize;

        #endregion

        //=====================================================================
        #region Public Methods and Properties

        /// <summary>
        /// Vertex channels within this container
        /// </summary>
        public List<VertexChannel> VertexChannels { get { return _vertexChannels; } }

        /// <summary>
        /// Raw vertex buffer containing the float data stream of vertices
        /// </summary>
        public float[] Vertices { get { return _data; } }

        /// <summary>
        /// Index Buffer of this vertex container, referencing to the vertex buffer float array
        /// </summary>
        public int[] Indices { get { return _indices; } }

        /// <summary>
        /// Vertex Size in number of floats (multiply by sizeof(float) to get size in bytes)
        /// </summary>
        public int VertexSize { get { return _vertexSize; } }

        /// <summary>
        /// Determines whether this container contains given element type
        /// </summary>
        /// <param name="element">element usage</param>
        /// <returns>True if a vertex channel is contained fitting the element usage</returns>
        public bool HasElement(VertexElementUsage element)
        {
            return (from channel in VertexChannels where channel.Description.VertexElementUsage == element select channel).Any();
        }

        #endregion       

        /// <summary>
        /// Creates a vertex container from a set of "raw" vertex channels as read from the COLLADA file.
        /// Hence, it is assumed that each channel uses its own source (rather than every channel using
        /// the same single source). 
        /// </summary>
        /// <param name="inputChannels">Original Input Channels from COLLADA file</param>
        public VertexContainer(List<VertexChannel> inputChannels)
        {            
            // Check for basic requirements
            if (inputChannels.Any(c => c.Description.VertexElementUsage == VertexElementUsage.Position) == false)
                throw new ArgumentException("Geometry has not all needed information. At least Positions are necessary!");

            // Convert Colors to single values, if necessary
            ConvertColorChannels(inputChannels);

            // Number of floats per vertex
            _vertexSize = CalculateVertexSize(inputChannels);
            
            // Expected number of indices
            int numIndices = inputChannels.First().Indices.Length;

            // vertex buffer with an expected number of 3/4 of the number of indices
            List<float> vbuffer = new List<float>(numIndices * 3 / 4);

            // Remember the position of distinct vertices to avoid duplicates
            Dictionary<VertexKey, int> usedVertices = new Dictionary<VertexKey, int>(numIndices * 3 / 4);

            // Indices referencing the new vertex buffer (vbuffer)
            List<int> indexList = new List<int>(numIndices);

            // Go through all indices to create vertices
            for (int i = 0; i < numIndices; i++)
            {
                VertexKey key = new VertexKey(inputChannels, i);
                int usedIndex = 0;

                if (usedVertices.TryGetValue(key, out usedIndex))
                {
                    // This vertex was already used, its index is "usedIndex"
                    indexList.Add(usedIndex);
                }
                else
                {
                    // If the vertex is unknown, add it to the vertex container (channel-wise)
                    // and remember that is has been used and the corresponding index
                    int index = vbuffer.Count / _vertexSize;

                    // Add all elements of the current vertex to the vertex buffer
                    foreach (VertexChannel channel in inputChannels)
                    {
                        float[] elementData = new float[channel.Source.Stride];
                        channel.GetValue(i, ref elementData, 0);

                        // origin of texture coordinates in XNA is top left, while 
                        // in COLLADA it is bottom left. Therefore they need to be
                        // converted here
                        if (channel.Description.VertexElementUsage == VertexElementUsage.TextureCoordinate)
                        {
                            elementData[1] = 1 - elementData[1];
                        }

                        vbuffer.AddRange(elementData);
                    }                                                                                

                    // Remember that this vertex combination was used before
                    // and store the index where it can be found in the 
                    // vertex container
                    usedVertices.Add(key, index);

                    // Add reference to the just created vertex to the index list / buffer
                    indexList.Add(index);
                }
            }

            // Create adequate vertex channels
            int offset = 0;

            foreach (VertexChannel inputChannel in inputChannels)
            {
                VertexSource newSource = new VertexSource()
                {                                        
                    Offset = offset // the element-offset within the vertex buffer
                };

                VertexElement desc = new VertexElement(offset, inputChannel.Description.VertexElementFormat, 
                    inputChannel.Description.VertexElementUsage, inputChannel.Description.UsageIndex);

                VertexChannel newChannel = new VertexChannel(newSource, desc);
                _vertexChannels.Add(newChannel);

                offset += inputChannel.Source.Stride;
            }

            // Swap winding order
            for (int i = 0; i < indexList.Count; i += 3)
            {
                int swap = indexList[i + 1];
                indexList[i + 1] = indexList[i + 2];
                indexList[i + 2] = swap;
            }

            _data = vbuffer.ToArray();
            _indices = indexList.ToArray();            

            // Update Source Data reference off all vertex channels 
            foreach (VertexChannel channel in _vertexChannels)
            {
                // Every channel uses the same source now (global vertex buffer)
                channel.Source.Data = _data;

                // Every channel also uses the same indices
                channel.Indices = _indices;

                // The stride of one entry containing all elements for one vertex
                channel.Source.Stride = _vertexSize;                
            }
        }

        /// <summary>
        /// Converts input channels that are used for colors from Vector3/Vector4
        /// to single float format, if necessary.
        /// </summary>
        /// <param name="inputChannels">Vertex Channels as read from COLLADA file</param>
        private void ConvertColorChannels(List<VertexChannel> inputChannels)
        {
            foreach (VertexChannel channel in inputChannels)
            {
                var usage = channel.Description.VertexElementUsage;
                var format = channel.Description.VertexElementFormat;

                if (usage != VertexElementUsage.Color) continue; // only relevant for colors
                if (format == VertexElementFormat.Single) continue; // nothing to do

                // Create updated vertex element description where each element is a single
                VertexElement newDesc = new VertexElement()
                {
                    Offset = 0,
                    UsageIndex = channel.Description.UsageIndex,
                    VertexElementFormat = VertexElementFormat.Single,
                    VertexElementUsage = VertexElementUsage.Color
                };

                // Old stride is 3 or 4 (corresponding to Vector3 or Vector4)
                int oldStride = channel.Source.Stride;
                float[] oldData = channel.Source.Data;

                // Create new source where each color is only represented by one single
                VertexSource newSource = new VertexSource();
                newSource.Stride = 1; // one float per color                                
                newSource.Data = new float[oldData.Length / oldStride];

                for (int i = 0; i < newSource.Data.Length; i++)
                {
                    // project start index to old data set (with $oldStride components per color)
                    int j = i * oldStride;

                    // Construct color from three or four floats in range [0,1]
                    Color color = (oldStride == 3) ?
                        new Color(oldData[j + 0], oldData[j + 1], oldData[j + 2]) :
                        new Color(oldData[j + 0], oldData[j + 1], oldData[j + 2], oldData[j + 3]);

                    // Transform the 4 color bytes to a float
                    // The resulting float might not be a valid float number; but only the bytes
                    // are important for usage later on the graphics card
                    byte[] bytes = BitConverter.GetBytes(((Color)color).PackedValue);
                    float colorFloat = BitConverter.ToSingle(bytes, 0);

                    newSource.Data[i] = colorFloat;
                }

                // Update description and source of channel
                channel.Description = newDesc;
                channel.Source = newSource;
            }
        }

        //=====================================================================
        #region Vertex Buffer creation for run-time representation

        /// <summary>
        /// Calculates the size of a complete vertex in number of floats used
        /// </summary>
        /// <returns></returns>
        protected int CalculateVertexSize(List<VertexChannel> channels = null)
        {
            int combinedSize = 0;
            if (channels == null) channels = _vertexChannels;

            foreach (VertexChannel channel in channels)
            {
                combinedSize += channel.Source.Stride;                
            }

            return combinedSize;
        }

        /// <summary>
        /// Dynamically creates a vertex declaration that fits the data contained by this
        /// vertex container.
        /// </summary>        
        /// <returns>A fitting vertex declaration</returns>
        public VertexDeclaration CreateVertexDeclaration()
        {
            VertexElement[] elements = new VertexElement[_vertexChannels.Count];
            short offset = 0;
            int index = 0;

            foreach (VertexChannel channel in _vertexChannels)
            {
                VertexElement element = channel.Description;
                element.Offset = offset;                               

                offset += (short)(channel.Source.Stride * sizeof(float));

                elements[index++] = element;
            }

            return new VertexDeclaration(elements);            
        }

        /// <summary>
        /// Creates a vertex buffer from this vertex container
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="vertexDeclaration"></param>
        public VertexBuffer CreateVertexBuffer(GraphicsDevice graphicsDevice)
        {            
            var vertexDeclaration = CreateVertexDeclaration();

            var vertexBuffer = new VertexBuffer(graphicsDevice, vertexDeclaration, _data.Length, BufferUsage.WriteOnly);            
            vertexBuffer.SetData<float>(_data);

            return vertexBuffer;
        }

        #endregion
    }

    class VertexChannels
    {
        List<VertexChannel> _channels;
        Dictionary<VertexElementUsage, int> _order;

        public VertexChannels(List<VertexChannel> channels)
        {
            _channels = channels;
            _order = new Dictionary<VertexElementUsage, int>();

            for (int i = 0; i < channels.Count; i++)
                _order.Add(_channels[i].Description.VertexElementUsage, i);
        }

        public VertexChannel this[VertexElementUsage elementUsage]
        {
            get
            {
                return _channels[_order[elementUsage]];
            }
        }

        public int GetIndex(VertexElementUsage elementUsage)
        {
            int index = -1;

            _order.TryGetValue(elementUsage, out index);

            return index;
        }
    }

    /// <summary>
    /// Vertex key to identify an unique vertex by the combination
    /// of its used position, normal, tangent and texture coordinate
    /// </summary>
    internal class VertexKey
    {
        int SuperIndex;
        VertexChannels VertexChannels;
        
        public VertexKey(List<VertexChannel> vertexChannels, int superIndex)
        {
            SuperIndex = superIndex;
            VertexChannels = new VertexChannels(vertexChannels);
        }        

        /// <summary>
        /// Determines whether two vertex keys are the same, that is they are both
        /// referring to the exact same vertex through their indices.
        /// </summary>
        /// <param name="obj">Another vertex key</param>
        /// <returns>True if both vertex keys refer to the same vertex</returns>
        public override bool Equals(object obj)
        {
            if (obj is VertexKey)
            {
                VertexKey other = (VertexKey) obj;
                var a = this.VertexChannels;
                var b = other.VertexChannels;
                int i = SuperIndex;
                int j = other.SuperIndex;
                var pos = VertexElementUsage.Position;
                var normal = VertexElementUsage.Normal;
                var tex = VertexElementUsage.TextureCoordinate;
                var tangent = VertexElementUsage.Tangent;
                var color = VertexElementUsage.Color;

                return a == b &&
                       a[color].Indices[i] == b[color].Indices[j] &&
                       a[pos].Indices[i] == b[pos].Indices[j] &&
                       a[normal].Indices[i] == b[normal].Indices[j] &&
                       a[tex].Indices[i] == b[tex].Indices[j] &&
                       a[tangent].Indices[i] == b[tangent].Indices[j];
                        
                // Note: Joint Indices/Weights are always the same for one position, 
                // no need to compare                       
            }
            
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // Position hash will vary the most
            return VertexChannels[VertexElementUsage.Position].Indices[SuperIndex].GetHashCode();
        }
    }
}
