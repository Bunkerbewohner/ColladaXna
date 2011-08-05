using System;
using Microsoft.Xna.Framework;
using Omi.Xna.Collada.Model.Materials;

namespace Omi.Xna.Collada.Importer.Processing
{
    public class ProcessingOptions
    {
        /// <summary>
        /// Name of the source file
        /// </summary>
        public string SourceFilename { get; set; }

        /// <summary>
        /// Folder where compiled shaders and textures should be
        /// stored. Default value is null for using the content
        /// root directory of the ContentManager that was used
        /// to create the RuntimeLoader.
        /// </summary>
        public string ContentFolder { get; set; }

        public bool DefaultLighting { get; set; }

        public NormalMapType DefaultNormalMapType { get; set; }

        public bool DisableNormalMap { get; set; }

        public Vector2 ParallaxScale { get; set; }

        public float ModelScale { get; set; }

        /// <summary>
        /// Rotation of the root joint in radians
        /// </summary>
        public Vector3 RootJoinRotation { get; set; }

        public ProcessingOptions()
        {
            SourceFilename = String.Empty;
            DefaultLighting = true;
            DefaultNormalMapType = NormalMapType.DotThreeBumpMapping;
            DisableNormalMap = false;
            ParallaxScale = new Vector2(0.03f, -0.025f);
            RootJoinRotation = new Vector3(0, 0, 0);
            ModelScale = 1;
            ContentFolder = null;
        }
    }
}
