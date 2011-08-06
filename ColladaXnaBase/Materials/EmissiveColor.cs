using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base.Materials
{
    /// <summary>
    /// The emissive color describes which light the material emits
    /// </summary>
    public class EmissiveColor : ColorProperty, IShaderDefaultValue
    {
        public override string Name
        {
            get { return "EmissiveColor"; }
        }

        public override string Code
        {
            get { return "Ec"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "float3"
        };

        public EmissiveColor()
        {
            Color = new Color(0, 1.0f, 0);
        }

        public EmissiveColor(Color color)
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
