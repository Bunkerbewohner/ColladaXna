using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ColladaXna.Base.Animation
{
    /// <summary>
    /// A key frame of a joint animation describing the transformation of the 
    /// joint at a given time. Transform matrices are decomposed for linear
    /// interpolation of SRT components.
    /// </summary>
    public class JointAnimationKeyFrame
    {
        /// <summary>
        /// Time of this keyframe in seconds
        /// </summary>
        public float Time { get { return _time; } }

        /// <summary>
        /// Gets the Scale transformation
        /// </summary>
        public Vector3 Scale { get { return _scale; } }

        /// <summary>
        /// Gets the Rotation as Quaternion
        /// </summary>
        public Quaternion Rotation { get { return _rotation; } }

        /// <summary>
        /// Gets the Translation vector
        /// </summary>
        public Vector3 Translation { get { return _translation; } }

        /// <summary>
        /// Gets the combined transform matrix
        /// </summary>
        public Matrix Transform { get { return _transform; } }

        /// <summary>
        /// Creates a new keyframe from given time and transformation matrix.
        /// Since internally the joint transformation is stored in form of its
        /// scale, rotation and translation components the matrix has to be 
        /// decomposed within this constructor.
        /// </summary>
        /// <param name="time">Time of keyframe</param>
        /// <param name="transform">Transformation</param>
        public JointAnimationKeyFrame(float time, Matrix transform)
        {
            _time = time;

            if (!transform.Decompose(out _scale, out _rotation, out _translation))
            {
                throw new ApplicationException("Could not decompose transformation matrix");
            }

            _transform = transform;
        }

        /// <summary>
        /// Creates a new keyframe from given time and transformations
        /// </summary>
        /// <param name="time">Time of keyframe</param>
        /// <param name="scale">Scale</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="translation">Translation</param>
        public JointAnimationKeyFrame(float time, Vector3 scale, Quaternion rotation, Vector3 translation)
        {
            _time = time;

            _scale = scale;
            _rotation = rotation;
            _translation = translation;

            _transform = Matrix.CreateScale(_scale) * Matrix.CreateFromQuaternion(_rotation) *
                Matrix.CreateTranslation(_translation);
        }

        /// <summary>
        /// Linear interpolation of two keyframe transformations.
        /// </summary>
        /// <param name="frame1">One keyframe</param>
        /// <param name="frame2">Another keyframe</param>
        /// <param name="amount">Weight of second keyframe between 0.0 and 1.0</param>
        /// <returns>Interpolated transformation matrix</returns>
        public static Matrix LerpTransform(JointAnimationKeyFrame frame1,
            JointAnimationKeyFrame frame2, float amount)
        {
            // Lerp between components
            Vector3 scale = Vector3.Lerp(frame1.Scale, frame2.Scale, amount);
            Quaternion rotation = Quaternion.Lerp(frame1.Rotation, frame2.Rotation, amount);
            Vector3 translation = Vector3.Lerp(frame1.Translation, frame2.Translation, amount);

            return Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) *
                Matrix.CreateTranslation(translation);
        }

        /// <summary>
        /// Linear interpolation of two keyframes. The resulting keyframe will consist
        /// of the interpolated time and transformation.
        /// </summary>
        /// <param name="frame1">One keyframe</param>
        /// <param name="frame2">Another keyframe</param>
        /// <param name="amount">Weight of the second keyframe between 0.0 and 1.0</param>
        /// <returns>Interpolated keyframe</returns>
        public static JointAnimationKeyFrame Lerp(JointAnimationKeyFrame frame1,
            JointAnimationKeyFrame frame2, float amount)
        {
            // Lerp between components
            Vector3 scale = Vector3.Lerp(frame1.Scale, frame2.Scale, amount);
            Quaternion rotation = Quaternion.Lerp(frame1.Rotation, frame2.Rotation, amount);
            Vector3 translation = Vector3.Lerp(frame1.Translation, frame2.Translation, amount);

            // Lerp time
            float time = frame1.Time + (frame2.Time - frame1.Time) * amount;

            return new JointAnimationKeyFrame(time, scale, rotation, translation);
        }

        private float _time = 0.0f;
        private Vector3 _scale = new Vector3(1, 1, 1);
        private Quaternion _rotation = Quaternion.Identity;
        private Vector3 _translation = Vector3.Zero;
        private Matrix _transform;
    }
}
