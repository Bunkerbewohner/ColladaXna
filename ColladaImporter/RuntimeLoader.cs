using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Importer.Import;
using Omi.Xna.Collada.Model.Animation;
using Omi.Xna.Collada.Model.Lighting;
using Omi.Xna.Collada.Importer.Processing;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Omi.Xna.Collada.Model.Materials;
using Omi.Xna.Collada.Model.Geometry;
using Microsoft.Xna.Framework;
using Omi.Xna.Collada.Importer.Util;

namespace Omi.Xna.Collada.Importer
{
    using EffectMaterial = Omi.Xna.Collada.Model.Materials.EffectMaterial;
    using Omi.Xna.Collada.Importer.Data;

    /// <summary>
    /// Implementation of a runtime loader for COLLADA models.
    /// This should only be used when neccessary, since loading
    /// models from .dae files at runtime is relatively slow.
    /// Also generic shaders which don't exist can only be compiled
    /// at runtime on Windows with a full XNA GS installation.
    /// </summary>
    public class RuntimeLoader
    {
        private GraphicsDevice _graphicsDevice;
        private ContentManager _contentManager;

        private List<IColladaImporter> importers = new List<IColladaImporter>(5);
        private List<IColladaProcessor> processors = new List<IColladaProcessor>(5);

        public RuntimeLoader(GraphicsDevice graphisDevice, ContentManager contentManager=null)
        {
            _graphicsDevice = graphisDevice;
            _contentManager = contentManager;

            // Importers
            importers.Add(new SkeletonImporter());
            importers.Add(new GeometryImporter());

            importers.Add(new MaterialImporter());
            importers.Add(new SceneImporter());
            importers.Add(new LightImporter());

            importers.Add(new AnimationImporter());

            // Processors
            processors.Add(new MeshProcessor());
            processors.Add(new LightProcessor());
            processors.Add(new MaterialLoader(_graphicsDevice, contentManager));
        }

        public ModelData Load(string filename)
        {
            ProcessingOptions options = new ProcessingOptions();
            options.SourceFilename = filename;
            options.ContentFolder = _contentManager != null ? 
                _contentManager.RootDirectory : ".";

            return Load(filename, options);
        }

        /// <summary>
        /// Loads a model during runtime
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="options"></param>
        /// <returns></returns>        
        public ModelData Load(string filename, ProcessingOptions options)
        {
            if (options.ContentFolder == null)
                options.ContentFolder = _contentManager != null ? 
                    _contentManager.RootDirectory : ".";

            if (String.IsNullOrEmpty(options.SourceFilename))
                options.SourceFilename = filename;

            //
            // 1. Read XML file
            //

            // Namespaces are handled wrongly by XPath 1.0 and also we don't need
            // them anyway, so all namespaces are simply removed
            string xmlWithoutNamespaces = Regex.Replace(File.ReadAllText(filename),
                @"xmlns="".+?""", "");

            // This is hack for replacing contents form the scene library into the visual
            // scene to avoid following node references
            // TODO: Implement a more elegant COLLADA XML reader
            XmlUtil.SubstituteNodeInstances(ref xmlWithoutNamespaces);

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlWithoutNamespaces);
            XmlNode xmlRoot = xml.DocumentElement;

            if (xmlRoot == null)
            {
                throw new ApplicationException("XML Root not found");
            } 
 
            //
            // 2. Import model data
            //

            IntermediateModel importedData = new IntermediateModel();
            importedData.SourceFilename = filename;

            foreach (IColladaImporter importer in importers)
            {
                importer.Import(xmlRoot, ref importedData);
            }

            //
            // 3. Process model data
            //

            IntermediateModel processedData = processors.Aggregate(importedData, (current, processor) =>
                processor.Process(current, options));

            //
            // 4. Create runtime Model Data
            //            

            // 1. Materials (converted to effects)
            var materials = LoadEffectMaterials(processedData.Materials);

            // 2. Meshes
            var inMeshes = processedData.Meshes;

            // 3. MeshInstances
            var inMeshInstances = processedData.MeshInstances;

            // 4. Lights
            var inLights = processedData.Lights;

            // 5. Joints
            var inJoints = processedData.Joints;

            // 6. Joint Animations
            var inJointAnimations = processedData.JointAnimations;            

            //=================================================================
            // create runtime model                        

            string modelName = Path.GetFileNameWithoutExtension(processedData.SourceFilename);            

