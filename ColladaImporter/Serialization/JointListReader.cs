using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Seafarer.Xna.Collada.Importer.Animation;

namespace Seafarer.Xna.Collada.Importer.Serialization
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content
    /// Pipeline to read the specified data type from binary .xnb format.
    /// 
    /// Unlike the other Content Pipeline support classes, this should
    /// be a part of your main game project, and not the Content Pipeline
    /// Extension Library project.
    /// </summary>
    public class JointListReader : ContentTypeReader<JointList>
    {
        protected override JointList Read(ContentReader input, JointList existingInstance)
        {                        
            List<Action> fixUps = new List<Action>();            

            int count = input.ReadInt32();           
            Joint[] joints = new Joint[count];     

            for (int i = 0; i < count; i++)
            {
                // Basis info
                Joint joint = new Joint();
                joint.Name = input.ReadString();
                joint.Index = input.ReadInt32();
                joint.Transform = input.ReadMatrix();
                joint.AbsoluteTransform = input.ReadMatrix();
                joint.InvBindPose = input.ReadMatrix();

                // Parent
                int parentIndex = input.ReadInt32();
                if (parentIndex >= 0)
                {
                    //fixUps.Add( () => joint.Parent = joints[parentId] );                    
                    fixUps.Add( () => SetParent(joint, joints, parentIndex) );
                }                                            

                // Children
                int numChildren = input.ReadInt32();

                if (numChildren > 0)
                {
                    joint.Children = new JointList(numChildren);

                    for (int j = 0; j < numChildren; j++)
                    {
                        int childIndex = input.ReadInt32();
                        //fixUps.Add( () => joint.Children.Add(joints[childIndex]) );
                        fixUps.Add( () => AddChild(joint, joints, childIndex) );
                    }
                }

                joints[i] = joint;
            }


            try
            {
                foreach (var fixup in fixUps) fixup.Invoke();
            }
            catch (IndexOutOfRangeException)
            {                
                // ignore this exception for now as it doesn't seem to affect
                // models. for some reason invalid data gets written but isn't
                // used by ModelReader
                // TODO: Find cause for mysterious IndexOutOfRangeException in JointListReader
            }

            return new JointList(joints);
        }

        private static void SetParent(Joint joint, Joint[] joints, int index)
        {
            joint.Parent = joints[index];
        }

        private static void AddChild(Joint joint, Joint[] joints, int index)
        {
            joint.Children.Add(joints[index]);
        }
    }    
}
