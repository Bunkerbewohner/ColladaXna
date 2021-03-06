﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ColladaXna.Base.Materials
{
    public abstract class MaterialProperty
    {
        /// <summary>
        /// Name of this material property as used in the shader
        /// program. This will be used to pass the property to
        /// the shader. 
        /// </summary>
        /// <example>xDiffuseTexture, DiffuseColor, ...</example>
        public abstract String Name { get; }

        /// <summary>
        /// An code figure to distinguish this material property
        /// from all other material properties. This should
        /// consist of at most two characters.
        /// </summary>
        public abstract String Code { get; }

        /// <summary>
        /// Instructions for the effect generator
        /// </summary>
        public abstract ShaderInstructions ShaderInstructions { get; }

        public abstract Object GetValue();
    }
}
