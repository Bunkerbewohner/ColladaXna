using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Materials
{
    public abstract class TextureProperty : MaterialProperty
    {
        public TextureReference Texture { get; set; }

        public override Object GetValue()
        {
            return Texture;
        }
    }
}
