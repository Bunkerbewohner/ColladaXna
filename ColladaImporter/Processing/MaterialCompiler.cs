using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omi.Xna.Collada.Importer.Data;
using Omi.Xna.Collada.Importer;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Materials;
using Omi.Xna.Collada.Importer.Processing;
using Omi.Xna.Collada.Importer.Processing.Effects;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Omi.Xna.Collada.Importer.Processing
{
    /// <summary>
    /// The material processor processes all materials by building all referenced textures
    /// and creating a ProcessedMaterial from each Material containing a reference to the
    /// built assets.
    /// </summary>
    public class MaterialCompiler : Omi.Xna.Collada.Importer.Processing.MaterialProcessor
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

            foreach (var shader in material.Properties.OfType<CustomShader>())
            {
                var entries = new List<Tuple<string, object>>();

                foreach (var param in (from p in shader.Parameters where p.Value is TextureReference select p))
                {
                    var texture = (TextureReference)param.Value;
                    var fileReference = new ExternalReference<TextureContent>(texture.Filename, origin);
                    var builtReference = _context.BuildAsset<TextureContent, TextureContent>(fileReference,
                        "TextureProcessor");

                    var tex = new TextureContentReference(texture, builtReference);    

                    entries.Add(new Tuple<string, object>(param.Key, tex));
                }

                foreach (var e in entries)
                {
                    shader.Parameters[e.Item1] = e.Item2;
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
