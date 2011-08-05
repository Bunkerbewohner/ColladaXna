using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Model.Animation
{
    public class JointAnimationList : List<JointAnimation>
    {
        /// <summary>
        /// Dictionary of animations by name, lazilly initialized when first accessed 
        /// per index operator
        /// </summary>
        private Dictionary<string, JointAnimation> _animationsByName;

        public JointAnimationList()
            : base()
        {
            
        }

        public JointAnimationList(IEnumerable<JointAnimation> animations)
            : base(animations)
        {
            
        }

        public JointAnimationList(int capacity)
            : base(capacity)
        {
            
        }

        /// <summary>
        /// Gets an animation contained by this list by its name.
        /// If no animation by this name is found, null is returned.
        /// </summary>
        /// <param name="name">Name of the animation</param>
        /// <returns>Animation of the given name or null, if it doesn't exist</returns>
        public JointAnimation this [string name]
        {
            get
            {
                // Lazy initialization of name dictionary
                if (_animationsByName == null)
                {
                    _animationsByName = Enumerable.ToDictionary(
                        this.Where(anim => !String.IsNullOrEmpty(anim.Name)),
                        anim => anim.Name);
                }

                JointAnimation animation;
                if (_animationsByName.TryGetValue(name, out animation))
                {
                    return animation;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
