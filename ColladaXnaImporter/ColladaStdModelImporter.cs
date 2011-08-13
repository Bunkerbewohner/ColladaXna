using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using ColladaXna.Base;
using System.IO;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Materials;

namespace ColladaXnaImporter
{
    using CVertexChannel = ColladaXna.Base.Geometry.VertexChannel;        

    /// <summary>
    /// This class imports a COLLADA ".dae" file into the XNA default content model for models.
    /// As a result models imported with this class can be processed and loaded with all the
    /// default content processors just like FBX models.
    /// </summary>
    [ContentImporter(".dae", CacheImportedData = true, DisplayName="COLLADA Standard Importer", DefaultProcessor="ModelProcessor")]
    public class ColladaStdModelImporter : ContentImporter<NodeContent>
    {
        /// <summary>
        /// This options determines how vertex channels for skinned mesh animation are imported.
        /// If true, a single vertex channel called "Weights" containing "BoneWeightCollection" 
        /// elements is created. Otherwise two separate channels "BlendIndices0" and 
        /// "BlendWeights0" are created, containing Vector4 and Vector3 elements.
        /// </summary>
        public const Boolean UseBoneWeightCollection = true;

        ColladaModel collada;
        protected static ContentImporterContext importerContext;
        NodeContent rootNode;
        MeshBuilder meshBuilder;
        Dictionary<String, MaterialContent> materials;
        static int boneIndex = 0;

        // Set true to exclude vertex channels for blend weights and indices
        bool excludeBlendWeights = false;

        /// <summary>
        /// Imports the COLLADA Model from the given .DAE file into the XNA Content Model.
        /// </summary>
        /// <param name="filename">Path to .DAE file</param>
        /// <param name="context">Context (is not used)</param>
        /// <returns></returns>
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            boneIndex = 0;
            importerContext = context;

            // Load the complete collada model which is to be converted / imported
            collada = new ColladaModel(filename);

            //Debugger.Launch();
            //Debugger.Break();
            
            rootNode = new NodeContent();
            rootNode.Name = Path.GetFileNameWithoutExtension(filename);
            rootNode.Identity = new ContentIdentity(filename);

            // The default XNA processor only supports up to 255 joints / bones
            if (collada.Joints.Count < 255)
            {
                CreateBones(CreateAnimations());            
            }
            else
            {
                String helpLink = "";
                ContentIdentity cid = rootNode.Identity;
                String msg = String.Format("Model contains {0} bones. Maximum is 255. " + 
                    "Skinning Data (Bones and Vertex Channels) will not be imported!",
                    collada.Joints.Count);

                importerContext.Logger.LogWarning(helpLink, cid, msg);
                excludeBlendWeights = true;
            }
            
            CreateMaterials();
            CreateMeshes();                        

            return rootNode;
        }       

        /// <summary>
        /// Creates BasicMaterialContent instances for each COLLADA material and stores
        /// them in the "materials" dictionary of this class. 
        /// </summary>
        void CreateMaterials()
        {
            materials = new Dictionary<string, MaterialContent>();

            for (int i = 0; i < collada.Materials.Count; i++)
            {
                BasicMaterialContent material = new BasicMaterialContent();                                
                material.Name = collada.Materials[i].Name;

                var diffuse = collada.Materials[i].Properties.OfType<DiffuseColor>().FirstOrDefault();
                if (diffuse != null) material.DiffuseColor = diffuse.Color.ToVector3();

                var texture = collada.Materials[i].Properties.OfType<DiffuseMap>().FirstOrDefault();
                if (texture != null)
                {
                    String dir = Path.GetDirectoryName(collada.SourceFilename) + "/";
                    material.Texture = new ExternalReference<TextureContent>(dir + texture.Texture.Filename);
                }

                var specular = collada.Materials[i].Properties.OfType<SpecularColor>().FirstOrDefault();
                if (specular != null) material.SpecularColor = specular.Color.ToVector3();

                var specpow = collada.Materials[i].Properties.OfType<SpecularPower>().FirstOrDefault();
                if (specpow != null) material.SpecularPower = specpow.Value;

                var alpha = collada.Materials[i].Properties.OfType<Opacity>().FirstOrDefault();
                if (alpha != null) material.Alpha = alpha.Value;

                var emissive = collada.Materials[i].Properties.OfType<EmissiveColor>().FirstOrDefault();
                if (emissive != null) material.EmissiveColor = emissive.Color.ToVector3();                                       

                materials.Add(material.Name, material);
            }            
        }
            
