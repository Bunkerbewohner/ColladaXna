using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Omi.Xna.Collada.Model.Lighting
{
    public class DirectionalLight : Light
    {
        static int count = 0;
        int nr;

        public static void ResetCount()
        {
            count = 0;
        }

        public Color Color;

        public Vector3 Direction;

        public override string Name
        {
            get
            {
                return base.Name ?? ("DirLight"+nr);
            }
            set
            {
                base.Name = value;
            }
        }

        public DirectionalLight(Vector3 direction, Color color)
        {
            Direction = direction;
            Color = color;
            nr = count++;
        }

        public DirectionalLight()
            : this(Vector3.Zero, Color.Black)
        {
        }
    }
}
