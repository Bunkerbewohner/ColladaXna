using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    public abstract class TextureProperty : MaterialProperty
    {
        public TextureReference Texture { get; set; }
    }
}
