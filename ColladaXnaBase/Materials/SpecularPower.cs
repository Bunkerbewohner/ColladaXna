﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Materials
{
    public class SpecularPower : ValueProperty, IShaderDefaultValue
    {
        public override string Name
        {
            get { return "SpecularPower"; }
        }

        public override string Code
        {
            get { return "Sp"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "float"
        };

        public SpecularPower()
        {
            Value = 1;
        }

        public SpecularPower(float shininess)
        {
            Value = shininess;
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
                return String.Format(CultureInfo.InvariantCulture,
                    "{0}", Value);
            }
        }

        #endregion
    }
}
