using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Materials
{
    public class SpecularMap : TextureProperty
    {
        public override string Name
        {
            get { return "SpecularMap"; }
        }

        public SpecularMap()
        {
            
        }

        public SpecularMap(TextureReference texture)
        {
            Texture = texture;
        }

        public override string Code
        {
            get { return "Sm"; }
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
