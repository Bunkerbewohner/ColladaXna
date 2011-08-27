using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using ColladaXna.Base.Geometry;
using ColladaXna.Base.Animation;
using ColladaXna.Base.Util;

namespace ColladaXna.Base.Import
{
    /// <summary>
    /// Importer for animation data of COLLADA models.
    /// Right now skinned mesh animation is supported.
    /// Morph Target animation is planned.
    /// </summary>
    /// <remarks>This importer needs the joints of the model to work,
    /// thus the SkeletonImporter must be executed beforehand.</remarks>
    public class AnimationImporter : IColladaImporter
    {
        private ColladaModel _model;

        #region IColladaImporter Member

        public void Import(XmlNode xmlRoot, ColladaModel model)
        {
            _model = model;

            // Find skin definitions. Multiple skin definitions are supported
            // but they have to work on disjoint sets (relate to different joints)
            XmlNodeList xmlSkins = xmlRoot.SelectNodes(".//skin");

            foreach (XmlNode xmlSkin in xmlSkins)
            {
                ImportSkin(xmlSkin, model);
            }

            // Next import animations from library_animations
            XmlNodeList xmlAnimations = xmlRoot.SelectNodes("/COLLADA/library_animations/animation");

            ImportAnimations(xmlAnimations, model);

            // Import the animation clips library
            XmlNode xmlNode = xmlRoot.SelectSingleNode("/COLLADA/library_animation_clips");
            ImportAnimationClips(xmlNode, model);
        }

        private void ImportAnimationClips(XmlNode xmlNode, ColladaModel model)
        {
            if (xmlNode == null || model.JointAnimations == null || !model.JointAnimations.Any())
                return;

            XmlNodeList xmlClips = xmlNode.SelectNodes("animation_clip");
            if (xmlClips == null) return;

            foreach (XmlNode xmlClip in xmlClips)
            {
                // Reference name of the animation clip: name > id > sid
                String name = xmlClip.GetAttributeString("name") ??
                              (xmlClip.GetAttributeString("id") ??
                               xmlClip.GetAttributeString("sid"));

                // Start and end times
                float start = float.Parse(xmlClip.Attributes["start"].Value, 
                    CultureInfo.InvariantCulture);

                float end = float.Parse(xmlClip.Attributes["end"].Value, 
                    CultureInfo.InvariantCulture);

                // Animations to be played
                XmlNodeList xmlInstances = xmlClip.SelectNodes("instance_animation");
                if (xmlInstances == null) return;

                List<JointAnimation> animations = new List<JointAnimation>();

                foreach (XmlNode xmlInstance in xmlInstances)
                {
                    // Assume url="#id"
                    String id = xmlInstance.GetAttributeString("url").Substring(1);

                    // Find animation with given id
                    var temp = from a in model.JointAnimations
                               where a.GlobalID.Equals("id")
                               select a;

                    animations.AddRange(temp);
                }

                JointAnimationClip clip = new JointAnimationClip(animations.ToArray(), 
                    TimeSpan.FromSeconds(start), TimeSpan.FromSeconds(end));

                model.JointAnimationClips.Add(clip);
            }
        }

        #endregion

        /// <summary>
        /// Imports skin information
        /// </summary>
        /// <remarks>Joint weights and indices defined by skins are imported
        /// by GeometryImporter since these values have to be added to the 
        /// vertex container of the meshes.</remarks>
        /// <param name="xmlSkin"></param>
        /// <param name="model"></param>
        static void ImportSkin(XmlNode xmlSkin, ColladaModel model)
        {
            Mesh mesh = GetMeshFromSkin(xmlSkin, model);
            if (mesh == null)
            {
                throw new Exception("Mesh referenced by skin not found");
            }

            // TODO: Take bind_shape_matrix into consideration for animation

            // Read inverse bind-pose matrices and save them in the corresponding joints
            GetUpdatedJointsFromSkin(xmlSkin, model);                                   
        }

        static string ExtractNodeIdFromTarget(string target)
        {
            return target.Substring(0, target.IndexOf('/'));
        }

