using System;
using Omi.Xna.Collada.Model.Animation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Omi.Xna.Collada.Model.Deserialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class JointAnimationListReader : ContentTypeReader<JointAnimationList>
    {
        protected override JointAnimationList Read(ContentReader input, JointAnimationList existingInstance)
        {
            int numAnimations = input.ReadInt32();
            JointAnimationList animations = new JointAnimationList(numAnimations);

            for (int i = 0; i < numAnimations; i++)
            {
                // Address
                string name = input.ReadString();
                if (name == String.Empty) name = null;

                string sid = input.ReadString();                
                if (sid == String.Empty) sid = null;

                string gid = input.ReadString();
                if (gid == String.Empty) gid = null;

                // Channels
                int numChannels = input.ReadInt32();
                JointAnimationChannel[] channels = new JointAnimationChannel[numChannels];

                for (int j = 0; j < numChannels; j++)
                {
                    // Surrogate Joint (must be synchronized with actual joint of the model by index later)
                    Joint surrogate = new Joint("surrogate");
                    surrogate.Index = input.ReadInt32();

                    // Sampler
                    var interpolation = (AnimationInterpolation)Enum.Parse(typeof(AnimationInterpolation), 
                        input.ReadInt32().ToString());

                    var preBehaviour = (AnimationBehaviour) Enum.Parse(typeof (AnimationBehaviour),
                        input.ReadInt32().ToString());

                    var postBehaviour = (AnimationBehaviour)Enum.Parse(typeof(AnimationBehaviour),
                        input.ReadInt32().ToString());

                    // Keyframes
                    int numKeyframes = input.ReadInt32();
                    JointAnimationKeyFrame[] keyframes = new JointAnimationKeyFrame[numKeyframes];

                    for (int k = 0; k < numKeyframes; k++)
                    {
                        float time = input.ReadSingle();
                        Vector3 scale = input.ReadVector3();
                        Quaternion rotation = input.ReadQuaternion();
                        Vector3 translation = input.ReadVector3();

                        keyframes[k] = new JointAnimationKeyFrame(time, scale, rotation, translation);
                    }

                    var sampler = new JointAnimationSampler(keyframes, interpolation);
                    var channel = new JointAnimationChannel(sampler, surrogate);

                    channels[j] = channel;
                }

                JointAnimation anim = new JointAnimation(channels);
                anim.Name = name;
                anim.GlobalID = gid;
                anim.ScopedID = sid;

                animations.Add(anim);
            }

            return animations;
        }        
    }
}
