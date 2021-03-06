﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Materials
{
    /// <summary>
    /// Defines an interface for properties who have a default value
    /// </summary>
    public interface IShaderDefaultValue
    {
        /// <summary>
        /// Returns the default value of the property in HLSL,
        /// e.g. "float4(1,1,1,0)" 
        /// </summary>
        String ShaderDefaultValue { get; }
    }
}
