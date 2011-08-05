using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Model.Materials
{
    /// <summary>
    /// A material containing a reference to an effect (.fx) file
    /// </summary>
    public class ReferencedMaterial : Material
    {
        public string EffectFilename { get; set; }

        public ReferencedMaterial(Material material, string effectFilename)
            : base(material.Name, material.Properties)
        {
            EffectFilename = effectFilename;
        }

        public ReferencedMaterial(Material material)
            : this(material, String.Empty)
        {
            
        }
    }
}