        /// <summary>
        /// Creates MeshContent instances for each mesh in the COLLADA model and attaches
        /// them to the NodeContent root.
        /// </summary>
        void CreateMeshes()
        {
            foreach (Mesh mesh in collada.Meshes)
            {
                foreach (MeshPart part in mesh.MeshParts)
                {
                    meshBuilder = MeshBuilder.StartMesh(mesh.Name);
                    meshBuilder.SwapWindingOrder = false;
                    meshBuilder.MergeDuplicatePositions = false;                                        
                    meshBuilder.SetMaterial(materials[part.MaterialName]);
                    meshBuilder.Name = mesh.Name;

                    // Positions
                    CVertexChannel posChannel = part.Vertices.VertexChannels.Where(c =>
                            c.Description.VertexElementUsage == VertexElementUsage.Position).
                            FirstOrDefault();

                    VertexContainer container = part.Vertices;
                    float[] data = container.Vertices;
                    int posOffset = posChannel.Source.Offset;                    

                    for (int i = 0; i < container.Vertices.Length; i += container.VertexSize)
                    {                        
                        Vector3 pos = new Vector3(data[i + posOffset + 0], 
                            data[i + posOffset + 1], data[i + posOffset + 2]);
                        meshBuilder.CreatePosition(pos);
                    }

                    // Vertex channels other than position
                    List<XnaVertexChannel> channels = new List<XnaVertexChannel>();                    

                    foreach (CVertexChannel cvChannel in part.Vertices.VertexChannels)
                    {
                        switch (cvChannel.Description.VertexElementUsage)
                        {
                            case VertexElementUsage.Position:
                                // Position is already created above
                                break;

                            case VertexElementUsage.BlendWeight:
                            case VertexElementUsage.BlendIndices:
                                // When bone weight collections are used these two
                                // channels get added separately later
                                if (UseBoneWeightCollection || excludeBlendWeights)                                    
                                    break;
                                else
                                    goto default;

                            default:
                                // standard channel like texcoord, normal etc.
                                channels.Add(new XnaVertexChannel(meshBuilder, cvChannel));
                                break;
                        }
                    }

                    // BoneWeightCollection vertex channel
                    if (UseBoneWeightCollection && !excludeBlendWeights)
                    {
                        try
                        {
                            channels.Add(new XnaBoneWeightChannel(meshBuilder, part.Vertices, 
                                collada.Joints));
                        }
                        catch (Exception)
                        {
                            importerContext.Logger.LogMessage("No skinning information found");
                        }
                    }                    

                    // Triangles
                    for (int i = 0; i < part.Indices.Length; i += 3)
                    {
                        for (int j = i; j < i + 3; j++)
                        {                            
                            // Set channel components (other than position)
                            foreach (var channel in channels)                            
                                channel.SetData(j);                            

                            meshBuilder.AddTriangleVertex(part.Indices[j]);            
                        }
                    }

                    MeshContent meshContent = meshBuilder.FinishMesh();
                    rootNode.Children.Add(meshContent);
                }
            }            
        }

        List<AnimationContent> CreateAnimations()
        {
            var animations = new List<AnimationContent>();

            for (int i = 0; i < collada.JointAnimations.Count; i++)
            {
                var sourceAnim = collada.JointAnimations[i];

                AnimationContent animation = new AnimationContent();
                animation.Name = sourceAnim.Name ?? String.Format("Take {0:000}", (i+1));
                animation.Duration = TimeSpan.FromSeconds(sourceAnim.EndTime - sourceAnim.StartTime);

                foreach (var sourceChannel in sourceAnim.Channels)
                {
                    AnimationChannel channel = new AnimationChannel();

                    // Adds the different keyframes to the animation channel
                    // NOTE: Might be better to sample the keyframes
                    foreach (var sourceKeyframe in sourceChannel.Sampler.Keyframes)
                    {
                        TimeSpan time = TimeSpan.FromSeconds(sourceKeyframe.Time);
                        Matrix transform = sourceKeyframe.Transform;

                        AnimationKeyframe keyframe = new AnimationKeyframe(time, transform);
                        channel.Add(keyframe);
                    }

                    String key = GetJointKey(sourceChannel.Target);
                    animation.Channels.Add(key, channel);
                }

                animation.OpaqueData.Add("FPS", sourceAnim.FramesPerSecond);      
                animations.Add(animation);
            }

            return animations;
        }

