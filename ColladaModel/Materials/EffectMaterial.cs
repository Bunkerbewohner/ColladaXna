using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Omi.Xna.Collada.Model.Geometry;

namespace Omi.Xna.Collada.Model.Materials
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

        Dictionary<String, Object> _parameterValues;
        Dictionary<String, EffectParameter> _parametersBySemantic;

        private Matrix _world;
        private Matrix _worldIT;
        private Matrix _view;
        private Matrix _projection;
        private Vector3 _cameraPos;        

        protected bool hasNormalMap;

        public Matrix World
        {
            get { return _world; }
            set
            {
                _world = value;
                _worldIT = Matrix.Invert(Matrix.Transpose(value));

                UpdateEffectParameter("WORLD");
                UpdateEffectParameter("WORLDINVERSE");
                UpdateEffectParameter("WORLDTRANSPOSE");
                UpdateEffectParameter("WORLDINVERSETRANSPOSE");
                UpdateEffectParameter("WORLDVIEWPROJECTION");
            }
        }

        public Matrix View
        {
            get { return _view; }
            set 
            { 
                _view = value;
                UpdateEffectParameter("VIEW");
                UpdateEffectParameter("VIEWINVERSE");
                UpdateEffectParameter("WORLDVIEWPROJECTION");
            }
        }

        public Matrix Projection
        {
            get { return _projection; }
            set 
            { 
                _projection = value;
                UpdateEffectParameter("PROJECTION");
                UpdateEffectParameter("WORLDVIEWPROJECTION");
            }
        }

        public Vector3 CameraPosition
        {
            get { return _cameraPos; }
            set 
            { 
                _cameraPos = value;
                UpdateEffectParameter("CAMERAPOSITION");
            }
        }

        public String Name { get { return _name; } }

        public Effect Effect { get { return _effect; } }

        public Material Material { get { return _material; } }

        public Dictionary<String,Object> Parameters { get { return _parameterValues; } }        

        public EffectMaterial(String name, Effect effect, Dictionary<String,Object> effectParameters, 
            Material materialDefinition)
        {
            _name = name;
            _effect = effect;
            _parameterValues = effectParameters;
            _material = materialDefinition;            

            hasNormalMap = materialDefinition.Properties.OfType<NormalMap>().Any();

            string[] commonSemantics = new string[] {
                "WORLD", "VIEW", "PROJECTION", "WORLDVIEWPROJECTION", "WORLDINVERSETRANSPOSE", "VIEWINVERSE",
                "CAMERAPOSITION"
            };

            _parametersBySemantic = new Dictionary<string, EffectParameter>();

            foreach (var param in _effect.Parameters.Where(p => 
                commonSemantics.Contains(p.Semantic.ToUpper())))
            {
                _parametersBySemantic.Add(param.Semantic.ToUpper(), param);
            }
        }

        void UpdateEffectParameter(string semantic)
        {
            EffectParameter parameter;            
            if (!_parametersBySemantic.TryGetValue(semantic, out parameter)) return;

            switch (semantic)
            {
                case "WORLD":
                    parameter.SetValue(_world);
                    break;

                case "WORLDINVERSE":
                    parameter.SetValue(Matrix.Invert(_world));
                    break;

                case "WORLDTRANSPOSE":
                    parameter.SetValue(Matrix.Transpose(_world));
                    break;

                case "VIEW":
                    parameter.SetValue(_view);
                    break;

                case "PROJECTION":
                    parameter.SetValue(_projection);
                    break;

                case "CAMERAPOSITION":
                    parameter.SetValue(_cameraPos);
                    break;

                case "WORLDINVERSETRANSPOSE":
                    parameter.SetValue(_worldIT);
                    break;

                case "VIEWINVERSE":
                    parameter.SetValue(Matrix.Invert(_view));
                    break;

                case "WORLDVIEWPROJECTION":
                    parameter.SetValue(_world * _view * _projection);
                    break;

                default:
                    throw new Exception("Unknown semantic '" + semantic + "'");
            }
        }

        /// <summary>
        /// Creates a default material fitting the given model mesh part
        /// </summary>
        /// <param name="meshPart"></param>
        /// <param name="graphicsDevice"></param>
        /// <returns></returns>
        public static EffectMaterial CreateDefaultMaterial(MeshPart meshPart, GraphicsDevice graphicsDevice)
        {
            BasicEffect effect = new BasicEffect(graphicsDevice);
            Dictionary<String,Object> effectParameters = new Dictionary<string,object>();
            Material material = new Material();

            if (meshPart.Vertices.HasElement(VertexElementUsage.Color))
                effect.VertexColorEnabled = true;

            if (meshPart.Vertices.HasElement(VertexElementUsage.Normal))
                effect.EnableDefaultLighting();            

            return new EffectMaterial("Default Material", effect, effectParameters, material);
        }
    }
}
