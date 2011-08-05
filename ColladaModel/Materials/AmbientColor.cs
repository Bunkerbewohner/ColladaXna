using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace Omi.Xna.Collada.Model.Materials
{
    public class AmbientColor : ColorProperty, IShaderDefaultValue
    {
        public override string Name
        {
            get { return "AmbientColor"; }
        }

        public override string Code
        {
            get { return "Ac"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "float3"
        };

        public AmbientColor()
        {
            Color = new Color(0.1f, 0.1f, 0.1f);
        }

        public AmbientColor(Color color)
        {
            Color = color;
        }

        #region IShaderDefaultValue Member

        /// <summary>
        /// Returns the default value of the property in HLSL,
        /// e.g. "float3(1,1,1)" 
        /// </summary>
        public string ShaderDefaultValue
        {
            get
            {
                Vector3 v = Color.ToVector3();
                return String.Format(CultureInfo.InvariantCulture, 
                    "float3({0},{1},{2})", v.X, v.Y, v.Z);
            }
        }

        #endregion
    }
}
