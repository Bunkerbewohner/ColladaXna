using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seafarer.Xna.Collada.Importer.Lighting;
using Seafarer.Xna.Collada.Importer.Processing;
using Seafarer.Xna.Collada.Importer.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Seafarer.Xna.Collada.Content.Processing;
using Seafarer.Xna.Collada.Importer.Geometry;
using Seafarer.Xna.Collada.Importer.Materials;
using Seafarer.Xna.Collada.Importer;
using Microsoft.Xna.Framework.Graphics;
using TInput = Seafarer.Xna.Collada.Importer.IntermediateModel;
using TOutput = Seafarer.Xna.Collada.Importer.IntermediateModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace COLLADA.ContentPipeline
{
    /// <summary>
    /// Verarbeitet die importierten Daten und erstellt daraus ein COLLADA-Model.
    /// </summary>
    [ContentProcessor(DisplayName = "COLLADA Processor")]
    public class ColladaProcessor : ContentProcessor<TInput, TOutput>
    {
        private readonly List<IColladaProcessor> processors = new List<IColladaProcessor>();

        private MaterialCompiler materialCompiler;

        /// <summary>
        /// Default lighting adds a default directional light to the model / scene
        /// if no lights are defined.
        /// </summary>
        [DefaultValue(true)]
        [Description("If DefaultLighting is set to true and no lights are defined within " + 
            " the DAE-file the content processor will create 3 default directional lights")]
        public bool DefaultLighting { get; set; }

        /// <summary>
        /// Default normal map type
        /// </summary>
        [DefaultValue(NormalMapType.DotThreeBumpMapping)]
        [Description("Determines the type of normal map that is going to be used if one " +
            " is defined in the COLLADA file")]
        public NormalMapType DefaultNormalMapType { get; set; }

        [DefaultValue(1.0f)]
        public float Scale { get; set; }

        [DefaultValue(false)]
        public bool DisableNormalMap { get; set; }

        /// <summary>
        /// X Scale amount for parallax and relief mapping. 
        /// depth = X * h + Y, where h is the normal map encoded height
        /// </summary>     
        [Description("Coefficient of scale for parallax and relief mapping")]           
        [DefaultValue(0.03f)]     
        public float ParallaxScaleX
        {
            get { return _parallaxScale.X; }
            set { _parallaxScale.X = value; }
        }

        /// <summary>
        /// Y Scale amount for parallax and relief mapping.
        /// depth = X * h + Y, where h is the normal map encoded height
        /// </summary>
        [Description("Additive constant of scale for parallax and relief mapping")]
        [DefaultValue(-0.025f)]
        public float ParallaxScaleY
        {
            get { return _parallaxScale.Y; }
            set { _parallaxScale.Y = value; }
        }

        private Vector2 _parallaxScale = new Vector2(0.03f, -0.025f);        

        public ColladaProcessor()
        {            
            // Default settings
            DefaultLighting = true;
            DefaultNormalMapType = NormalMapType.DotThreeBumpMapping;
            Scale = 1.0f;
            DisableNormalMap = false;

            // Add processors (order is relevant)            
            processors.Add(new MeshProcessor());            
            processors.Add(new LightProcessor());                           
        }

        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            ProcessingOptions options = new ProcessingOptions();

            // Save source filename to reference relative paths correctly
            // This is a custom parameter that can be used by processors
            if (context != null)
            {
                options.SourceFilename = input.SourceFilename;
                options.DefaultLighting = DefaultLighting;
                options.DefaultNormalMapType = DefaultNormalMapType;
                options.ParallaxScale = _parallaxScale;
                options.ModelScale = Scale;
                options.DisableNormalMap = DisableNormalMap;
            }

            if (materialCompiler == null)
            {
                materialCompiler = new MaterialCompiler(context);
                processors.Add(materialCompiler);
            }
            
            ContentAssert.AreEqual(materialCompiler.Context, context,
                "Context must not change");

            return processors.Aggregate(input, (current, processor) => 
                processor.Process(current, options));
        }
    }
}
