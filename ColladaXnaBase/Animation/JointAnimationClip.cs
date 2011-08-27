using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Animation
{
    /// <summary>
    /// List of animations that can be played in order.
    /// TODO: Still has to be imported by the AnimationImporter
    /// </summary>
    public class JointAnimationClip
    {
        private JointAnimation[] _animations;

        /// <summary>
        /// Name of this animation clip (optional, i.e. might be null)
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Start time of this animation within the referenced animation(s) in seconds.
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// End time of this animation within the referenced animation(s) in seconds.
        /// </summary>
        public TimeSpan EndTime { get; set; }
        
        /// <summary>
        /// List of animations that are part of this clip
        /// </summary>
        public JointAnimation[] Animations
        {
            get { return _animations; }
        }

        /// <summary>
        /// Creates a new animation clip consisting of given animations. 
        /// </summary>
        /// <param name="animations">Animations to play</param>
        /// <param name="startTime">Start time within animations</param>
        /// <param name="endTime">End time within animations</param>
        public JointAnimationClip(JointAnimation[] animations, TimeSpan startTime, TimeSpan endTime)
        {
            _animations = animations;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
