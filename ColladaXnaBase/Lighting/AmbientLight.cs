using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ColladaXna.Base.Lighting
{
    public class AmbientLight : Light
    {
        public Color Color;        

        public override string Name
        {
            get
            {
                return base.Name ?? "AmbientLight";
            }
            set
            {
                base.Name = value;
            }
        }

        public AmbientLight(Color color)
        {
            Color = color;
        }

        public AmbientLight()
            : this(Color.Black)
        {
        }
    }
}
