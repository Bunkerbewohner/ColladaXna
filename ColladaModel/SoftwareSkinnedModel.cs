using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Animation;
using Omi.Xna.Collada.Model.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Omi.Xna.Collada.Model.Geometry;
using ModelMesh = Omi.Xna.Collada.Model.ModelMesh;
using ModelMeshPart = Omi.Xna.Collada.Model.ModelMeshPart;

namespace Omi.Xna.Collada.Model
{
    /// <summary>
    /// Implementation of a model with skinned mesh animation where calculations
    /// are performed on the CPU (thus "Software", as opposed to "Hardware", 
    /// i.e. on the GPU).
    /// </summary>
    public class SoftwareSkinnedModel : IModel
    {
        //=====================================================================
        #region Private Fields

        /// <summary>
        /// Model data instance
        /// </summary>
        private ModelData _modelData;

        /// <summary>
        /// Another copy of vertex data that is being manipulated by the animation.
        /// Model.VertexData is used as the original template.
        /// </summary>
        private float[][][] _vertexData;

        /// <summary>
        /// Working copy of joints to transform
        /// </summary>
        private JointList _joints;

        /// <summary>
        /// Dynamic vertex buffers used for animation
        /// </summary>
        private DynamicVertexBuffer[][] _vertexBuffers;

        /// <summary>
        /// Index buffers
        /// </summary>
        private VariableIndexBuffer[][] _indexBuffers;

        private Dictionary<string, JointAnimation> _animationsById;
        private Dictionary<string, JointAnimation> _animationsByName;

        private float _timer = 0;

        private Matrix inverseRootJointTransform = Matrix.Identity;

        #endregion

        public bool EnableCulling { get; set; }

        public JointList AnimatedJoints { get { return _joints; } }        

        //=====================================================================
        #region Constructor and object creation

        /// <summary>
        /// Creates a new Model that supports software (CPU) skinned animation.
        /// The given model is used as template and is not modified, thus multiple
        /// SoftwareSkinnedModel[s] can use a single Model instance.
        /// </summary>
        /// <param name="modelData">Model instance to animate</param>
        public SoftwareSkinnedModel(ModelData modelData)
        {
            // For software skinning a copy of the vertex data is needed
            if (modelData.VertexData == null)
                throw new ArgumentException("Model must contain copy of vertex data");

            _modelData = modelData;
            _joints = modelData.Joints.CopyPrimary();

            // Animation dictionaries            
            _animationsByName = modelData.JointAnimations.Where(j => j.Name != null).
                ToDictionary(j => j.Name);

            _animationsById = modelData.JointAnimations.Where(j => j.GlobalID != null).
                ToDictionary(j => j.GlobalID);
            
            // Deep copy vertex data
            _vertexData = new float[modelData.VertexData.Length][][];

            for (int i = 0; i < modelData.VertexData.Length; i++)
            {
                _vertexData[i] = new float[modelData.VertexData[i].Length][];
                
                for (int j = 0; j < modelData.VertexData[i].Length; j++)
                {
                    _vertexData[i][j] = new float[modelData.VertexData[i][j].Length];
                    Array.Copy(modelData.VertexData[i][j], _vertexData[i][j], modelData.VertexData[i][j].Length);
                }
            }                 

            // Create dynamic vertex buffers and index buffers
            GraphicsDevice graphicsDevice = modelData.Meshes[0].Materials[0].Effect.GraphicsDevice;

            _vertexBuffers = new DynamicVertexBuffer[modelData.Meshes.Count][];          
            _indexBuffers = new VariableIndexBuffer[modelData.Meshes.Count][];            

            for (int i = 0; i < _vertexBuffers.Length; i++)
            {
                _vertexBuffers[i] = new DynamicVertexBuffer[modelData.Meshes[i].Parts.Count];
                _indexBuffers[i] = new VariableIndexBuffer[modelData.Meshes[i].Parts.Count];                
                
                for (int j = 0; j < _vertexBuffers[i].Length; j++)
                {                    
                    _vertexBuffers[i][j] = new DynamicVertexBuffer(graphicsDevice, modelData.Meshes[i].Parts[j].VertexDeclaration, 
                        _vertexData[i][j].Length, BufferUsage.None);

                    _vertexBuffers[i][j].ContentLost += new EventHandler<System.EventArgs>(ContentLost);
                    _vertexBuffers[i][j].SetData(_vertexData[i][j]);                    

                    _indexBuffers[i][j] = _modelData.IndexData[i][j];                    
                }
            }

            EnableCulling = true;
        }

        /// <summary>
        /// Handler for the GraphicsDevice.ContentLost event.
        /// Fills the dynamic vertex buffers anew.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void ContentLost(object sender, EventArgs args)
        {
            if (!(sender is DynamicVertexBuffer)) return;
            for (int i = 0; i < _vertexBuffers.Length; i++)
            {
                for (int j = 0; j < _vertexBuffers[i].Length; j++)
                {
                    if (_vertexBuffers[i][j] == sender)
                    {
                        _vertexBuffers[i][j].SetData(_vertexData[i][j]);
                        break;
                    }
                }
            }            
        }