        static bool DoesAnimationAffectJoints(Dictionary<string, Joint> joints, XmlNode xmlAnimation)
        {
            XmlNode xmlChannel = xmlAnimation.SelectSingleNode("//channel");
            if (xmlChannel == null)
            {
                throw new Exception("Animation '" + xmlAnimation.Attributes["id"].Value + 
                    "' does not contain a channel:" + xmlAnimation.ChildNodes.Count);
            }

            string target = xmlChannel.Attributes["target"].Value;

            return joints.ContainsKey(ExtractNodeIdFromTarget(target));
        }

        static void ImportAnimations(XmlNodeList xmlAnimations, ColladaModel model)
        {            
            // Joint dictionary
            Dictionary<string, Joint> joints = model.Joints.ToDictionary(joint => joint.GlobalID);

            // Import single animations
            List<JointAnimation> animations = (from XmlNode xmlAnim in xmlAnimations 
                                               where DoesAnimationAffectJoints(joints, xmlAnim)
                                               select ImportAnimation(xmlAnim, joints)).ToList();   
         
            // Combine those affecting the same joint to one animation            
            var combined = from c in 
                              (from anim in animations          
                               where anim.Channels.Length == 1
                               group anim by anim.Channels[0].Target)
                           select CombineAnimations(c);

            // List of those who cannot be joined (because they have multiple channels)
            var rest = from anim in animations where anim.Channels.Length > 1 select anim;

            animations = combined.Union(rest).ToList();

            // TODO: Implement Animation Clips (missing example so far)
            
            // Try to merge all animations (must be extended to consider animation clips)
            // NOTE: Didn't work for Maya animations, something's still of with them
            if (animations.Count > 1 && false)
            {
                try
                {
                    var animation = MergeAnimations(animations);
                    animations.Clear();
                    animations.Add(animation);
                }
                catch (Exception)
                {
                    // didn't work, ignore
                }
            }

            model.JointAnimations = new JointAnimationList(animations);
        }


        /// <summary>
        /// Takes a list of animations each having one channel with a distinct target
        /// and creates a single animation of them by storing all channels of the
        /// input animations in one resulting animation instance.
        /// </summary>
        /// <remarks>This is necessary because some DCC tools split one "real" animation
        /// up into several animation tags in the collada file.</remarks>
        /// <param name="animations">List of animations</param>
        /// <returns>Merged Animation</returns>
        static JointAnimation MergeAnimations(IEnumerable<JointAnimation> animations)
        {
            JointAnimation animation = null;

            if ((from a in animations select a.Channels[0].Target).Distinct().Count() != animations.Count() ||
                (from a in animations select a.Channels.Length).Max() > 1)
            {
                throw new Exception("Only animations with one channel each targetting distinct joints can be merged");
            }

            var channels = (from a in animations select a.Channels[0]).ToArray();

            animation = new JointAnimation(channels);

            return animation;
        }

        /// <summary>
        /// Takes a group of animations which affect the same joint and combines them into
        /// one animation with one channel. For this all transformations of each individual
        /// keyframe are combined. Translations are added, Scales multiplied and rotations
        /// multiplied as well.
        /// </summary>
        /// <param name="animations">List of animations</param>
        /// <returns>Combined animation</returns>
        static JointAnimation CombineAnimations(IEnumerable<JointAnimation> animations)
        {
            if (animations.Count() == 1)
            {
                // If there is only one animation there's no need to join
                return animations.First();
            }
            
            // Number of frames that have to be combined
            int numFrames = animations.First().NumFrames;

            // All animations must have the same number of frames
            if(!animations.All(a => a.NumFrames == numFrames))
            {
                throw new NotImplementedException("Animations affecting the same joint must " + 
                    "have the same number of keyframes");
            }

            var combinedKeyframes = new JointAnimationKeyFrame[numFrames];

            for (int i = 0; i < numFrames; i++)
            {
                // Create a combined key frame
                float time = animations.First().Channels[0].Sampler.Keyframes[i].Time;

                Vector3 scale = new Vector3(1,1,1);
                Quaternion rotation = Quaternion.Identity;
                Vector3 translation = new Vector3(0,0,0);
                
                foreach (var add in animations.Select(anim => anim.Channels[0].Sampler.Keyframes[i]))
                {
                    if (add.Scale != Vector3.One)
                        scale *= add.Scale;

                    if (add.Translation != Vector3.Zero)                    
                        translation += add.Translation;

                    // Single axis rotations are executed in order (as defined in the document)
                    // Note: Not sure if this is correct
                    if (add.Rotation != Quaternion.Identity)
                        rotation = add.Rotation * rotation;
                }

                var keyframe = new JointAnimationKeyFrame(time, scale, rotation, translation);
                combinedKeyframes[i] = keyframe;
            }

            Joint target = animations.First().Channels[0].Target;
            var sampler = new JointAnimationSampler(combinedKeyframes, AnimationInterpolation.Linear);

            JointAnimation animation = new JointAnimation(new JointAnimationChannel[]
                                          {
                                              new JointAnimationChannel(sampler, target)
                                          });

            // Names
            if (animations.Any(a => !String.IsNullOrEmpty(a.Name)))
            {
                animation.Name = animations.Where(a => !String.IsNullOrEmpty(a.Name)).
                    Select(a => a.Name).Aggregate((sum, name) => sum + "+" + name);
            }

            if (animations.Any(a => !String.IsNullOrEmpty(a.GlobalID)))
            {
                animation.GlobalID = animations.Where(a => !String.IsNullOrEmpty(a.GlobalID)).
                    Select(a => a.GlobalID).Aggregate((sum, name) => sum + "\n" + name);                
            }

            return animation;
        }