        /// <summary>
        /// Usually Joints (or Bones) are referenced by their name. However, in COLLADA
        /// names are optional. Instead each joint might only have an ID or SID, or nothing.
        /// This method generates a reliable string key for each joint. 
        /// </summary>
        /// <param name="joint">An joint</param>
        /// <returns>String key that assumes one of the following possible values in ascending
        /// precedence (depending on the availability of each property): 
        /// Name > GlobalID > ScopedID > Joint[NR]</returns>
        protected static String GetJointKey(Joint joint)
        {
            if (joint.Name != null && joint.Name.Length > 0)
                return joint.Name;

            if (joint.GlobalID != null && joint.GlobalID.Length > 0)
                return joint.GlobalID;

            if (joint.ScopedID != null && joint.ScopedID.Length > 0)
                return joint.ScopedID;

            return String.Format("Bone{0}", ++boneIndex);
        }

        void CreateBones(List<AnimationContent> animations)
        {
            if (collada.Joints.Count == 0) return;
            boneIndex = 0;
            Joint rootJoint = collada.Joints[collada.Joints.Count - 1];

            // Create skeleton recursively from joints
            BoneContent root = CreateSkeleton(rootJoint);            

            // Attach animations to root bone 
            foreach (var animation in animations)
                root.Animations.Add(animation.Name, animation);

            rootNode.Children.Add(root);
        }

        BoneContent CreateSkeleton(Joint joint)
        {
            var bone = new BoneContent();
            bone.Name = GetJointKey(joint);
            bone.Transform = joint.Transform;

            if (joint.Children != null && joint.Children.Count > 0)
            {
                foreach (var childJoint in joint.Children)
                {
                    bone.Children.Add(CreateSkeleton(childJoint));
                }
            }

            return bone;
        }

        /// <summary>
        /// Helper class for creating vertex channels with MeshBuilder
        /// from Collada VertexChannels.
        /// </summary>
        class XnaVertexChannel
        {
            protected MeshBuilder _meshBuilder;
            protected CVertexChannel _colladaVertexChannel;            
            protected int _channelIndex;
            protected int _vertexSize;
            protected int _offset;            

            /// <summary>
            /// Creates a new vertex channel using the given MeshBuilder based on a
            /// Collada VertexChannel. No data is initially added to the vertex channel!
            /// For this, use the SetData method.
            /// </summary>
            /// <param name="meshBuilder">MeshBuilder instance to store vertex channel in</param>
            /// <param name="colladaVertexChannel">Original COLLADA Vertex Channel</param>
            public XnaVertexChannel(MeshBuilder meshBuilder, CVertexChannel colladaVertexChannel)
            {
                _colladaVertexChannel = colladaVertexChannel;                
                _meshBuilder = meshBuilder;
                _vertexSize = colladaVertexChannel.Source.Stride;
                _offset = colladaVertexChannel.Source.Offset;                

                Create();
            }

            protected XnaVertexChannel(MeshBuilder mb)
            {
                _meshBuilder = mb;
            }

            /// <summary>
            /// Creates the vertex channel using the MeshBuilder (no data is added yet).
            /// </summary>
            protected virtual void Create()
            {
                var usage = _colladaVertexChannel.Description.VertexElementUsage;
                int usageIndex = _colladaVertexChannel.Description.UsageIndex;

                // Construct correct usage string
                String usageString = VertexChannelNames.EncodeName(usage, usageIndex);                         

                // Generic standard channel (TexCoord, Normal, Binormal, etc.)
                switch (_colladaVertexChannel.Description.VertexElementFormat)
                {
                    case VertexElementFormat.Vector4:
                        _channelIndex = _meshBuilder.CreateVertexChannel<Vector4>(usageString);
                        break;

                    case VertexElementFormat.Vector3:
                        _channelIndex = _meshBuilder.CreateVertexChannel<Vector3>(usageString);
                        break;

                    case VertexElementFormat.Vector2:
                        _channelIndex = _meshBuilder.CreateVertexChannel<Vector2>(usageString);
                        break;

                    case VertexElementFormat.Single:
                        _channelIndex = _meshBuilder.CreateVertexChannel<Single>(usageString);                        
                        break;

                    default:
                        throw new Exception("Unexpected vertex element format");
                }
            }

