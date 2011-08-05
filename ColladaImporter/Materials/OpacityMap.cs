using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// An opacity map is a texture that defines
    /// the transparency of the material at different
    /// locations.
    /// </summary>
    public class OpacityMap : TextureProperty
    {
        public OpacityMap()
        {
            
        }
        
        public OpacityMap(TextureReference texture)
        {
            Texture = texture;
        }

        public override string Name
        {
            get { return "OpacityMap"; }
        }

        public override string Code
        {
            get { return "Om"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "Texture"
        };
    }
}
