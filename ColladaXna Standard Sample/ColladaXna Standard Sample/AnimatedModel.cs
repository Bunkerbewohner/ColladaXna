using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SkinnedModel;

namespace ColladaXna_Standard_Sample
{
    /// <summary>
    /// Wrapper class for animated models. Only works with models that actually contain
    /// animation data and where processed by the SkinnedMeshProcessor! Can be used
    /// to easily play and draw contained animations.
    /// </summary>
    public class AnimatedModel
    {
        /// <summary>
        /// The animated model itself
        /// </summary>
        protected Model model;

        /// <summary>
        /// Animation Controller that is used to apply animations to the model
        /// </summary>
        protected AnimationPlayer player;

        /// <summary>
        /// The underlying skinning data including bone weights and indices
        /// for skinned mesh animation.
        /// </summary>
        protected SkinningData skinningData;

        /// <summary>
        /// Animation Controller
        /// </summary>
        public AnimationPlayer Player
        {
            get { return player; }
        }

        /// <summary>
        /// Creates a new AnimatedModel instance based on the given model, which:
        /// a) already has to be loaded
        /// </summary>
        /// <param name="model"></param>
        public AnimatedModel(Model model)
        {
            this.model = model;

            skinningData = model.Tag as SkinningData;
            if (skinningData == null)
            {
                throw new InvalidOperationException("Model contains no animation data! Did you use the SkinnedModelProcessor?");
            }

            player = new AnimationPlayer(skinningData);
        }

        /// <summary>
        /// Starts playing an animation clip by its name. The default animation clip
        /// in FBX models is called "Take 001" and therefore is default value of the
        /// clipName parameter. Starting the clip doesn't have any effect on the displayed
        /// model until the animation is updated (via Update()).
        /// </summary>
        /// <param name="clipName">Name of the animation clip</param>
        public void Play(String clipName = "Take 001")
        {
            AnimationClip clip = skinningData.AnimationClips[clipName];
            player.StartClip(clip);
        }

        /// <summary>
        /// Advances the animation by given time and updates the model
        /// skin accordingly.
        /// </summary>
        /// <param name="gameTime">Elapsed time</param>
        public void Update(GameTime gameTime)
        {
            player.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);
        }

        /// <summary>
        /// Draws the animated model's transformed skin with regard to any currently playing
        /// animation.
        /// </summary>        
        /// <param name="world">World transform</param>
        /// <param name="view">Camera View transform</param>
        /// <param name="projection">Camera Projection transform</param>
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            Matrix[] bones = player.GetSkinTransforms();

            // Render the skinned mesh.
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    effect.SetBoneTransforms(bones);

                    effect.World = world;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.SpecularColor = new Vector3(0.25f);
                    effect.SpecularPower = 16;
                }

                mesh.Draw();
            }
        }
    }
}
