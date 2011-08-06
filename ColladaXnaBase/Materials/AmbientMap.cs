using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Materials
{
    public class AmbientMap : TextureProperty
    {
        public override string Name
        {
            get { return "AmbientMap"; }
        }

        public override string Code
        {
            get { return "Am"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "Texture"
        };

        public AmbientMap()
        {
            
        }

        public AmbientMap(TextureReference texture)
        {
            Texture = texture;
        }
    }
}
