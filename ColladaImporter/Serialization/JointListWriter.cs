using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = Omi.Xna.Collada.Model.Animation.JointList;
using Omi.Xna.Collada.Model.Animation;

namespace Omi.Xna.Collada.Importer.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class JointListWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write(ContentWriter output, TWrite value)
        {                        
            output.Write(value.Count);
            
            foreach (Joint joint in value)
            {
                output.Write(joint.Name);
                output.Write(joint.Index);
                output.Write(joint.Transform);
                output.Write(joint.AbsoluteTransform);
                output.Write(joint.InvBindPose);

                if (joint.Parent == null) output.Write(-1);
                else output.Write(joint.Parent.Index);

                if (joint.Children != null && joint.Children.Count > 0)
                {
                    output.Write(joint.Children.Count);

                    foreach (var child in joint.Children)
                    {
                        output.Write(child.Index);
                    }
                }
                else
                {
                    output.Write(0);
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Omi.Xna.Collada.Model.Deserialization.JointListReader, OmiXnaColladaModel";
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(JointList).AssemblyQualifiedName;
        }
    }    
}