        static JointAnimation ImportAnimation(XmlNode xmlAnimation, Dictionary<string,Joint> joints)
        {
            XmlNodeList xmlChannels = xmlAnimation.SelectNodes("//channel");
            List<JointAnimationChannel> channels = new List<JointAnimationChannel>();

            foreach (XmlNode xmlChannel in xmlChannels)
            {
                // Target joint
                string target = xmlChannel.Attributes["target"].Value;
                string jointId = ExtractNodeIdFromTarget(target);
                if (!joints.ContainsKey(jointId))
                {
                    continue;
                    //throw new ApplicationException("Animated Joint '" + jointId + "' not found");
                }

                Joint joint = joints[jointId];                

                // Sampler
                string samplerId = xmlChannel.Attributes["source"].Value.Substring(1);
                XmlNode xmlSampler = xmlAnimation.SelectSingleNode("//sampler[@id='" + samplerId + "']");
                if (xmlSampler == null)
                    throw new ApplicationException("Animation Sampler '" + samplerId + "' not found");

                var sampler = ImportSampler(xmlAnimation, xmlSampler, target);

                channels.Add(new JointAnimationChannel(sampler, joint));
            }

            JointAnimation animation = new JointAnimation(channels.ToArray());

            if (xmlAnimation.Attributes["id"] != null)
                animation.GlobalID = xmlAnimation.Attributes["id"].Value;

            if (xmlAnimation.Attributes["name"] != null)
                animation.Name = xmlAnimation.Attributes["name"].Value;

            if (xmlAnimation.Attributes["sid"] != null)
                animation.ScopedID = xmlAnimation.Attributes["sid"].Value;

            return animation;
        }

        /// <summary>
        /// Imports an animation sampler.
        /// Right now only LINEAR interpolation is supported, thus any other definitions
        /// are simply ignored and LINEAR is used instead.
        /// </summary>
        /// <param name="xmlAnimation">XML animation node</param>
        /// <param name="xmlSampler">XML sampler node</param>
        /// <param name="target">Value of sampler's target attribute</param>
        /// <returns>Animation Sampler</returns>
        // TODO: Implement import of other interpolation methods than LINEAR
        static JointAnimationSampler ImportSampler(XmlNode xmlAnimation, XmlNode xmlSampler,
            string target)
        {            
            // Input and Output sources
            Source input = Source.FromInput(xmlSampler.SelectSingleNode("input[@semantic='INPUT']"),
                                            xmlAnimation);
            Source output = Source.FromInput(xmlSampler.SelectSingleNode("input[@semantic='OUTPUT']"),
                                             xmlAnimation);

            // Target (matrix, translation.*, rotation.*, scale.*)
            target = target.Substring(target.IndexOf('/')+1);

            // It is assumed that TIME is used for input
            var times = input.GetData<float>();

            var keyframes = new JointAnimationKeyFrame[times.Count];

            if (target == "matrix")
            {
                // Baked matrices were used so that there is one animation per joint animating
                // the whole transform matrix. Now we just need to save these transforms in keyframes
                var transforms = output.GetData<Matrix>();
                Debug.Assert(transforms.Count == times.Count);                

                for (int i = 0; i < times.Count; i++)
                {
                    keyframes[i] = new JointAnimationKeyFrame(times[i], transforms[i]);                    
                }
            }
            else if (output.DataType == typeof(float))
            {
                // No baked matrices, i.e. there is an animation for each component of the transform
                var data = output.GetData<float>();
                Debug.Assert(times.Count == data.Count);

                for (int i = 0; i < times.Count; i++)
                {
                    keyframes[i] = CreateKeyFrameFromSingle(target, data[i], times[i]);
                }
            }
            else if (output.DataType == typeof(Vector3))
            {
                var data = output.GetData<Vector3>();
                Debug.Assert(times.Count == data.Count);

                for (int i = 0; i < times.Count; i++)
                {
                    keyframes[i] = CreateKeyFrameFromVector(target, data[i], times[i]);
                }
            }

            return new JointAnimationSampler(keyframes, AnimationInterpolation.Linear);
        }

