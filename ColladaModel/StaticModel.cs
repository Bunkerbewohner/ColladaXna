using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omi.Xna.Collada.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Omi.Xna.Collada.Model.Materials;
using ModelMesh = Omi.Xna.Collada.Model.ModelMesh;
using ModelMeshPart = Omi.Xna.Collada.Model.ModelMeshPart;

namespace Omi.Xna.Collada.Model
{
    /// <summary>
    /// Represents a static model which allows to draw 
    /// the static meshes of a model while not considering skin
    /// information.
    /// </summary>
    public class StaticModel : IModel
    {
        protected ModelData _modelData;        

        /// <summary>
        /// Creates a new instance of a static model
        /// based on the given model data. The given model data
        /// is never modified by this class.
        /// </summary>
        /// <param name="modelData">Model data</param>
        public StaticModel(ModelData modelData)
        {
            _modelData = modelData;
            EnableCulling = true;

            // Check if alpha blending is needed
            if (modelData.Effects.Values.Any(effect => effect.Material.Properties.Any(property =>
                (property is Opacity && (property as Opacity).Value < 1) || property is OpacityMap)))
            {
                EnableAlphaBlending = true;
            }
        }

        #region IModel Member

        /// <summary>
        /// Gets or sets the EnableCulling option.
        /// If set to false no culling is performed.
        /// Culling is enabled by default.
        /// </summary>        
        public bool EnableCulling { get; set; }

        /// <summary>
        /// Gets or sets the EnableAlphaBlending option.
        /// If set to true alpha blending is used to draw
        /// transparent materials.
        /// </summary>
        public bool EnableAlphaBlending { get; set; }

        /// <summary>
        /// Gets the data of the underlying model
        /// </summary>
        public ModelData Data
        {
            get { return _modelData; }
        }

        /// <summary>
        /// Draws this model
        /// </summary>
        /// <param name="world">World transform matrix</param>
        /// <param name="view">World to View matrix</param>
        /// <param name="projection">View to Projection matrix</param>
        /// <param name="cameraPosition">Position of the camera</param>
        public virtual void Draw(Matrix world, Matrix view, Matrix projection, Vector3 cameraPosition)
        {
            foreach (ModelMesh mesh in _modelData.Meshes)
            {
                // Update shared effect parameters                
                foreach (var material in mesh.Materials)
                {
                    if (material.Effect is BasicEffect)
                    {
                        BasicEffect basic = material.Effect as BasicEffect;
                        basic.Projection = projection;
                        basic.View = view;
                    }
                    else
                    {
                        material.View = view;
                        material.Projection = projection;
                        material.CameraPosition = cameraPosition;
                    }
                }

                GraphicsDevice graphicsDevice = mesh.Materials[0].Effect.GraphicsDevice;
                VertexDeclaration vertexDeclaration = mesh.Parts[0].VertexDeclaration;
                VertexBuffer vertexBuffer = mesh.Parts[0].VertexBuffer;

                graphicsDevice.SetVertexBuffer(mesh.Parts[0].VertexBuffer);                
                
                /*
                graphicsDevice.RasterizerState.CullMode = EnableCulling ?
                    CullMode.CullCounterClockwiseFace : CullMode.None;
                */

                if (EnableAlphaBlending)
                {     
                    // TODO: Fix Alpha Blending
                    /*                     
                    graphicsDevice.RenderState.AlphaBlendEnable = true;                    
                    graphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    graphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    graphicsDevice.RenderState.AlphaTestEnable = true;
                    graphicsDevice.RenderState.ReferenceAlpha = 128;
                    graphicsDevice.RenderState.AlphaFunction = CompareFunction.GreaterEqual;
                    */
                }
                else
                {
                    //graphicsDevice.RenderState.AlphaBlendEnable = false;
                }

                foreach (ModelMeshPart part in mesh.Parts)
                {
                    if (part.VertexBuffer != vertexBuffer)
                        graphicsDevice.SetVertexBuffer(part.VertexBuffer);

                    graphicsDevice.Indices = part.IndexBuffer;                                        

                    foreach (var instance in mesh.Instances)
                    {                        
                        Matrix transform = instance.AbsoluteTransform * _modelData.RootJoint.Transform * world;

                        if (part.Material.Effect is BasicEffect)
                            (part.Material.Effect as BasicEffect).World = transform;
                        else
                            part.Material.World = transform;

                        foreach (EffectPass pass in part.Material.Effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                0, part.NumVertices, 0, part.NumVertices / 3);                            
                        }
                    }                    
                }
            }
        }

        #endregion
    }
}
