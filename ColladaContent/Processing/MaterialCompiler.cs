using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seafarer.Xna.Collada.Content.Data;
using Seafarer.Xna.Collada.Importer;
using Seafarer.Xna.Collada.Importer.Materials;
using Seafarer.Xna.Collada.Importer.Processing;
using Seafarer.Xna.Collada.Importer.Processing.Effects;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Seafarer.Xna.Collada.Content.Processing
{
    /// <summary>
    /// The material processor processes all materials by building all referenced textures
    /// and creating a ProcessedMaterial from each Material containing a reference to the
    /// built assets.
    /// </summary>
    public class MaterialCompiler : Seafarer.Xna.Collada.Importer.Processing.MaterialProcessor
    {
        private ContentProcessorContext _context;

        public ContentProcessorContext Context { get { return _context; }}

        public MaterialCompiler(ContentProcessorContext context)
        {
            _context = context;
        }

        #region Material Processing        

        protected override Material ProcessMaterial(Material material, IntermediateModel model,
            ProcessingOptions options)
        {
            ContentIdentity origin = new ContentIdentity(options.SourceFilename);            

            if (options.DisableNormalMap)
            {
                material.Properties.RemoveAll(property => property is NormalMap);
            }

            // TODO: replace hotfix for Normal Map + Specular Problem with real solution 
            if (options.ModelScale != 1 && material.Properties.OfType<NormalMap>().Any())
            {
                ValueProperty spec = material.Properties.OfType<SpecularPower>().First();
                //spec.Value /= options.ModelScale;
            }

            // Create external references for all textures so they can be saved            
            foreach (var property in material.Properties.OfType<TextureProperty>())
            {
                TextureReference texture = property.Texture;

                var fileReference = new ExternalReference<TextureContent>(texture.Filename, origin);
                var builtReference = _context.BuildAsset<TextureContent, TextureContent>(fileReference,
                    "TextureProcessor");

                property.Texture = new TextureContentReference(texture, builtReference);                

                if (property is NormalMap)
                {
                    NormalMap normalMap = property as NormalMap;
                    normalMap.Type = options.DefaultNormalMapType;
                    normalMap.ParallaxScale = options.ParallaxScale;
                }
            }

            // Generate Effect
            EffectGenerator generator = new BasicEffectGenerator();
            EffectDescription effectDesc = generator.CreateEffect(material, model);

            // Compile effect and store external reference
            var contentRef = new ExternalReference<EffectContent>(effectDesc.Filename);
            var builtRef = _context.BuildAsset<EffectContent, CompiledEffectContent>(contentRef, 
                "EffectProcessor");

            var processedMaterial = new CompiledMaterial(material);
            processedMaterial.Effect = builtRef;
            processedMaterial.EffectParameters = effectDesc.Parameters;

            return processedMaterial;
        }

        #endregion
    }
}
