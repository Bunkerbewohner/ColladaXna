using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// A material that has been loaded by compiling the corresponding
    /// effect. 
    /// </summary>
    public class LoadedMaterial : Material
    {
        /// <summary>
        /// The effect that corresponds to this material
        /// </summary>
        public CompiledEffectContent CompiledEffect { get; private set;}

        /// <summary>
        /// Creates a new loaded material
        /// </summary>
        /// <param name="material"></param>
        /// <param name="compiledEffect"></param>
        public LoadedMaterial(Material material, CompiledEffectContent compiledEffect)
            : base(material.Name, material.Properties)
        {
            CompiledEffect = compiledEffect;
        }
    }
}
