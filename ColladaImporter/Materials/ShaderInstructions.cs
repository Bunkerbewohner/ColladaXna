using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// Instructions for incorporating a material into an effect shader.
    /// </summary>
    public class ShaderInstructions
    {
        /// <summary>
        /// Type of parameter, e.g. float4x4, Texture, float, float4 etc.
        /// </summary>
        public String ParameterType;
    }    
}
