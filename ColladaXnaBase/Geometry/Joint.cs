using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ColladaXna.Base.Geometry
{
    /// <summary>
    /// Representation of a joint of a skeleton. 
    /// Sometimes also called "Bone".
    /// </summary>
    public class Joint : IAddress
    {
        /// <summary>
        /// Counter for generic joint names (if no name is given)
        /// </summary>
        private static int counter;

        /// <summary>
        /// Id of this joint (is not related to ids defined in COLLADA document)
        /// </summary>        
        private int _id;

        /// <summary>
        /// Name of this joint for convenient access
        /// </summary>
        private string _name;

        /// <summary>
        /// Internal ID of this joint
        /// </summary>
        internal int ID { get { return _id; }}

        /// <summary>
        /// Index of this joint in its joint collection
        /// </summary>
        public int Index;

        /// <summary>
        /// Name of this joint
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                _name = value;
            }
        }

        /// <summary>
        /// Document wide unique ID corresponding to the XML attribute "id".
        /// </summary>
        public string GlobalID { get; set; }

        /// <summary>
        /// Scoped Identifier corresponding to the XML attribute "sid"
        /// </summary>
        public string ScopedID { get; set; }

        /// <summary>
        /// Parent joint or null, if there is none
        /// </summary>
        [ContentSerializer(SharedResource = true)]
        public Joint Parent;

        /// <summary>
        /// List of child joints or null if there are none
        /// </summary>
        public JointList Children;

        /// <summary>
        /// Local Transformation of this joint
        /// </summary>
        public Matrix Transform = Matrix.Identity;

        /// <summary>
        /// Current absolute transformation of this joint,
        /// depending on the transformation of the parent's
        /// absolute transformation.
        /// </summary>
        public Matrix AbsoluteTransform
        {
            get
            {
                if (Parent != null) return Parent.AbsoluteTransform * Transform;
                else return Transform;
            }
        }

        /// <summary>
        /// Inverse Bind-Pose Matrix of this joint
        /// </summary>
        public Matrix InvBindPose;

        /// <summary>
        /// Creates a new Joint by the given name. If no name
        /// is supplied (null) a generic name is created.
        /// </summary>
        /// <param name="name"></param>
        public Joint(string name)
        {
            _id = (++counter);

            if (name != null)
            {
                _name = name;
            }
            else
            {
                _name = "Joint" + _id;
            }            
        }

        public Joint()
            : this(null)
        {
            
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Joint)
            {
                Joint other = (Joint) obj;
                return other.ID == _id;
            }
            
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return "Joint[" + _name + "]";
        }
    }
}
