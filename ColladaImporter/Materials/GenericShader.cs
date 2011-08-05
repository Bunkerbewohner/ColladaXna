﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    public class GenericShader : ShaderProperty
    {
        /// <summary>
        /// A dictionary of shader parameters
        /// </summary>
        Dictionary<String, Object> _parameters = new Dictionary<String, Object>();

        /// <summary>
        /// A dictionary of shader parameters
        /// </summary>
        public Dictionary<String, Object> Parameters
        {
            get { return _parameters; }
        }

        public GenericShader()
        {
            Filename = String.Empty;
        }

        public override string Name
        {
            get { return "GenericShader"; }
        }

        public override string Code
        {
            get { return "Fx"; }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = null
        };
    }
}