        #endregion

        //=====================================================================
        #region Animation

        /// <summary>
        /// Transforms the private working copy of the vertex data according to the
        /// animation state at the given time.
        /// </summary>
        /// <param name="animation">Animation</param>
        /// <param name="time">Time within animation</param>
        protected void ApplyJointAnimation(JointAnimation animation, float time)
        {
            animation.Sample(time, _joints);
        }

        /// <summary>
        /// Applies the transform of the given joint to all of its children
        /// and the resulting transform to their respective children etc. 
        /// recursively
        /// </summary>
        protected void ApplyParentTransform(Joint parent)
        {            
            if (parent.Parent == null)
            {
                // Root joint
                parent.AbsoluteTransform = parent.Transform;
                inverseRootJointTransform = Matrix.Invert(parent.AbsoluteTransform);
            }

            if (parent.Children == null)
                return;

            foreach (Joint joint in parent.Children)
            {
                joint.AbsoluteTransform = joint.Transform * parent.AbsoluteTransform;
                if (parent.Parent == null) joint.AbsoluteTransform = joint.AbsoluteTransform * inverseRootJointTransform;

                ApplyParentTransform(joint);
            }
        }

        // Transforms the working copy of the vertices according to the joints
        protected void TransformVertices()
        {
            for (int i = 0; i < _modelData.Meshes.Count; i++)
            {
                for (int j = 0; j < _modelData.Meshes[i].Parts.Count; j++)
                {                    
                    ModelMeshPart part = _modelData.Meshes[i].Parts[j];

                    VertexElement[] vertexElements = part.VertexDeclaration.GetVertexElements();
                    bool normals = vertexElements.Any(el => el.VertexElementUsage == VertexElementUsage.Normal);
                    bool tangents = vertexElements.Any(el => el.VertexElementUsage == VertexElementUsage.Tangent);

                    // Check if there are joint indices and weights)
                    if (!part.VertexDeclaration.GetVertexElements().Any(el => 
                        el.VertexElementUsage == VertexElementUsage.BlendIndices))
                    {
                        // TODO: Optimize the check for Blend Indices in TransformVertices()
                        // this part seems not to be part of the skin, so nothing
                        // has to be calculated here
                        continue;
                    }

                    int vertexStride = part.VertexStride / sizeof(float);
                    float[] data = _modelData.VertexData[i][j];

                    for (int k = 0, l = 0; l < (data.Length / vertexStride); k += vertexStride, l++)
                    {
                        // Start index of the next vertex
                        int k2 = k + vertexStride;

                        // Weights (last three floats)
                        Vector4 weights = new Vector4(data[k2-3], data[k2-2], data[k2-1], 0);
                        weights.W = 1 - weights.X - weights.Y - weights.Z;
                        if(weights.W == 1) continue;

                        // Indices (the four floats before)
                        Vector4 indices = new Vector4(data[k2-7], data[k2-6], data[k2-5], data[k2-4]);                                     

                        // Position is always the first three floats
                        Vector3 oldPos = new Vector3(data[k+0], data[k+1], data[k+2]);
                        Vector3 oldNormal = Vector3.Zero;
                        Vector3 oldTangent = Vector3.Zero;

                        // If available get normals and tangents, which have to be transformed as well
                        if (normals) oldNormal = new Vector3(data[k + 3], data[k + 4], data[k + 5]);
                        if (tangents) oldTangent = new Vector3(data[k + 6], data[k + 7], data[k + 8]);                        

                        Vector3 newPos = Vector3.Zero;
                        Vector3 newNormal = Vector3.Zero;
                        Vector3 newTangent = Vector3.Zero;

                        // Calculate skin matrix
                        int j0 = (int) indices.X;
                        int j1 = (int) indices.Y;
                        int j2 = (int) indices.Z;
                        int j3 = (int) indices.W;

                        Matrix skinTransform =
                            (_joints[j0].InvBindPose * _joints[j0].AbsoluteTransform) * weights.X +
                            (_joints[j1].InvBindPose * _joints[j1].AbsoluteTransform) * weights.Y +
                            (_joints[j2].InvBindPose * _joints[j2].AbsoluteTransform) * weights.Z +
                            (_joints[j3].InvBindPose * _joints[j3].AbsoluteTransform) * weights.W;

                        newPos = Vector3.Transform(oldPos, skinTransform);

                        if (normals || tangents)
                        {
                            Matrix normalTransform = Matrix.Invert(Matrix.Transpose(skinTransform));                            

                            newNormal = Vector3.Transform(oldNormal, normalTransform);
                            newTangent = Vector3.Transform(oldTangent, normalTransform);
                        }                       

                        // Save to working copy of vertex data
                        float[] final = _vertexData[i][j];

                        final[k + 0] = newPos.X;
                        final[k + 1] = newPos.Y;
                        final[k + 2] = newPos.Z;

                        if (normals)
                        {
                            newNormal.Normalize();

                            final[k + 3] = newNormal.X;
                            final[k + 4] = newNormal.Y;
                            final[k + 5] = newNormal.Z;
                        }

                        if (tangents)
                        {
                            newTangent.Normalize();

                            final[k + 6] = newTangent.X;
                            final[k + 7] = newTangent.Y;
                            final[k + 8] = newTangent.Z;
                        }                                       
                    }

                    // Update dynamic vertex buffer
                    _vertexBuffers[i][j].SetData(_vertexData[i][j]);
                }
            }            
        }        

