﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ColladaXna.Base.Materials
{
    public abstract class ColorProperty : MaterialProperty
    {
        public Color Color { get; set; }

        public override Object GetValue()
        {
            return Color;
        }
    }
}
