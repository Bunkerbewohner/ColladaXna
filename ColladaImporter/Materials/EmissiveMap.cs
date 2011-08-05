using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// This property describes self-illumination (emissive color)
    /// based on a texture.
    /// </summary>
    public class EmissiveMap : TextureProperty
    {
        public override string Name
        {
            get { return "EmissiveMap"; }
        }

        public override string Code
        {
            get { return "Em"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "Texture"
        };

        public EmissiveMap()
        {
            
        }

        public EmissiveMap(TextureReference texture)
        {
            Texture = texture;
        }
    }
}
