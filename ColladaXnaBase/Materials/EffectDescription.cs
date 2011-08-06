using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace ColladaXna.Base.Materials
{
    /// <summary>
    /// Description of an effect
    /// </summary>
    public class EffectDescription
    {        
        // parameters and their default values
        Dictionary<String,Object> _parameters = new Dictionary<String, Object>();

        /// <summary>
        /// Name of the effect
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Name of the Effect source code (HLSL) file
        /// </summary>
        public String Filename { get; set; }

        /// <summary>
        /// HLSL-Code of the effect, might be empty if a filename is set.
        /// In this case the source has to be loaded from the referenced file.
        /// </summary>
        public String Code { get; set; }

        /// <summary>
        /// Gets Names of used parameters, for example "World", "View", "Projection".
        /// Parameters for material properties should correspond to their respective
        /// names (MaterialProperty.Name).
        /// </summary>
        /// <see>MaterialProperty.Name</see>
        public Dictionary<String,Object> Parameters { get { return _parameters; } }

        /// <summary>
        /// Creates a new effect description
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filename"></param>
        /// <param name="parameters"></param>
        public EffectDescription(String name, String filename, Dictionary<String,Object> parameters)
        {
            Name = name;
            Filename = filename;

            foreach (var param in parameters)
            {
                _parameters.Add(param.Key, param.Value);
            }
        }

        public void AddParameters(Dictionary<String,Object> parameters)
        {
            foreach (var param in parameters)
            {
                _parameters.Add(param.Key, param.Value);
            }
        }

        public EffectDescription()
        {
        }
    }
}
