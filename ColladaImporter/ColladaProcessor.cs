using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Omi.Xna.Collada.Model.Lighting;
using Omi.Xna.Collada.Importer.Processing;
using Omi.Xna.Collada.Importer.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Omi.Xna.Collada.Importer.Processing;
using Omi.Xna.Collada.Model.Geometry;
using Omi.Xna.Collada.Model.Materials;
using Omi.Xna.Collada.Importer;
using Microsoft.Xna.Framework.Graphics;
using TInput = Omi.Xna.Collada.Model.IntermediateModel;
using TOutput = Omi.Xna.Collada.Model.IntermediateModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Omi.Xna.Collada.Importer
{
    /// <summary>
    /// Verarbeitet die importierten Daten und erstellt daraus ein COLLADA-Model.
    /// </summary>
    [ContentProcessor(DisplayName = "COLLADA Custom Model Processor")]
    public class ColladaProcessor : ContentProcessor<TInput, TOutput>
    {
        private readonly List<IColladaProcessor> processors = new List<IColladaProcessor>();

        private ProcessingOptions options = null;

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

        [Description("Rotation angle in degress around X-axis to apply to the root joint")]        
        public float RotationX { get; set; }

        [Description("Rotation angle in degress around Y-axis to apply to the root joint")]
        public float RotationY { get; set; }

        [Description("Rotation angle in degress around Z-axis to apply to the root joint")]
        public float RotationZ { get; set; }

        public ColladaProcessor()
        {
            // Default settings
            DefaultLighting = true;
            DefaultNormalMapType = NormalMapType.DotThreeBumpMapping;
            Scale = 1.0f;
            DisableNormalMap = false;
            RotationX = 0;
            RotationY = 0;
            RotationZ = 0;            

            // Add processors (order is relevant)            
            processors.Add(new MeshProcessor());            
            processors.Add(new LightProcessor());                           
        }

        public ColladaProcessor(ProcessingOptions options)
            : this()
        {
            this.options = options;
        }

        public override TOutput Process(TInput input, ContentProcessorContext context)
        {
            if (this.options == null)
            {
                options = new ProcessingOptions();                

                options.DefaultLighting = DefaultLighting;
                options.DefaultNormalMapType = DefaultNormalMapType;
                options.ParallaxScale = _parallaxScale;
                options.ModelScale = Scale;
                options.DisableNormalMap = DisableNormalMap;
                options.RootJoinRotation = new Vector3(MathHelper.ToRadians(RotationX),
                        MathHelper.ToRadians(RotationY), MathHelper.ToRadians(RotationZ));  
            }

            // Save source filename to reference relative paths correctly
            // This is a custom parameter that can be used by processors
            if (context != null)
            {
                options.SourceFilename = input.SourceFilename;
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
