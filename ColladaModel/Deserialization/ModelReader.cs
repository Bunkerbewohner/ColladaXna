using System.Collections.Generic;
using Omi.Xna.Collada.Model.Materials;
using Microsoft.Xna.Framework.Content;
using Omi.Xna.Collada.Model.Geometry;
using Omi.Xna.Collada.Model.Lighting;
using Omi.Xna.Collada.Model.Animation;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Omi.Xna.Collada.Model.Deserialization
{
    using EffectMaterial = Omi.Xna.Collada.Model.Materials.EffectMaterial;

    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class ModelReader : ContentTypeReader<ModelData>
    {
        protected override ModelData Read(ContentReader input, ModelData existingInstance)
        {
            GraphicsDevice graphicsDevice = GetGraphicsDevice(input);

            //=================================================================
            // read intermediate data from file

            string name = input.ReadString();

            // 1. Materials (converted to effects)
            var materials = input.ReadObject<List<Omi.Xna.Collada.Model.Materials.EffectMaterial>>();

            // 2. Meshes
            var inMeshes = input.ReadObject<List<Mesh>>();

            // 3. MeshInstances
            var inMeshInstances = input.ReadObject<List<MeshInstance>>();

            // 4. Lights
            var inLights = input.ReadObject<List<Light>>();

            // 5. Joints
            var inJoints = input.ReadObject<JointList>();

            // 6. Joint Animations
            var inJointAnimations = input.ReadObject<JointAnimationList>();
            SynchronizeJoints(inJoints, inJointAnimations);

            //=================================================================
            // create runtime model                                

            return ModelData.CreateFromIntermediateData(name, materials, inMeshes, inMeshInstances,
                inLights, inJoints, inJointAnimations, graphicsDevice);
        }

        GraphicsDevice GetGraphicsDevice(ContentReader input)
        {
            IGraphicsDeviceService graphicsDeviceService =
                (IGraphicsDeviceService)input.ContentManager.ServiceProvider.
                GetService(typeof(IGraphicsDeviceService));
            return graphicsDeviceService.GraphicsDevice;
        }

        /// <summary>
        /// Synchronizes joints so that all joints referenced in the JointAnimationList
        /// correspond to instances of the given JointList. This is used after reading
        /// the JointAnimationList from XNB, because in the file there are only joint
        /// indices. Since JointAnimationListReader has no access to the "real" Joints
        /// which are read seperately by JointListReader it can only read indices.
        /// Therefore joints referenced by JointAnimationList are only surrogates 
        /// containing only a valid Index that can be used to synchronize to the
        /// actual joints.
        /// </summary>
        /// <param name="joints">Joints</param>
        /// <param name="animations">Joint Animations</param>
        void SynchronizeJoints(JointList joints, JointAnimationList animations)
        {                        
            foreach (var anim in animations)
            {
                foreach (var channel in anim.Channels)
                {                    
                    int targetIndex = channel.Target.Index;
                    Joint synchronizedTarget = joints[targetIndex];

                    channel.Target = synchronizedTarget;
                }
            }
        }
    }
}
