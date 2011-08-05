using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ColladaXna.Base.Geometry
{
    /// <summary>
    /// Flexible container for vertex data
    /// </summary>
    public class VertexContainer
    {
        //=====================================================================
        #region Private fields

        [ContentSerializer]
        List<VertexChannel> _vertexChannels = new List<VertexChannel>();

        #endregion

        //=====================================================================
        #region Methods for accessing vertex channels

        public List<VertexChannel> VertexChannels { get { return _vertexChannels; } }

        public bool HasElement(VertexElementUsage element)
        {
            return (from channel in VertexChannels where channel.Description.VertexElementUsage == element select channel).Any();
        }

        #endregion

        //=====================================================================
        #region Methods for Adding vertex channels

        /// <summary>
        /// Adds a generic vertex channel according to the given element description
        /// and data.
        /// </summary>
        /// <param name="element">Element description</param>
        /// <param name="stride">Stride (number of floats per element)</param>
        /// <param name="data">Array containing all data (length must be a multiple of stride)</param>
        public void AddVertexData(VertexElement element, int stride, float[] data)
        {
            VertexSource source = new VertexSource();            
            source.Stride = stride;
            source.Data = data;

            VertexChannel channel = new VertexChannel(source, element);
            _vertexChannels.Add(channel);
        }        

        /// <summary>
        /// Adds positions to the vertex data. This assumes that data contains
        /// Vector3 elements. This is just a convenience method call for AddVertexData
        /// with a vertex element that fits 3d position vectors.
        /// </summary>
        /// <param name="data">Array holding positions with every three values representing one Vector3</param>
        public void AddPositions(float[] data)
        {            
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0);

            AddVertexData(element, 3, data);
        }

        public void AddPositions(Vector3[] positions)
        {
            float[] data = new float[positions.Length * 3];
            
            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                data[i] = positions[j].X;
                data[i + 1] = positions[j].Y;
                data[i + 2] = positions[j].Z;
            }

            AddPositions(data);
        }

        public void AddColors(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Color, VertexElementUsage.Color, 0);

            AddVertexData(element, 1, data);
        }

        public void AddColors(Color[] colors)
        {            
            var floats = (from color in colors select ColorToFloat(color)).ToArray();
            AddColors(floats);
        }

        float ColorToFloat(Color color)
        {
            byte[] bytes = BitConverter.GetBytes(color.PackedValue);
            return BitConverter.ToSingle(bytes, 0);
        }

        /// <summary>
        /// Adds normals to the vertex data. This assumes that data contains Vector3 elements.
        /// </summary>
        /// <param name="data"></param>
        public void AddNormals(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0);

            AddVertexData(element, 3, data);
        }

        public void AddNormals(Vector3[] normals)
        {
            float[] data = new float[normals.Length * 3];

            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                data[i] = normals[j].X;
                data[i + 1] = normals[j].Y;
                data[i + 2] = normals[j].Z;
            }

            AddNormals(data);
        }

        /// <summary>
        /// Adds texture coordinates to the vertex data. This assumes that data contains Vector2
        /// elements.
        /// </summary>
        /// <param name="data"></param>
        public void AddTextureCoordinates(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);

            AddVertexData(element, 2, data);
        }

        public void AddTextureCoordinates(Vector2[] coordinates)
        {
            float[] data = new float[coordinates.Length * 2];

            for (int i = 0, j = 0; i < data.Length; i += 2, j++)
            {
                data[i] = coordinates[j].X;
                data[i + 1] = coordinates[j].Y;                
            }

            AddTextureCoordinates(data);
        }

        /// <summary>
        /// Adds texture coordinates to the vertex data. This assumes that data contains Vector3
        /// elements.
        /// </summary>
        /// <param name="data"></param>
        public void AddTextureCoordinates3D(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0);

            AddVertexData(element, 3, data);
        }

        public void AddTextureCoordinates3D(Vector3[] coordinates)
        {
            float[] data = new float[coordinates.Length * 3];

            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                data[i] = coordinates[j].X;
                data[i + 1] = coordinates[j].Y;
                data[i + 2] = coordinates[j].Z;
            }

            AddTextureCoordinates3D(data);
        }

        /// <summary>
        /// Adds tangents to the vertex data. This assumes that data contains Vector3 elements.
        /// </summary>
        /// <param name="data"></param>
        public void AddTangents(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0);

            AddVertexData(element, 3, data);
        }

        public void AddTangents(Vector3[] tangents)
        {
            float[] data = new float[tangents.Length * 3];

            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                data[i] = tangents[j].X;
                data[i + 1] = tangents[j].Y;
                data[i + 2] = tangents[j].Z;
            }

            AddTangents(data);
        }

        /// <summary>
        /// Adds binormals to the vertex data. This assumes that data contains Vector3 elements.
        /// </summary>
        /// <param name="data"></param>
        public void AddBinormals(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector3, 
                VertexElementUsage.Binormal, 0);

            AddVertexData(element, 3, data);
        }

        public void AddBinormals(Vector3[] binormals)
        {
            float[] data = new float[binormals.Length * 3];

            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                data[i] = binormals[j].X;
                data[i + 1] = binormals[j].Y;
                data[i + 2] = binormals[j].Z;
            }

            AddBinormals(data);
        }

        public void AddJointIndices(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0);

            AddVertexData(element, 4, data);
        }

        public void AddJointIndices(Vector4[] indices)
        {
            float[] data = new float[indices.Length * 4];

            for (int i = 0, j = 0; i < data.Length; i += 4, j++)
            {
                data[i] = indices[j].X;
                data[i + 1] = indices[j].Y;
                data[i + 2] = indices[j].Z;
                data[i + 3] = indices[j].W;
            }

            AddJointIndices(data);
        }

        public void AddJointWeights(float[] data)
        {
            VertexElement element = new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.BlendWeight, 0);

            AddVertexData(element, 3, data);
        }

        public void AddJointWeights(Vector3[] weights)
        {
            float[] data = new float[weights.Length * 3];

            for (int i = 0, j = 0; i < data.Length; i += 3, j++)
            {
                data[i] = weights[j].X;
                data[i + 1] = weights[j].Y;
                data[i + 2] = weights[j].Z;                
            }

            AddJointWeights(data);
        }

        #endregion

        //=====================================================================
        #region Vertex Buffer creation for run-time representation

        /// <summary>
        /// Calculates the size of a complete vertex in number of floats used
        /// </summary>
        /// <returns></returns>
        protected int CalculateVertexSize()
        {
            int combinedSize = 0;

            foreach (VertexChannel channel in _vertexChannels)
            {
                combinedSize += channel.Source.Stride;
            }

            return combinedSize;
        }

        /// <summary>
        /// Combines all individual vertex channels into one vertex data stream
        /// which can be used to create a vertex buffer.
        /// </summary>
        /// <returns></returns>
        public float[] CreateDataStream()
        {
            // Size of an individual vertex in bytes
            int vertexSize = CalculateVertexSize();
            int offset = 0;

            // Number of vertices (all channels must have an equal number of items)
            int numVertices = _vertexChannels[0].Indices.Length;

            // Buffer holding all components of all vertices
            float[] buffer = new float[vertexSize * numVertices];

            // for each vertex
            for (int i = 0; i < numVertices; i++)
            {
                // write all channels for this vertex (Position, Normal etc.)
                foreach (VertexChannel c in _vertexChannels)
                {
                    // write components of the current channel
                    for (int k = 0; k < c.Source.Stride; k++)
                    {
                        buffer[offset++] = c.Source.Data[i * c.Source.Stride + k];
                    }                    
                }
            }

            return buffer;
        }

        /// <summary>
        /// Dynamically creates a vertex declaration that fits the data contained by this
        /// vertex container.
        /// </summary>        
        /// <returns>A fitting vertex declaration</returns>
        protected VertexDeclaration CreateVertexDeclaration()
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
        public void CreateVertexBuffer(GraphicsDevice graphicsDevice, out VertexBuffer vertexBuffer, 
            out VertexDeclaration vertexDeclaration)
        {            
            float[] stream = CreateDataStream();
            
            vertexDeclaration = CreateVertexDeclaration();

            vertexBuffer = new VertexBuffer(graphicsDevice, vertexDeclaration, stream.Length, BufferUsage.WriteOnly);            
            vertexBuffer.SetData<float>(stream);
        }

        /// <summary>
        /// Creates a vertex buffer from this vertex container
        /// and stores a copy of the vertex data from which the vertex
        /// buffer is created to the float array vertexData.
        /// </summary>
        /// <param name="graphicsDevice"></param>
        /// <param name="vertexBuffer"></param>
        /// <param name="vertexDeclaration"></param>
        /// <param name="vertexData"></param>
        public void CreateVertexBuffer(GraphicsDevice graphicsDevice, out VertexBuffer vertexBuffer,
            out VertexDeclaration vertexDeclaration, out float[] vertexData)
        {
            vertexData = CreateDataStream();

            vertexDeclaration = CreateVertexDeclaration();
            
            vertexBuffer = new VertexBuffer(graphicsDevice, vertexDeclaration, vertexData.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<float>(vertexData);
        }

        #endregion

        //=====================================================================
        #region Static Methods

        /// <summary>
        /// Creates a vertex container according to the vertex indices given.
        /// </summary>
        /// <param name="indices"></param>
        /// <returns>A vertex container</returns>
        public static VertexContainer CreateVertexContainer(VertexInstructions instructions, 
            out int[] indices, out BoundingBox bounds)
        {
            VertexContainer vertices = new VertexContainer();
            bounds = new BoundingBox();
            bounds.Min = instructions.FetchData<Vector3>(VertexDataType.Position, 0);
            bounds.Max = bounds.Min;

            // Check for basic requirements
            if (instructions.Contains(VertexDataType.Position) == false)
            {
                throw new ArgumentException("Geometry has not all needed information. At least Positions are necessary!");
            }

            List<Vector3> positions = new List<Vector3>(instructions.PositionIndices.Length);
            List<Color> colors = new List<Color>(instructions.ColorIndices.Length);
            List<Vector3> normals = new List<Vector3>(instructions.NormalIndices.Length);
            List<Vector3> tangents = new List<Vector3>(instructions.TangentIndices.Length);
            List<Vector3> binormals = new List<Vector3>(instructions.BinormalIndices.Length);
            List<Vector2> texCoords = new List<Vector2>(instructions.TexCoordIndices.Length);
            List<Vector4> jointIndices = new List<Vector4>(instructions.JointIndices.Length);
            List<Vector3> jointWeights = new List<Vector3>(instructions.JointWeightIndices.Length);

            Dictionary<VertexKey, int> usedVertices = new Dictionary<VertexKey, int>(instructions.NumVertices * 3 / 4);

            List<int> indexList = new List<int>(instructions.PositionIndices.Length);

            // Go through all indices to create vertices
            for (int i = 0; i < instructions.NumVertices; i++)
            {
                int positionIndex = instructions.PositionIndices[i];
                int colorIndex = instructions.HasColors ? instructions.ColorIndices[i] : -1;
                int normalIndex = instructions.HasNormals ? instructions.NormalIndices[i] : -1;
                int tangentIndex = instructions.HasTangents ? instructions.TangentIndices[i] : -1;
                int binormalIndex = instructions.HasBinormals ? instructions.BinormalIndices[i] : -1;
                int texCoordIndex = instructions.HasTextureCoordinates ? instructions.TexCoordIndices[i] : -1;
                int jointIndicesIndex = instructions.HasJoints ? instructions.JointIndices[i] : -1;
                int jointWeightsIndex = instructions.HasJoints ? instructions.JointWeightIndices[i] : -1;

                VertexKey key = new VertexKey(positionIndex, normalIndex, tangentIndex, 
                    texCoordIndex, jointIndicesIndex, jointWeightsIndex);

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
                    int index = positions.Count;

                    // Position is a must
                    Vector3 position = instructions.FetchData<Vector3>(VertexDataType.Position, i);

                    positions.Add(position);

                    if (instructions.HasColors)
                    {
                        Color color = instructions.FetchColor(i);
                        colors.Add(color);
                    }

                    if (instructions.HasNormals)
                    {
                        Vector3 normal = instructions.FetchNormal(i);
                        normals.Add(normal);                        
                    }

                    if (instructions.HasTangents)
                    {
                        Vector3 tangent = instructions.FetchTangent(i);
                        tangents.Add(tangent);                        
                    }

                    if (instructions.HasBinormals)
                    {
                        Vector3 binormal = instructions.FetchBinormal(i);
                        binormals.Add(binormal);
                    }

                    if (instructions.HasTextureCoordinates)
                    {
                        Vector2 texCoord = instructions.FetchTextureCoordinate(i);
                        texCoords.Add(texCoord);                        
                    }                    

                    if (instructions.HasJoints)
                    {
                        jointIndices.Add(instructions.FetchJointIndices(i));
                        jointWeights.Add(instructions.FetchJointWeights(i));
                    }

                    // Remember that this vertex combination was used before
                    // and store the index where it can be found in the 
                    // vertex container
                    usedVertices.Add(key, index);

                    // Add reference to the just created vertex to the index list / buffer
                    indexList.Add(index);

                    // Bounding Box calculation
                    bounds.Min = Vector3.Min(bounds.Min, position);
                    bounds.Max = Vector3.Max(bounds.Max, position);
                }
            }

            // Swap winding order
            for (int i = 0; i < indexList.Count; i+=3)
            {
                int swap = indexList[i + 1];
                indexList[i + 1] = indexList[i + 2];
                indexList[i + 2] = swap;
            }

            // Add all distinct channel entries to the vertex container
            if (positions.Count > 0) vertices.AddPositions(positions.ToArray());
            if (colors.Count > 0) vertices.AddColors(colors.ToArray());
            if (normals.Count > 0) vertices.AddNormals(normals.ToArray());
            if (tangents.Count > 0) vertices.AddTangents(tangents.ToArray());
            if (binormals.Count > 0) vertices.AddBinormals(binormals.ToArray());
            if (texCoords.Count > 0) vertices.AddTextureCoordinates(texCoords.ToArray());
            if (jointIndices.Count > 0) vertices.AddJointIndices(jointIndices.ToArray());
            if (jointWeights.Count > 0) vertices.AddJointWeights(jointWeights.ToArray());

            indices = indexList.ToArray();

            return vertices;
        }        

        #endregion 
    }

    /// <summary>
    /// Vertex key to identify an unique vertex by the combination
    /// of its used position, normal, tangent and texture coordinate
    /// </summary>
    internal struct VertexKey
    {
        private int _positionIndex;
        private int _normalIndex;
        private int _tangentIndex;
        private int _texCoordIndex;
        private int _jointIndicesIndex;
        private int _jointWeightsIndex;

        /// <summary>
        /// Returns the index of the position used or -1 if no position
        /// index was set
        /// </summary>
        public int PositionIndex
        {
            get { return _positionIndex - 1; }
            set { _positionIndex = value + 1; }
        }

        /// <summary>
        /// Returns the index of the joint indices used or -1 if no joint
        /// indices index was set
        /// </summary>
        public int JointIndicesIndex
        {
            get { return _jointIndicesIndex - 1; }
            set { _jointIndicesIndex = value + 1; }
        }

        /// <summary>
        /// Returns the index of the joint weights used or -1 if no joint
        /// weights index was set
        /// </summary>
        public int JointWeightsIndex
        {
            get { return _jointWeightsIndex - 1; }
            set { _jointWeightsIndex = value + 1; }
        }

        /// <summary>
        /// Returns the index of the normal used or -1 if no normal
        /// index was set
        /// </summary>
        public int NormalIndex
        {
            get { return _normalIndex - 1; }
            set { _normalIndex = value + 1; }
        }

        /// <summary>
        /// Returns the index of the tangent used or -1 if no tangent
        /// index was set
        /// </summary>
        public int TangentIndex
        {
            get { return _tangentIndex - 1; }
            set { _tangentIndex = value + 1; }
        }

        /// <summary>
        /// Returns the index of the texture coordinate used or -1 if 
        /// no texture coordinate index was set
        /// </summary>
        public int TexCoordIndex
        {
            get { return _texCoordIndex - 1; }
            set { _texCoordIndex = value + 1; }
        }

        public VertexKey(int positionIndex, int normalIndex, 
            int tangentIndex, int texCoordIndex, int jointIndicesIndex, int jointWeightsIndex)
        {
            _positionIndex = positionIndex + 1;
            _normalIndex = normalIndex + 1;
            _tangentIndex = tangentIndex + 1;
            _texCoordIndex = texCoordIndex + 1;
            _jointIndicesIndex = jointIndicesIndex;
            _jointWeightsIndex = jointWeightsIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexKey)
            {
                VertexKey other = (VertexKey) obj;

                return PositionIndex == other.PositionIndex &&
                       NormalIndex == other.NormalIndex &&
                       TangentIndex == other.TangentIndex &&
                       TexCoordIndex == other.TexCoordIndex; 
                        
                // Note: Joint Indices/Weights are always the same for one position, 
                // no need to compare                       
            }
            
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // Position hash will vary the most
            return PositionIndex.GetHashCode();
        }

        public override string ToString()
        {
            return "VertexKey[" + PositionIndex + ", " +
                   NormalIndex + ", " + TangentIndex + ", " + TexCoordIndex + "]";
        }
    }
}
