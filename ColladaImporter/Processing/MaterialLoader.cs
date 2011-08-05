using System;
using System.Linq;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Materials;
using Omi.Xna.Collada.Importer.Processing.Effects;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Omi.Xna.Collada.Importer.Data;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace Omi.Xna.Collada.Importer.Processing
{
    /// <summary>
    /// Material processor for runtime application. Other than the MaterialCompiler,
    /// which can (and should) be used in the content pipeline, this processor loads
    /// textures from files directly (rather than compiling them) and loads existent
    /// shaders. If a required shader does not yet exist, it is generated, if possible.
    /// </summary>
    public class MaterialLoader : MaterialProcessor
    {
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        public MaterialLoader(GraphicsDevice graphicsDevice, ContentManager contentManager = null)
        {
            _graphicsDevice = graphicsDevice;
            _contentManager = contentManager;
        }

        LoadedTextureReference LoadTexture(TextureReference textureReference)
        {
            string filename = textureReference.Filename;
            string texchannel = textureReference.TextureChannel;
            Texture2D texture = null;

            if (!File.Exists(filename))
            {
                // see if there's a compiled xnb
                filename = Path.GetDirectoryName(filename) + "\\" +
                           Path.GetFileNameWithoutExtension(filename) + ".xnb";

                if (File.Exists(filename) && _contentManager != null)
                {
                    // Load via content manager
                    filename = filename.Replace(".xnb", "").Replace("Content\\", "");
                    texture = _contentManager.Load<Texture2D>(filename);
                }
                else
                {
                    throw new Exception("Could not load texture '" + filename + "'");
                }
            }
            else
            {
                using (FileStream fileIn = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {                    
                    // TIFF and TGA images are not supported by default
                    if (filename.ToLower().EndsWith("tif") ||
                        filename.ToLower().EndsWith(".tif") ||
                        filename.ToLower().EndsWith(".tga"))
                    {

                        try
                        {
                            TextureImporter importer = new TextureImporter();
                            TextureProcessor processor = new TextureProcessor();

                            TextureContent content = importer.Import(filename, null);
                            Texture2DContent processedContent = processor.Process(content,
                                new CustomProcessorContext()) as Texture2DContent;

                            BitmapContent bitmap = (BitmapContent)processedContent.Mipmaps[0];
                            SurfaceFormat format;

                            if (bitmap.TryGetFormat(out format))
                                texture = new Texture2D(_graphicsDevice, bitmap.Width, bitmap.Height, false, format);
                            else
                                texture = new Texture2D(_graphicsDevice, bitmap.Width, bitmap.Height);

                            texture.SetData<byte>(bitmap.GetPixelData());
                        }
                        catch (Exception)
                        {
                            // default processor couldn't load the image, try it manually
                            texture = ImageImporter.LoadImage(fileIn, _graphicsDevice);
                        }
                        finally
                        {
                            if (texture == null)
                            {
                                throw new Exception("Could not load texture '" + filename + "'");
                            }
                        }
                    }
                    else
                    {
                        texture = Texture2D.FromStream(_graphicsDevice, fileIn);
                    }                    
                }
            }

            return new LoadedTextureReference(filename, texchannel, texture);
        }

        protected override Material ProcessMaterial(Material material, 
            IntermediateModel model, ProcessingOptions options)
        {
            string baseDir = Path.GetDirectoryName(model.SourceFilename);

            // Adjust filename paths
            foreach (var texture in material.Properties.OfType<TextureProperty>().Select(p => p.Texture))
            {
                if (Path.IsPathRooted(texture.Filename))
                    continue;

                texture.Filename = baseDir + @"\" + texture.Filename;
            }

            if (options.DisableNormalMap)
            {
                material.Properties.RemoveAll(property => property is NormalMap);
            }            

            // Create external references for all textures so they can be saved            
            foreach (var property in material.Properties.OfType<TextureProperty>())
            {
                string filename = property.Texture.Filename;
                property.Texture = LoadTexture(property.Texture);                                             

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
                    LoadedTextureReference tex = LoadTexture((TextureReference)param.Value);
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

            if (!Path.IsPathRooted(effectDesc.Filename))
            {
                effectDesc.Filename = options.ContentFolder + "\\" + effectDesc.Filename;
            }            

            // If the shader does not yet exist, it has to be compiled
            if (!File.Exists(effectDesc.Filename.Replace(".fx", ".xnb")))
            {
                EffectProcessor processor = new EffectProcessor();
                processor.Defines = "WINDOWS;HIDEF";
                processor.DebugMode = EffectProcessorDebugMode.Optimize;                     

                // Compile effect
                EffectContent effectSource = new EffectContent() 
                {
                    Name = effectDesc.Name,
                    Identity = new Microsoft.Xna.Framework.Content.Pipeline.ContentIdentity(effectDesc.Filename),
                    EffectCode = effectDesc.Code
                };

                if (effectSource.EffectCode == null)
                {
                    effectSource.EffectCode = File.ReadAllText(effectDesc.Filename);
                }

                CompiledEffectContent effect = processor.Process(effectSource, new CustomProcessorContext());

                return new LoadedMaterial(material, effect);
            }
            else
            {
                return new ReferencedMaterial(material, effectDesc.Filename.Replace(".fx", ""));
            }
        }
    }

    class MyLogger : ContentBuildLogger
    {
        public override void LogMessage(string message, params object[] messageArgs) { }
        public override void LogImportantMessage(string message, params object[] messageArgs) { }
        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs) { }
    }

    class CustomProcessorContext : ContentProcessorContext
    {
        public override TargetPlatform TargetPlatform { get { return TargetPlatform.Windows; } }
        public override GraphicsProfile TargetProfile { get { return GraphicsProfile.HiDef; } }
        public override string BuildConfiguration { get { return string.Empty; } }
        public override string IntermediateDirectory { get { return string.Empty; } }
        public override string OutputDirectory { get { return string.Empty; } }
        public override string OutputFilename { get { return string.Empty; } }

        public override OpaqueDataDictionary Parameters { get { return parameters; } }
        OpaqueDataDictionary parameters = new OpaqueDataDictionary();

        public override ContentBuildLogger Logger { get { return logger; } }
        ContentBuildLogger logger = new MyLogger();

        public override void AddDependency(string filename) { }
        public override void AddOutputFile(string filename) { }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters) { throw new NotImplementedException(); }
        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName) { throw new NotImplementedException(); }
        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName) { throw new NotImplementedException(); }
    }
}
