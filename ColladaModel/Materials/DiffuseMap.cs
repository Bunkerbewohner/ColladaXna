using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Model.Materials
{
    public class DiffuseMap : TextureProperty
    {
        public override string Name
        {
            get { return "DiffuseMap"; }
        }

        public override string Code
        {
            // [T]e[x]ture
            get { return "Tx"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "Texture"
        };

        public DiffuseMap()
        {
            
        }

        public DiffuseMap(TextureReference texture)
        {
            Texture = texture;
        }
    }
}