        /// <summary>
        /// Creates a key frame by interpreting the target string and deriving a transform
        /// matrix from it, e.g. "translation.X" results in a translation matrix that only
        /// translates X and so on.
        /// </summary>
        /// <param name="target">Target string (e.g. translation.X, rotation_y.ANGLE, scale.Z, ...)</param>
        /// <param name="datum">Value to derive transformation from, i.e. translation offset, rotation angle, scale factor</param>
        /// <param name="time">Time key for the key frame</param>
        /// <returns>A key frame</returns>
        static JointAnimationKeyFrame CreateKeyFrameFromVector(string target, Vector3 datum, float time)
        {
            Vector3 S = new Vector3(1, 1, 1);
            Quaternion R = Quaternion.Identity;
            Vector3 T = new Vector3(0, 0, 0);

            string comp = target.ToLower();
                
            // Using name semantics - that's not the proper way to do it, but it works most of the time)
            // TODO: Correctly use references for channel targets
            if (comp.Contains("trans"))
                T = new Vector3(datum.X, datum.Y, datum.Z);                    

            else if (comp.Contains("rot"))
                // Note: Might not be right, needs to be verified
                R = Quaternion.CreateFromYawPitchRoll(datum.X, datum.Y, datum.Z);                    

            else if (comp.Contains("scal"))
                S = new Vector3(datum.X, datum.Y, datum.Z);                    

            else                
                Debug.WriteLine("Unsupported animation target '" + target + "' was ignored");             

            return new JointAnimationKeyFrame(time, S, R, T);
        }

        /// <summary>
        /// Creates a key frame by interpreting the target string and deriving a transform
        /// matrix from it, e.g. "translation.X" results in a translation matrix that only
        /// translates X and so on.
        /// </summary>
        /// <param name="target">Target string (e.g. translation.X, rotation_y.ANGLE, 
        /// scale.Z, ...)</param>
        /// <param name="datum">Value to derive transformation from, i.e. translation offset, 
        /// rotation angle, scale factor</param>
        /// <param name="time">Time key for the key frame</param>
        /// <returns>A key frame</returns>
        static JointAnimationKeyFrame CreateKeyFrameFromSingle(string target, float datum, 
            float time)
        {
            Vector3 S = new Vector3(1,1,1);
            Quaternion R = Quaternion.Identity;
            Vector3 T = new Vector3(0,0,0);

            string comp = target.ToLower();

            // Using name semantics instead of references
            // TODO: Implement proper channel target reference handling
            if (comp.Contains("trans"))
            {
                if (comp.Contains("x"))
                    T = new Vector3(datum, 0, 0);                

                else if (comp.Contains("y"))
                    T = new Vector3(0, datum, 0);                

                else if (comp.Contains("z"))
                    T = new Vector3(0, 0, datum);                
            }

            if (comp.Contains("rot"))
            {
                if (comp.Contains("angle"))
                {
                    if (comp.Contains("x"))
                        R = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), 
                            MathHelper.ToRadians(datum));                    

                    else if (comp.Contains("y"))
                        R = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), 
                            MathHelper.ToRadians(datum));                    

