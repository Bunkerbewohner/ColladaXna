using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omi.Xna.Collada.Model.Materials
{
    /// <summary>
    /// Material property describing the lighting model to be used,
    /// such as Blinn or Phong.
    /// </summary>
    public class LightingModel : MaterialProperty
    {
        public enum Model
        {            
            Phong,
            Blinn
        }

        public LightingModel(Model model)
        {
            Value = model;
        }

        public LightingModel()
        {
            Value = Model.Phong;
        }

        public Model Value { get; set; }

        public override string Name
        {
            get { return "LightingModel"; }
        }

        public override string Code
        {
            get { return Value.ToString(); }
        }

        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static ShaderInstructions _shaderInstructions = new ShaderInstructions()
        {
            ParameterType = null
        };

        public override Object GetValue()
        {
            return Value;
        }
    }
}