            return ModelData.CreateFromIntermediateData(modelName, materials, inMeshes, inMeshInstances,
                inLights, inJoints, inJointAnimations, _graphicsDevice);
        }        

        /// <summary>
        /// Loads effects referenced by given materials
        /// </summary>
        /// <param name="materials">Material references</param>
        /// <returns>Effect materials</returns>
        protected List<EffectMaterial> LoadEffectMaterials(List<Material> materials)
        {
            List<EffectMaterial> effectMaterials = new List<EffectMaterial>(materials.Count);

            foreach (Material material in materials)
            {
                Effect effect = null;

                if (material is ReferencedMaterial)
                {
                    ReferencedMaterial referencedMaterial = material as ReferencedMaterial;                    

                    // Load compiled effect
                    effect = _contentManager.Load<Effect>(referencedMaterial.EffectFilename);

                    if (effect == null)
                    {
                        throw new ApplicationException("Could not load referenced effect '" + 
                            referencedMaterial.EffectFilename + "'");
                    }
                }
                else if (material is LoadedMaterial)
                {
                    LoadedMaterial compiledMaterial = material as LoadedMaterial;
                    effect = new Effect(_graphicsDevice, compiledMaterial.CompiledEffect.GetEffectCode());                    
                }
                else
                {
                    throw new Exception("Unexpected material");
                }

                // Create copy of effect and set individual parameters
                effect = effect.Clone();
                var parameters = new Dictionary<string, object>();

                if (material.Properties.OfType<CustomShader>().Any())
                {
                    CustomShader customShader = material.Properties.OfType<CustomShader>().First();

                    foreach (var param in customShader.Parameters)
                    {
                        parameters.Add(param.Key, param.Value);

                        var p = effect.Parameters[param.Key];
                        if (p == null) continue;

                        if (param.Value is float)
                            p.SetValue((float)param.Value);
                        else if (param.Value is Vector3)
                        {
                            p.SetValue((Vector3)param.Value);
                        }
                        else if (param.Value is Vector4)
                        {
                            if (p.ColumnCount == 3)
                            {
                                Vector4 val = (Vector4)param.Value;
                                p.SetValue(val.XYZ());
                            }
                            else
                            {
                                p.SetValue((Vector4)param.Value);
                            }
                        }
                        else if (param.Value is Color)
                            p.SetValue(((Color)param.Value).ToVector4());
                        else if (param.Value is Matrix)
                            p.SetValue((Matrix)param.Value);
                        else if (param.Value is LoadedTextureReference)
                            p.SetValue(((LoadedTextureReference)param.Value).Texture);
                    }
                }
                else
                {

                    // set textures                
                    foreach (var property in material.Properties.OfType<TextureProperty>())
                    {
                        string paramName = property.Name;
                        var reference = property.Texture as LoadedTextureReference;
                        if (reference == null) throw new ApplicationException("no texture reference");

                        effect.Parameters[paramName].SetValue(reference.Texture);

                        if (property is NormalMap)
                        {
                            NormalMap normalMap = property as NormalMap;
                            if (normalMap.Type == NormalMapType.ParallaxMapping ||
                                normalMap.Type == NormalMapType.ReliefMapping)
                            {
                                effect.Parameters["ReliefScale"].SetValue(normalMap.ParallaxScale);
                            }
                        }
                    }

                    // set default values for color properties (AmbientColor, SpecularColor etc.)
                    foreach (var colorProperty in material.Properties.OfType<ColorProperty>())
                    {
                        if (colorProperty.ShaderInstructions.ParameterType == "float3")
                        {
                            effect.Parameters[colorProperty.Name].SetValue(colorProperty.Color.ToVector3());
                        }
                        else
                        {
                            effect.Parameters[colorProperty.Name].SetValue(colorProperty.Color.ToVector4());
                        }
                    }

                    // set default values for single parameters (Specularity etc.)
                    foreach (var valueProperty in material.Properties.OfType<ValueProperty>())
                    {
                        effect.Parameters[valueProperty.Name].SetValue(valueProperty.Value);
                    }

                    foreach (var property in material.Properties)
                    {
                        parameters.Add(property.Name, property.GetValue());
                    }
                }
                               
                effectMaterials.Add(new EffectMaterial(material.Name, effect, 
                    parameters, material));
            }

            return effectMaterials;
        }
    }
}
