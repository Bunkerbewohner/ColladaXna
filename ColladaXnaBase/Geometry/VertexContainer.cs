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
        public static VertexContainer CreateVertexContainer(List<VertexChannel> inputChannels, 
            out int[] indices, out BoundingBox bounds)
        {
            VertexContainer container = new VertexContainer();
            bounds = new BoundingBox();
            bounds.Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            bounds.Max = bounds.Min;

            // Check for basic requirements
            if (inputChannels.Any(c => c.Description.VertexElementUsage == VertexElementUsage.Position) == false)            
                throw new ArgumentException("Geometry has not all needed information. At least Positions are necessary!");

            // List of vertices as float array (first 3 floats might be position, next 3 color and so on)
            List<float[]> vertices = new List<float[]>();
            int numIndices = inputChannels.First().Indices.Length;

            // Remember the position of distinct vertices to avoid duplicates
            Dictionary<VertexKey, int> usedVertices = new Dictionary<VertexKey, int>(numIndices * 3 / 4);

            // Indices referencing the new vertex buffer (vertices)
            List<int> indexList = new List<int>(numIndices);

            // Go through all indices to create vertices
            for (int i = 0; i < numIndices; i++)
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
            if (positions.Count > 0) container.AddPositions(positions.ToArray());
            if (colors.Count > 0) container.AddColors(colors.ToArray());
            if (normals.Count > 0) container.AddNormals(normals.ToArray());
            if (tangents.Count > 0) container.AddTangents(tangents.ToArray());
            if (binormals.Count > 0) container.AddBinormals(binormals.ToArray());
            if (texCoords.Count > 0) container.AddTextureCoordinates(texCoords.ToArray());
            if (jointIndices.Count > 0) container.AddJointIndices(jointIndices.ToArray());
            if (jointWeights.Count > 0) container.AddJointWeights(jointWeights.ToArray());

            indices = indexList.ToArray();

            return container;
        }        

        #endregion 
    }

    /// <summary>
    /// Vertex key to identify an unique vertex by the combination
    /// of its used position, normal, tangent and texture coordinate
    /// </summary>
    internal struct VertexKey
    {
        List<int> Indices;
        List<VertexChannel> VertexChannels;
        
        public VertexKey(List<int> indices, List<VertexChannel> vertexChannels)
        {
            Indices = indices;
            VertexChannels = vertexChannels;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexKey)
            {
                VertexKey other = (VertexKey) obj;

                return Indices[0] == other.Indices[0] &&
                       (Indices[1] == other.Indices[1] &&
                       Indices[2] == other.Indices[2] &&
                       Indices[3] == other.Indices[3]; 
                        
                // Note: Joint Indices/Weights are always the same for one position, 
                // no need to compare                       
            }
            
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            // Position hash will vary the most
            return Indices[0].GetHashCode();
        }
    }
}
