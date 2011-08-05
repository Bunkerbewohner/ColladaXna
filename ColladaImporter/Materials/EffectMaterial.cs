using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// Run-time equivalent of a material which was 
    /// converted to an effect instance.
    /// </summary>
    public class EffectMaterial
    {
        Effect _effect;

        String _name;

        Material _material;

        List<String> _effectParameters;

        private EffectParameter _world;
        private EffectParameter _worldIT;
        private EffectParameter _view;
        private EffectParameter _projection;
        private EffectParameter _cameraPos;

        protected bool hasNormalMap;

        public Matrix World
        {
            get { return _world.GetValueMatrix(); }
            set
            {
                _world.SetValue(value);
                _worldIT.SetValue(Matrix.Invert(Matrix.Transpose(value)));
            }
        }

        public Matrix View
        {
            get { return _view.GetValueMatrix(); }
            set { _view.SetValue(value); }
        }

        public Matrix Projection
        {
            get { return _projection.GetValueMatrix(); }
            set { _projection.SetValue(value); }
        }

        public Vector3 CameraPosition
        {
            get { return _cameraPos.GetValueVector3(); }
            set { _cameraPos.SetValue(value); }
        }

        public String Name { get { return _name; } }

        public Effect Effect { get { return _effect; } }

        public Material Material { get { return _material; } }

        public List<String> Parameters { get { return _effectParameters; } }        

        public EffectMaterial(String name, Effect effect, List<String> effectParameters, 
            Material materialDefinition)
        {
            _name = name;
            _effect = effect;
            _effectParameters = effectParameters;
            _material = materialDefinition;

            _world = effect.Parameters["World"];
            _worldIT = effect.Parameters["WorldIT"];
            _view = effect.Parameters["View"];
            _projection = effect.Parameters["Projection"];
            _cameraPos = effect.Parameters["EyePosition"];

            hasNormalMap = materialDefinition.Properties.OfType<NormalMap>().Any();                        
        }
    }
}
