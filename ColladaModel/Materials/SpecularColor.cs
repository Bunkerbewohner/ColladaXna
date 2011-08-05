using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omi.Xna.Collada.Model.Materials
{
    public class SpecularColor : ColorProperty, IShaderDefaultValue
    {
        public SpecularColor()
        {
            Color = new Color(0, 0, 1.0f);
        }

        public SpecularColor(Color color)
        {
            Color = color;
        }
    
        public override string Name
        {
            get { return "SpecularColor"; }
        }

        public override string Code
        {
            get { return "Sc"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "float3"
        };

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
