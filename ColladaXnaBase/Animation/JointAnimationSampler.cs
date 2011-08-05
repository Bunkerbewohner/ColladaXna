using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Animation
{
    /// <summary>
    /// Joint animation sampler. Does only support linear interpolation right now.
    /// If an other interpolation type (like Bézier, Hermite etc.) is defined in the
    /// COLLADA document linear is used instead.
    /// </summary>
    public class JointAnimationSampler
    {        
        /// <summary>
        /// List of key frame samples used by this sampler
        /// </summary>
        public JointAnimationKeyFrame[] Keyframes { get { return _keyframes; }}

        /// <summary>
        /// Interpolation method used to interpolate between two key frames
        /// </summary>
        public AnimationInterpolation Interpolation { get { return _interpolation; } }

        /// <summary>
        /// Gets the Time key of the first frame 
        /// </summary>
        public float StartTime { get; private set; }

        /// <summary>
        /// Gets the Time key of the last frame
        /// </summary>
        public float EndTime { get; private set; }

        /// <summary>
        /// Behaviour for time keys before the first frame.
        /// By default Constant behaviour is set.
        /// </summary>
        public AnimationBehaviour PreBehaviour { get; set; }

        /// <summary>
        /// Behaviour for time keys after the last frame.
        /// By default Constant behaviour is set.
        /// </summary>
        public AnimationBehaviour PostBehaviour { get; set; }

        /// <summary>
        /// Creates a new animation sampler from a list of keyframes
        /// </summary>
        /// <param name="keyframes">List of samples</param>
        /// <param name="interpolation">Interpolation method to use</param>
        public JointAnimationSampler(JointAnimationKeyFrame[] keyframes, 
            AnimationInterpolation interpolation)
        {
            _keyframes = keyframes;
            _interpolation = interpolation;

            if (_interpolation != AnimationInterpolation.Linear)
            {
                // Right now only linear interpolation is supported
                _interpolation = AnimationInterpolation.Linear;
            }

            if (keyframes == null || keyframes.Length == 0)
            {
                throw new ApplicationException("Sampler needs keyframes");
            }

            StartTime = keyframes[0].Time;
            EndTime = keyframes[keyframes.Length - 1].Time;

            PostBehaviour = AnimationBehaviour.Cycle;
        }

        /// <summary>
        /// Calculates a key frame at given time by interpolating between the samples
        /// that are nearest (by time).
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Interpolated key frame for given time key</returns>
        public JointAnimationKeyFrame GetInterpolatedKeyframe(float time)
        {
            if (time < StartTime)
            {
                switch (PreBehaviour)
                {
                    case AnimationBehaviour.Constant:
                        time = StartTime;
                        break;

                    case AnimationBehaviour.Cycle:
                        time = 0;
                        break;

                    default:
                        // TODO: Implement other animation behaviours
                        throw new NotImplementedException("No animation behaviours " + 
                            "other than Constant implemented yet");
                }
            }
            else if (time > EndTime)
            {
                switch (PostBehaviour)
                {
                    case AnimationBehaviour.Constant:
                        time = EndTime;
                        break;

                    case AnimationBehaviour.Cycle:
                        time %= EndTime;
                        break;

                    default:
                        // TODO: Implement other animation behaviours
                        throw new NotImplementedException("No animation behaviours " +
                            "other than Constant implemented yet");
                }
            }                                   

            int keyframeIndex = 0;

            // Find the two frames to interpolate between
            while (_keyframes[keyframeIndex].Time <= time)
                keyframeIndex++;

            if (keyframeIndex > 0) keyframeIndex--;

            JointAnimationKeyFrame frame1 = _keyframes[keyframeIndex];
            JointAnimationKeyFrame frame2 = _keyframes[keyframeIndex + 1];

            if (frame1.Time == time)
                return frame1;

            switch (_interpolation)
            {
                case AnimationInterpolation.Linear:
                    float amount = 1.0f / (frame2.Time - frame1.Time) * (time - frame1.Time);
                    return JointAnimationKeyFrame.Lerp(frame1, frame2, amount);

                default:
                    // TODO: Implement other animation interpolations
                    throw new NotImplementedException("No animation interpolation " +
                        "other than Linear implemented yet");
            }            
        }

        private JointAnimationKeyFrame[] _keyframes;
        private AnimationInterpolation _interpolation;
    }

    /// <summary>
    /// Possible interpolation types (not all of which are supported right now)
    /// </summary>
    public enum AnimationInterpolation
    {        
        /// <summary>
        /// Two frames are interpolated linearly (default setting)
        /// </summary>
        Linear,

        /// <summary>
        /// The first of two frames is selected
        /// </summary>
        Step,

        /// <summary>
        /// The values follows a Bézier spline
        /// </summary>
        Bézier,

        /// <summary>
        /// Hermite Spline
        /// </summary>
        Hermite,

        /// <summary>
        /// Bicubic Spline
        /// </summary>
        Bspline,

        /// <summary>
        /// Cardinal curve
        /// </summary>
        Cardinal
    }

    /// <summary>
    /// Animation behaviour if time key is out of range (before first frame
    /// or after last frame)
    /// </summary>
    public enum AnimationBehaviour
    {
        /// <summary>
        /// Time keys are clamped to the valid range
        /// </summary>
        Constant,

        /// <summary>
        /// Value follows the line given by the last two keys in the sample
        /// </summary>
        Gradient,

        /// <summary>
        /// The key is mapped in the [first_key, last_key] interval so that the 
        /// animation cycles
        /// </summary>
        Cycle,

        /// <summary>
        /// The keys is mapped in the [first_key, last_key] interval so that the 
        /// animation oscillates
        /// </summary>
        Oscillate,

        /// <summary>
        /// The animation continues indefinitely
        /// </summary>
        CycleRelative
    }
}
