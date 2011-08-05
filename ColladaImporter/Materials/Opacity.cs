using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// The opacity property describes how much light 
    /// can pass through a material. Valid values lie between
    /// 0 (fully transparent) and 1 (fully opaque).
    /// </summary>
    public class Opacity : ValueProperty, IShaderDefaultValue
    {
        public Opacity()
        {
            Value = 1;
        }

        public Opacity(float value)
        {
            Value = value;
        }

        public override string Name
        {
            get { return "Opacity"; }
        }

        public override string Code
        {
            get { return "Op"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "float"
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
                return String.Format(CultureInfo.InvariantCulture,
                    "{0}", Value);
            }
        }

        #endregion
    }
}