                    else if (comp.Contains("z"))
                        R = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), 
                            MathHelper.ToRadians(datum));                    
                }
            }

            if (comp.Contains("scal"))
            {
                if (comp.Contains("x"))
                    S = new Vector3(datum, 0, 0);                

                else if (comp.Contains("y"))
                    S = new Vector3(0, datum, 0);                

                else if (comp.Contains("z"))
                    S = new Vector3(0, 0, datum);                
            }

            return new JointAnimationKeyFrame(time, S, R, T);
        }

        #region Helper Methods and XML Parsing

        /// <summary>
        /// Retrieves all the joints referenced by the given skin. The bones are only
        /// references to the bones contained by the given model! While fetching
        /// the bones (by following references in XML) this method updates the
        /// inverse bind-pose matrix of each bone according to the XML INV_BIND_MATRIX
        /// input as found in the XML skin definition.
        /// </summary>
        /// <param name="xmlSkin">XML skin node</param>
        /// <param name="model">Model instance</param>
        /// <returns>Used bones with updated inverse bind-pose matrices</returns>
        static List<Joint> GetUpdatedJointsFromSkin(XmlNode xmlSkin, ColladaModel model)
        {
            if (model.Joints.Count <= 1)
            {
                throw new ArgumentException("Model does not contain any joints");
            }            

            // Inverse Bind-Pose Matrices
            XmlNode xmlMatInput = xmlSkin.SelectSingleNode("joints/input[@semantic='INV_BIND_MATRIX']");
            Source matrixSource = Source.FromInput(xmlMatInput, xmlSkin);            
            List<Matrix> invBindMatrices = matrixSource.GetData<Matrix>();

            // Joints (as referenced by this skin)
            XmlNode xmlJointInput = xmlSkin.SelectSingleNode("joints/input[@semantic='JOINT']");
            Source jointSource = Source.FromInput(xmlJointInput, xmlSkin);                       
            List<string> names = jointSource.GetData<string>();
            List<Joint> joints = new List<Joint>(names.Count);
            int i = 0;            
            
            // Dictionary of joints in model by their name
            Dictionary<string, Joint> modelJoints = model.Joints.ToDictionary(j => 
                j.GetAddressPart(jointSource.ColladaType));

            // Check if the names actually refer to the joints in the dictionary
            if (modelJoints.ContainsKey(names[0]) == false)
            {
                // As of COLLADA 1.4 "name" can refer to name OR sid attribute!
                // If the former didn't work try the latter
                if (jointSource.ColladaType == "name")
                {
                    // Only elements that actually have a sid can be part of the dictionary.
                    // Elements which don't have a SID usually aren't referenced, so they are 
                    // not needed in the dictionary anyway
                    modelJoints = model.Joints.Where(j => j.GetAddressPart("sid") != null).
                        ToDictionary(j => j.GetAddressPart("sid"));
                }

                // If that still didn't help it's hopeless
                if (modelJoints.ContainsKey(names[0]) == false)
                {
                    throw new ApplicationException("Invalid joint references in skin definition");
                }
            }

            // Look up bones within the model by name from JOINT input)
            foreach (Joint joint in names.Select(name => modelJoints[name]))
            {
                Debug.Assert(joint != null);

                // while we're at it update inverse bind-pose matrix
                joint.InvBindPose = invBindMatrices[i++];

                joints.Add(joint);
            }

            return joints;
        }

        /// <summary>
        /// Returns the mesh referenced by given skin XML node if that mesh exists
        /// in the given model. Otherwise null is returned.
        /// </summary>
        /// <param name="xmlSkin">XML skin node</param>
        /// <param name="model">Model instance</param>
        /// <returns>instance of Mesh or null</returns>
        static Mesh GetMeshFromSkin(XmlNode xmlSkin, ColladaModel model)
        {
            string meshId = xmlSkin.Attributes["source"].Value.Substring(1);
            XmlNode xmlGeom = xmlSkin.OwnerDocument.DocumentElement.
                SelectSingleNode(".//geometry[@id='" + meshId + "']");

            string name = xmlGeom.Attributes["name"] != null
                              ? xmlGeom.Attributes["name"].Value
                              : meshId;

            return (from mesh in model.Meshes 
                    where mesh.Name.Equals(name) 
                    select mesh).SingleOrDefault();
        }

        #endregion
    }              
}
