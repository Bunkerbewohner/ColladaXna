using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Model.Materials
{
    public abstract class ValueProperty : MaterialProperty
    {
        public float Value { get; set; }

        public override Object GetValue()
        {
            return Value;
        }        
    }
}
