using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Seafarer.Xna.Collada.Importer.Materials
{
    /// <summary>
    /// Type of normal mapping
    /// </summary>
    public enum NormalMapType
    {
        /// <summary>
        /// Default normal mapping algorithm: Dot3 Bump Mapping
        /// </summary>
        DotThreeBumpMapping,

        /// <summary>
        /// Simple Parallax Mapping (also called Offset Mapping or Visual Displacement Mapping)
        /// </summary>
        ParallaxMapping,

        /// <summary>
        /// Relief Mapping (also called Steep Parallax Mapping or Parallax Occlusion Mapping)
        /// </summary>
        ReliefMapping
    }

    /// <summary>
    /// A normal map according to the dot3 bump mapping technique
    /// </summary>
    public class NormalMap : TextureProperty
    {                
        ///<summary>
        /// Type of normal mapping to use (see NormalMapType).
        /// By default Dot3 Bump Mapping is used.
        ///</summary>
        public NormalMapType Type { get; set; }

        /// <summary>
        /// Scale amount for parallax and relief maps
        /// Not used by Dot3 Bump Mapping shader
        /// </summary>
        public Vector2 ParallaxScale { get; set; }

        /// <summary>
        /// Name of this material property
        /// </summary>
        public override string Name
        {
            get { return "NormalMap"; }
        }

        /// <summary>
        /// An code figure to distinguish this material property
        /// from all other material properties. This should
        /// consist of at most two characters.
        /// </summary>
        public override string Code
        {                        
            get
            {
                switch (Type)
                {
                    case NormalMapType.DotThreeBumpMapping:
                        return "Nd";
                    case NormalMapType.ParallaxMapping:
                        return "Np";
                    case NormalMapType.ReliefMapping:
                        return "Nr";

                    default:
                        return "Nm";
                }
            }
        }

        /// <summary>
        /// Instructions for the effect generator
        /// </summary>
        public override ShaderInstructions ShaderInstructions
        {
            get { return _shaderInstructions; }
        }

        static readonly ShaderInstructions _shaderInstructions = new ShaderInstructions
        {
            ParameterType = "Texture"
        };

        /// <summary>
        /// Creates an empty normal map with no texture and the default
        /// normal mapping type (Dot3 Bump Mapping).
        /// </summary>
        public NormalMap()
            : this(null, NormalMapType.DotThreeBumpMapping)
        {
            
        }

        /// <summary>
        /// Creates a default normal map with given texture
        /// and Dot3 Bump Mapping.
        /// </summary>
        /// <param name="texture"></param>
        public NormalMap(TextureReference texture)
            : this(texture, NormalMapType.ParallaxMapping)
        {
            
        }

        /// <summary>
        /// Creates a normal map with given texture and type
        /// </summary>
        /// <param name="texture">Texture reference</param>
        /// <param name="type">Type of normal mapping</param>        
        public NormalMap(TextureReference texture, NormalMapType type)
        {
            Texture = texture;
            Type = type;
            ParallaxScale = new Vector2(0.03f, -0.025f);
        }
    }
}
