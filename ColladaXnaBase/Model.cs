using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base
{
    public class Model
    {
        #region Public Properties

        /// <summary>
        /// Three-dimensional float array containing vertex data used by
        /// all mesh parts. There is an entry for each mesh (first dimension)
        /// and each mesh part (second dimension).
        /// This data can be used for different techniques which need access
        /// to the vertex data like skinned animation.         
        /// This copy of vertex data exists because vertex buffers are created
        /// with BufferUsage.WriteOnly for performance reasons.
        /// </summary>
        /// <remarks>This might be null</remarks>
        protected float[][][] _vertexData;

        /// <summary>
        /// Two-dimensional index buffer array containing index data used by
        /// all mesh parts. The structure is just like the vertex data's.
        /// </summary>
        /// <remarks>This might be null</remarks>
        protected VariableIndexBuffer[][] _indexData;

        /// <summary>
        /// Dictionary of effects used by this model. The name of the corresponding
        /// material of each effect is used as the key.
        /// </summary>
        public Dictionary<string, EffectMaterial> Effects { get { return _effects; } }

        /// <summary>
        /// List of meshes which contain vertex and index buffers
        /// </summary>
        public List<ModelMesh> Meshes { get { return _meshes; } }

        /// <summary>
        /// List of mesh instances
        /// </summary>
        public List<ModelMeshInstance> MeshInstances { get { return _meshInstances; } }

        /// <summary>
        /// Number of polygons this model consist of
        /// </summary>
        public int PolyCount { get { return _polyCount; } }

        /// <summary>
        /// List of joints. Can be null, if no joints were defined
        /// in the original model file.
        /// </summary>
        public JointList Joints { get { return _joints; } }

        /// <summary>
        /// List of joint animations for Skinned Mesh animation.
        /// Can be null if no animations were defined in the original
        /// model file.
        /// </summary>
        public JointAnimationList JointAnimations { get { return _jointAnimations; } }

        /// <summary>
        /// Working copy of vertex data contained by vertex buffers of this model.
        /// This can be used to access vertex data instead of fetching data from
        /// vertex buffers directly, since that wouldn't be possible for they are
        /// created with BufferUsage.WriteOnly by default.
        /// </summary>
        /// <remarks>Might be null if this data wasn't provided when this ModelData
        /// instance was created</remarks>
        public float[][][] VertexData { get { return _vertexData; } }

        /// <summary>
        /// Working copy of index data contained by index buffers of this model.
        /// This can be used to access index data instead of fetching data from
        /// index buffers directly, since that wouldn't be possible for they are
        /// created with BufferUsage.WriteOnly by default.
        /// </summary>
        public VariableIndexBuffer[][] IndexData { get { return _indexData; } }

        /// <summary>
        /// Gets the bounding box encompassing all of this model's geometry
        /// </summary>
        public BoundingBox Bounds
        {
            get { return _bounds; }
        }

        /// <summary>
        /// Gets the name of this model
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Gets the root joint of this model. Is null if there are no joints defined.
        /// </summary>
        public Joint RootJoint
        {
            get { return _joints != null && _joints.Count > 0 ? _joints[_joints.Count - 1] : null; }
        }

        #endregion

        /// <summary>
        /// Creates an instance of a runtime model representation that can be used to 
        /// draw the 3d model data contained.
        /// </summary>
        /// <param name="name">Name of the model</param>
        /// <param name="effects">Dictionary of effects used</param>
        /// <param name="meshes">List of runtime meshes</param>
        /// <param name="meshInstances">List of runtime mesh instances</param>
        /// <param name="lights">List of lights</param>
        /// <param name="joints">List of joints</param>
        /// <param name="animations">List of joint animations</param>
        /// <param name="vertexData">Copy of vertex data if necessary</param>
        /// <param name="indexData">Copy of index data if necessary</param>
        public ModelData(string name, Dictionary<string, EffectMaterial> effects, List<ModelMesh> meshes,
            List<ModelMeshInstance> meshInstances, List<Light> lights, JointList joints,
            JointAnimationList animations, float[][][] vertexData, VariableIndexBuffer[][] indexData)
        {
            Name = name;
            _effects = effects;
            _meshes = meshes;
            _meshInstances = meshInstances;
            _lights = lights;
            _joints = joints;
            _vertexData = vertexData;
            _indexData = indexData;
            _jointAnimations = animations;

            // Calculate bounds of all meshes in object space
            _bounds = new BoundingBox();

            foreach (var mi in _meshInstances)
            {
                BoundingBox current = mi.ModelMesh.Bounds;
                Vector3.Transform(current.Min, mi.AbsoluteTransform);
                Vector3.Transform(current.Max, mi.AbsoluteTransform);

                _bounds.Min = Vector3.Min(Bounds.Min, current.Min);
                _bounds.Max = Vector3.Max(Bounds.Max, current.Max);
            }

#if DEBUG
            foreach (var part in meshInstances.SelectMany(inst => inst.ModelMesh.Parts))
            {
                _polyCount += part.NumVertices / 3;
            }
#endif
        }

        #region Static Helper Methods

        /// <summary>
        /// Creates a runtime representation of ModelData from intermediate data that was either
        /// read from a compiled XNB file via ModelReader or directly loaded from a DAE file 
        /// via the RuntimeLoader.
        /// </summary>
        /// <param name="name">Name of the model</param>
        /// <param name="materials">List of used materials</param>
        /// <param name="meshes">List of meshes</param>
        /// <param name="meshInstances">List of mesh instances</param>
        /// <param name="lights">List of lights</param>
        /// <param name="joints">List of joints</param>
        /// <param name="jointAnimations">List of joint animations</param>
        /// <returns>ModelData instance</returns>
        public static ModelData CreateFromIntermediateData(string name, List<EffectMaterial> materials,
            List<Mesh> meshes, List<MeshInstance> meshInstances,
            List<Light> lights, JointList joints, JointAnimationList jointAnimations, GraphicsDevice graphicsDevice)
        {
            // Material Library by Name
            Dictionary<string, EffectMaterial> materialLibrary = materials.ToDictionary(mat => mat.Name);

            // Determine if vertex data should be stored seperately from the vertex buffer
            bool storeVertexData = jointAnimations.Any() || joints.Any();

            List<ModelMesh> modelMeshes = new List<ModelMesh>(meshes.Count);
            float[][][] vertexData = storeVertexData ? new float[meshes.Count][][] : null;
            VariableIndexBuffer[][] indexData = storeVertexData ?
                new VariableIndexBuffer[meshes.Count][] : null;

            // Counters for [m]eshes and mesh [p]arts (for vertexData indices)
            int m = 0, p = 0;

            foreach (var inMesh in meshes)
            {
                ModelMesh modelMesh = new ModelMesh();
                modelMesh.Name = inMesh.Name;
                modelMesh.Parts = new List<ModelMeshPart>(inMesh.MeshParts.Length);
                modelMesh.Instances = new List<ModelMeshInstance>();
                modelMesh.Bounds = inMesh.Bounds;

                HashSet<EffectMaterial> distinctMaterials = new HashSet<EffectMaterial>();
                if (storeVertexData)
                {
                    vertexData[m] = new float[inMesh.MeshParts.Length][];
                    indexData[m] = new VariableIndexBuffer[inMesh.MeshParts.Length];
                }

                foreach (var inMeshPart in inMesh.MeshParts)
                {
                    ModelMeshPart modelMeshPart = new ModelMeshPart();
                    if (inMeshPart.MaterialName == null || inMeshPart.MaterialName.Length == 0)
                        modelMeshPart.Material = EffectMaterial.CreateDefaultMaterial(inMeshPart, graphicsDevice);
                    else
                        modelMeshPart.Material = materialLibrary[inMeshPart.MaterialName];
                    distinctMaterials.Add(modelMeshPart.Material);

                    // Index Buffer
                    int numVertices = inMeshPart.Vertices.VertexChannels[0].Count;
                    if (numVertices < short.MaxValue)
                    {
                        // convert indices to short array
                        short[] indices = (from i in inMeshPart.Indices select (short)i).ToArray();

                        var indexBuffer = new IndexBuffer(graphicsDevice, typeof(short),
                            inMeshPart.Indices.Length, BufferUsage.WriteOnly);
                        indexBuffer.SetData(indices);

                        modelMeshPart.IndexBuffer = indexBuffer;
                        if (storeVertexData) indexData[m][p] = new VariableIndexBuffer(indices);
                    }
                    else
                    {
                        var indexBuffer = new IndexBuffer(graphicsDevice, typeof(int),
                            inMeshPart.Indices.Length, BufferUsage.WriteOnly);
                        indexBuffer.SetData(inMeshPart.Indices);

                        modelMeshPart.IndexBuffer = indexBuffer;
                        if (storeVertexData) indexData[m][p] = new VariableIndexBuffer(inMeshPart.Indices);
                    }

                    // Vertex Buffer
                    VertexBuffer vertexBuffer;
                    VertexDeclaration vertexDeclaration;

                    if (storeVertexData)
                    {
                        // Note: VertexData might has to be copied
                        inMeshPart.Vertices.CreateVertexBuffer(graphicsDevice, out vertexBuffer,
                            out vertexDeclaration, out vertexData[m][p]);
                    }
                    else
                    {
                        inMeshPart.Vertices.CreateVertexBuffer(graphicsDevice, out vertexBuffer,
                            out vertexDeclaration);
                    }

                    modelMeshPart.VertexBuffer = vertexBuffer;
                    modelMeshPart.VertexDeclaration = vertexDeclaration;
                    modelMeshPart.VertexStride = vertexDeclaration.VertexStride;
                    modelMeshPart.NumVertices = inMeshPart.Indices.Length;

                    modelMesh.Parts.Add(modelMeshPart);
                    ++p;
                }

                // save all used materials in mesh
                modelMesh.Materials = distinctMaterials.ToList();

                modelMeshes.Add(modelMesh);
                ++m;
                p = 0;
            }

            // Mesh instances
            var modelMeshInstances = new List<ModelMeshInstance>(meshInstances.Count);

            foreach (var inst in meshInstances)
            {
                ModelMeshInstance modelMeshInstance = new ModelMeshInstance();
                modelMeshInstance.AbsoluteTransform = inst.AbsoluteTransform;
                modelMeshInstance.ModelMesh = modelMeshes.Where(mesh =>
                    inst.MeshName.Equals(mesh.Name)).Single();
                modelMeshInstance.ModelMesh.Instances.Add(modelMeshInstance);

                modelMeshInstances.Add(modelMeshInstance);
            }

            // Lights: Set Effect Parameters
            foreach (var mat in modelMeshes.SelectMany(mesh => mesh.Materials))
            {
                ApplyLightParameters(mat, lights);
            }

            ModelData modelData = new ModelData(name, materialLibrary, modelMeshes, modelMeshInstances,
                lights, joints, jointAnimations, vertexData, indexData);

            return modelData;
        }

        /// <summary>
        /// Applies light parameters
        /// </summary>
        /// <param name="material">Material whose parameters shall be set</param>
        /// <param name="lights">List of lights to apply</param>
        static void ApplyLightParameters(EffectMaterial material, List<Light> lights)
        {
            if (material.Effect is BasicEffect)
            {
                BasicEffect effect = material.Effect as BasicEffect;

                var dirLights = lights.OfType<DirectionalLight>().ToList();
                for (int i = 0; i < dirLights.Count; i++)
                {
                    var light = (i == 0 ? effect.DirectionalLight0 :
                        (i == 1 ? effect.DirectionalLight1 : effect.DirectionalLight2));

                    light.DiffuseColor = dirLights[i].Color.ToVector3();
                    light.Direction = dirLights[i].Direction;
                    light.Enabled = true;
                }

                var ambient = lights.OfType<AmbientLight>().FirstOrDefault();
                if (ambient != null)
                {
                    effect.AmbientLightColor = ambient.Color.ToVector3();
                }

                return;
            }
            else if (material.Material.Properties.OfType<CustomShader>().Any())
            {
                // Custom Shaders handle lights themselves, if there are any
            }
            else
            {
                // Directional Lights
                foreach (var light in lights.OfType<DirectionalLight>())
                {
                    string dirParam = light.Name + "Direction";
                    string colorParam = light.Name + "Color";

                    material.Effect.Parameters[colorParam].SetValue(light.Color.ToVector3());
                    material.Effect.Parameters[dirParam].SetValue(light.Direction);
                }

                // Ambient Lights
                foreach (var light in lights.OfType<AmbientLight>())
                {
                    string colorParam = light.Name + "Color";

                    if (material.Effect.Parameters[colorParam] == null)
                    {
                        throw new ApplicationException("Parameter '" + colorParam + "' existiert " +
                            "nicht");
                    }
                    material.Effect.Parameters[colorParam].SetValue(light.Color.ToVector3());
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Rotates the model represented by this ModelData instance by 
        /// setting the root joint transform to given transform. The
        /// bounding box is transformed accordingly.
        /// </summary>
        /// <param name="rotation"></param>
        public void Rotate(Matrix rotation)
        {
            RootJoint.Transform = rotation;
            RootJoint.AbsoluteTransform = rotation;

            _bounds.Min = Vector3.Transform(_bounds.Min, rotation);
            _bounds.Max = Vector3.Transform(_bounds.Max, rotation);
        }

        #endregion

        #region Private and Protected Fields

        protected Dictionary<string, EffectMaterial> _effects;
        protected List<ModelMesh> _meshes;
        protected List<ModelMeshInstance> _meshInstances;
        protected List<Light> _lights;
        protected BoundingBox _bounds;
        protected int _polyCount;
        protected JointList _joints;
        protected JointAnimationList _jointAnimations;

        #endregion
    }
}
