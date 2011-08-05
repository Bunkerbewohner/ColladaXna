using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Seafarer.Xna.Collada.Importer.Animation;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Seafarer.Xna.Collada.Content.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class JointAnimationListWriter : ContentTypeWriter<JointAnimationList>
    {
        protected override void Write(ContentWriter output, JointAnimationList value)
        {
            output.Write(value.Count);

            foreach (JointAnimation anim in value)
            {
                // Address
                output.Write(anim.Name ?? "");
                output.Write(anim.ScopedID ?? "");
                output.Write(anim.GlobalID ?? "");

                // Channels
                output.Write(anim.Channels.Length);

                foreach (var channel in anim.Channels)
                {
                    // Index of Target Joint (references IntermediateModel.Joints)
                    output.Write(channel.Target.Index);

                    // Sampler
                    var sampler = channel.Sampler;

                    // Behaviour
                    output.Write((int)sampler.Interpolation);
                    output.Write((int)sampler.PreBehaviour);
                    output.Write((int)sampler.PostBehaviour);

                    // Keyframes
                    output.Write(sampler.Keyframes.Length);

                    foreach (var keyframe in sampler.Keyframes)
                    {
                        output.Write(keyframe.Time);
                        output.Write(keyframe.Scale);
                        output.Write(keyframe.Rotation);
                        output.Write(keyframe.Translation);
                    }
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Seafarer.Xna.Collada.Importer.Serialization.JointAnimationListReader, ColladaImporter";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(JointAnimationList).AssemblyQualifiedName;
        }
    }
}
