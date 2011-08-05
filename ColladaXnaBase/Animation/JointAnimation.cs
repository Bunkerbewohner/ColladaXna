using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using ColladaXna.Base.Geometry;

namespace ColladaXna.Base.Animation
{
    /// <summary>
    /// Animation of a joint node corresponding to animations in library_animations.
    /// Animation clips are composed of one or more animations.
    /// An animation may contain multiple channels, but one channel is default.
    /// </summary>
    public class JointAnimation
    {
        protected JointAnimationChannel[] _channels;

        /// <summary>
        /// Frames per second for animation playback
        /// </summary>
        public int FramesPerSecond { get; set; }

        /// <summary>
        /// Gets the number of frames contained by this animation
        /// </summary>
        public int NumFrames { get; protected set; }

        /// <summary>
        /// Gets the start time of the animation
        /// </summary>
        /// <remarks>This value corresponds to the minimum
        /// start time of all channels
        /// </remarks>
        public float StartTime { get; protected set; }

        /// <summary>
        /// Gets the end time of the animation
        /// </summary>
        /// <remarks>This value corresponds to the maximum
        /// end time of all channels</remarks>
        public float EndTime { get; protected set; }

        /// <summary>
        /// List of Animation Channels the animaton consists of
        /// </summary>
        public JointAnimationChannel[] Channels { get { return _channels; }}    
    
        protected JointAnimation()
        {
            _channels = null;
            NumFrames = 0;
        }

        public JointAnimation(JointAnimationChannel[] channels)
        {
            _channels = channels;

            NumFrames = channels.Max(channel => channel.Sampler.Keyframes.Length);
            StartTime = channels.Min(channel => channel.Sampler.StartTime);
            EndTime = channels.Max(channel => channel.Sampler.EndTime);
        }

        /// <summary>
        /// Samples this animation and applies the result to the target joint list
        /// </summary>
        /// <param name="time"></param>        
        /// <param name="targetJoints"></param>
        public void Sample(float time, JointList targetJoints)
        {
            if (_channels.Length == 0)
                throw new ApplicationException("Animation contains no channels!");

            foreach (var channel in _channels)
            {
                Joint target = targetJoints[channel.Target.Index];
                var keyframe = channel.Sampler.GetInterpolatedKeyframe(time);                

                // TODO: Might be necessary to multiply keyframe transform rather than assign
                target.Transform = keyframe.Transform;                
            }
        }

        /// <summary>
        /// Name corresponding to the XML attribute "name"
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Document wide unique ID corresponding to the XML attribute "id".
        /// </summary>
        public string GlobalID { get; set; }

        /// <summary>
        /// Scoped Identifier corresponding to the XML attribute "sid"
        /// </summary>
        public string ScopedID { get; set; }
    }
}
