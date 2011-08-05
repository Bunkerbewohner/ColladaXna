using System.Linq;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Omi.Xna.Collada.Importer.Processing
{
    using DirectionalLight = Omi.Xna.Collada.Model.Lighting.DirectionalLight;

    public class LightProcessor : IColladaProcessor
    {
        #region IColladaProcessor Member

        public IntermediateModel Process(IntermediateModel model, ProcessingOptions options)
        {            
            if (options.DefaultLighting)
            {
                if (!model.Lights.OfType<AmbientLight>().Any())
                {
                    AmbientLight ambient = new AmbientLight
                                               {
                                                   Name = "AmbientLight",
                                                   Color = new Color(0.2f, 0.2f, 0.2f)
                                               };

                    model.Lights.Add(ambient);
                }                

                if (!model.Lights.OfType<DirectionalLight>().Any())
                {
                    DirectionalLight keyLight = new DirectionalLight
                    {
                        Name = "DirLight1",
                        Color = new Color(1f, 1f, 0.90f),
                        Direction = new Vector3(0, -0.6f, 1)
                    };

                    DirectionalLight fillLight = new DirectionalLight
                    {
                        Name = "DirLight2",
                        Color = new Color(0.3f, 0.3f, 0.4f),
                        Direction = new Vector3(-1, 1.0f, 0)
                    };

                    DirectionalLight backLight = new DirectionalLight
                    {
                        Name = "DirLight3",
                        Color = new Color(0.1f, 0.1f, 0.1f),
                        Direction = new Vector3(0, 0, -1)
                    };

                    keyLight.Direction.Normalize();
                    fillLight.Direction.Normalize();
                    backLight.Direction.Normalize();                    

                    model.Lights.Add(keyLight);
                    model.Lights.Add(fillLight);
                    model.Lights.Add(backLight);
                }
            }

            return model;
        }

        #endregion
    }
}