            /// <summary>
            /// Sets vertex channel data for the current vertex (controlled externally through the
            /// MeshBuilder instance) to the value found at given index in the original COLLADA
            /// Vertex Channel.
            /// </summary>
            /// <param name="index">Index of element in the original COLLADA Vertex Channel</param>
            public virtual void SetData(int index)
            {                
                int i = _colladaVertexChannel.Indices[index] * _vertexSize + _offset;
                float[] data = _colladaVertexChannel.Source.Data;

                object value;

                switch (_colladaVertexChannel.Description.VertexElementFormat)
                {
                    case VertexElementFormat.Vector4:
                        value = new Vector4(data[i], data[i + 1], data[i + 2], data[i + 3]);
                        break;

                    case VertexElementFormat.Vector3:
                        value = new Vector3(data[i], data[i + 1], data[i + 2]);
                        break;

                    case VertexElementFormat.Vector2:
                        value = new Vector2(data[i], data[i + 1]);
                        break;

                    case VertexElementFormat.Single:
                        value = data[i];
                        break;

                    default:
                        throw new Exception("Unexpected vertex element format");
                }

                _meshBuilder.SetVertexChannelData(_channelIndex, value);        
            }
        }

        class XnaBoneWeightChannel : XnaVertexChannel
        {
            JointList _joints;

            CVertexChannel _indicesChannel;
            CVertexChannel _weightChannel;

            int _indicesOffset;
            int _weightOffset;

            public XnaBoneWeightChannel(MeshBuilder mb, VertexContainer vc, JointList joints)        
                : base(mb)
            {
                try
                {
                    _indicesChannel = (from c in vc.VertexChannels
                                       where c.Contains(VertexElementUsage.BlendIndices)
                                       select c).First();

                    _weightChannel = (from c in vc.VertexChannels
                                      where c.Contains(VertexElementUsage.BlendWeight)
                                      select c).First();
                }
                catch (Exception)
                {
                    throw new Exception("Missing blend indices or weights");
                }

                _joints = joints;
                _vertexSize = vc.VertexSize;
                _indicesOffset = _indicesChannel.Source.Offset;
                _weightOffset = _weightChannel.Source.Offset;

                this.Create();           
            }

            protected override void Create()
            {                                
                String usageString = VertexChannelNames.Weights();
                _channelIndex = _meshBuilder.CreateVertexChannel<BoneWeightCollection>(usageString);
            }

            public override void SetData(int index)
            {
                int i = _indicesChannel.Indices[index] * _vertexSize + _indicesOffset;
                int j = _weightChannel.Indices[index] * _vertexSize + _weightOffset;

                // Both channels use the same source data
                float[] data = _indicesChannel.Source.Data;

                float[] blendIndices = new float[] { data[i + 0], data[i + 1], data[i + 2], data[i + 3] };
                float[] blendWeights = new float[] { data[j + 0], data[j + 1], data[j + 2], 0 };

                // Fourth blend weight is stored implicitly
                blendWeights[3] = 1 - blendWeights[0] - blendWeights[1] - blendWeights[2];
                if (blendWeights[3] == 1) blendWeights[3] = 0;

                BoneWeightCollection weights = new BoneWeightCollection();

                for (int k = 0; k < blendIndices.Length; k++)
                {
                    int jointIndex = (int)blendIndices[k];
                    float jointWeight = blendWeights[k];
                    if (jointWeight <= 0) continue;

                    String jointName = GetJointKey(_joints[jointIndex]);
                    
                    weights.Add(new BoneWeight(jointName, jointWeight));
                }

                if (weights.Count > 0)
                    _meshBuilder.SetVertexChannelData(_channelIndex, weights);        
            }
        }
    }        
}