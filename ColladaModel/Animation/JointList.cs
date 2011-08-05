using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Model.Animation
{
    public class JointList : List<Joint>
    {
        public JointList()
        {

        }

        public JointList(IEnumerable<Joint> list)
            : base(list)
        {

        }

        public JointList(int capacity)
            : base(capacity)
        {

        }               

        /// <summary>
        /// Creates a copy of this joint list with primary
        /// data only, i.e. transform matrices, but no address
        /// information.
        /// </summary>
        /// <returns></returns>
        public JointList CopyPrimary()
        {
            JointList list = new JointList(Count);

            // First flat copy
            foreach (Joint joint in this)
            {
                Joint copy = new Joint(joint.Name);
                copy.Index = joint.Index;
                copy.InvBindPose = joint.InvBindPose;
                copy.Transform = joint.Transform;

                list.Add(copy);
            }

            // Relations: parent / children
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Parent = this[i].Parent != null ? list[this[i].Parent.Index] : null;

                if (this[i].Children != null)
                {
                    list[i].Children = new JointList(this[i].Children.Count);

                    for (int j = 0; j < this[i].Children.Count; j++)
                    {
                        int jointIndex = this[i].Children[j].Index;
                        list[i].Children.Add(list[jointIndex]);
                    }
                }
            }

            return list;
        }
    }
}
