using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    public abstract class ColorProperty : MaterialProperty
    {
        public Color Color { get; set; }
    }
}