        /// <summary>
        /// Simply plays the first (and usually only) animation of this model
        /// </summary>
        /// <param name="timeAdvance">time advanced in seconds</param>
        public void PlayFirstAnimation(float timeAdvance)
        {
            _timer += timeAdvance;
            JointAnimation animation = _modelData.JointAnimations[0];
            ApplyJointAnimation(animation, _timer);
        }        

        /// <summary>
        /// Plays the animation by given name, if it exists.
        /// </summary>
        /// <param name="name">Name or ID of the animation</param>
        /// <param name="timeAdvance">time advanced in seconds</param>
        public void PlayAnimation(string name, float timeAdvance)
        {
            JointAnimation animation = null;
            bool found = _animationsByName.TryGetValue(name, out animation);

            if (!found)
            {
                found = _animationsById.TryGetValue(name, out animation);

                if (!found)
                {
                    throw new ArgumentException("No animation called '" + name + "' was found");
                }
            }
        }

        public void PlayAnimation(int index, float timeAdvance)
        {
            PlayAnimation(Data.JointAnimations[index], timeAdvance);
        }

        public void PlayAnimation(JointAnimation animation, float timeAdvance)
        {
            _timer += timeAdvance;            
            ApplyJointAnimation(animation, _timer);
        }

        /// <summary>
        /// Sets the current time of the animation playback
        /// </summary>
        /// <param name="animation">Animation</param>
        /// <param name="time">time (seconds)</param>
        public void SetAnimationTime(JointAnimation animation, float time)
        {
            _timer = time;
            ApplyJointAnimation(animation, _timer);
        }

        #endregion

        #region IModel Member

        /// <summary>
        /// Gets the data of the underlying model
        /// </summary>
        public ModelData Data
        {
            get { return _modelData; }
        }

        /// <summary>
        /// Draws the model 
        /// </summary>
        /// <param name="world">Object to World matrix</param>
        /// <param name="view">World to View matrix</param>
        /// <param name="projection">View to Projection matrix</param>
        /// <param name="cameraPosition">Camera position</param>
        public void Draw(Matrix world, Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            GraphicsDevice graphicsDevice = _modelData.Meshes[0].Materials[0].Effect.GraphicsDevice;            
            graphicsDevice.SetVertexBuffer(null);

            // Update joint transforms
            ApplyParentTransform(_joints[_joints.Count - 1]);

            // Do the skinning by applying weighted joint transforms to vertices
            TransformVertices();

            int i = 0, j = 0;

            foreach (ModelMesh mesh in _modelData.Meshes)
            {
                // Update shared effect parameters                
                foreach (var material in mesh.Materials)
                {
                    material.View = view;
                    material.Projection = projection;
                    material.CameraPosition = cameraPosition;
                }                               

                /* TODO: Cull-Mode nur noch on-change ändern (neue Instanz RasterizerState benötigt)
                graphicsDevice.RasterizerState.CullMode = EnableCulling ?
                    CullMode.CullCounterClockwiseFace : CullMode.None;                
                */

                foreach (ModelMeshPart part in mesh.Parts)
                {
                    graphicsDevice.SetVertexBuffer(_vertexBuffers[i][j]);                    
                    graphicsDevice.Indices = part.IndexBuffer;                    

                    foreach (var instance in mesh.Instances)
                    {
                        part.Material.World = instance.AbsoluteTransform * _modelData.RootJoint.Transform * world;                        

                        foreach (EffectPass pass in part.Material.Effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                0, part.NumVertices, 0, part.NumVertices / 3);                            
                        }
                    }
                    
                    j++;
                }

                i++;
                j = 0;
            }
        }

        #endregion
    }

    public static class VectorExtensions
    {
        public static float Get(this Vector4 v, int index)
        {
            switch (index)
            {
                case 0:
                    return v.X;

                case 1:
                    return v.Y;

                case 2:
                    return v.Z;

                case 3:
                    return v.W;

                default:
                    throw new ArgumentOutOfRangeException("index",
                        "index must be 0 (X), 1 (Y), 2 (Z) or 3 (W)");
            }
        }

        public static float Get(this Vector3 v, int index)
        {
            switch (index)
            {
                case 0:
                    return v.X;

                case 1:
                    return v.Y;

                case 2:
                    return v.Z;

                default:
                    throw new ArgumentOutOfRangeException("index",
                        "index must be 0 (X), 1 (Y), 2 (Z) or 3 (W)");
            }
        }
    }
}
