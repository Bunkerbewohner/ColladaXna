using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using Omi.Xna.Collada.Importer.Exceptions;
using Omi.Xna.Collada.Model;
using Omi.Xna.Collada.Model.Materials;
using Omi.Xna.Collada.Importer.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;
using System.IO;

namespace Omi.Xna.Collada.Importer.Import
{
    public class MaterialImporter : IColladaImporter
    {
        private IntermediateModel model;

        #region IColladaImporter Member

        public void Import(XmlNode xmlRoot, ref IntermediateModel model)
        {
            this.model = model;

            // Find material nodes in library
            XmlNodeList xmlMaterials = xmlRoot.SelectNodes("library_materials/material");
            if (xmlMaterials == null || xmlMaterials.Count == 0)
            {
                // No Materials found                
                return;
            }                       

            model.Materials = (from XmlNode xmlMaterial in xmlMaterials 
                               select ImportMaterial(xmlMaterial)).ToList();           
        }

        #endregion

        #region Material Importing

        Material ImportMaterial(XmlNode xmlMaterialNode)
        {
            Material material = new Material();
            material.Name = XmlUtil.GetName(xmlMaterialNode);                      

            XmlNode xmlEffect = FindUsedEffect(xmlMaterialNode);
            if (xmlEffect == null)
            {
                throw new NotFoundException("No Effect found for Material '" +
                    material.Name + "'", xmlMaterialNode);
            }

            // read data definition per COLLADA common profile
            ReadCommonTechnique(xmlEffect, ref material);

            // Search normal map and add it, if it exists (that's not part of the common profile)
            SearchNormalMap(xmlEffect, ref material);

            if (!material.Properties.OfType<DiffuseColor>().Any() &&
                !material.Properties.OfType<DiffuseMap>().Any())
            {
                material.Properties.Add(new DiffuseColor(new Color(0.5f, 0.5f, 0.5f)));
            }

            ReadEffectImport(xmlMaterialNode, xmlEffect, ref material);

            return material;
        }

        /// <summary>
        /// Tries to read import statements for external .FX files
        /// </summary>
        /// <param name="xmlEffect">xml effect node</param>
        /// <param name="material">Material instance to store data in</param>
        void ReadEffectImport(XmlNode xmlMaterial, XmlNode xmlEffect, ref Material material)
        {
            // for now the simplest solution: Simply search for a file path containing ".fx"
            Match match = Regex.Match(xmlEffect.InnerXml, "\"([^\"]+\\.fx)\"");

            if (!match.Success) return;            
            string filename = match.Groups[1].Value.Replace("%20", " ");

            if (!Path.IsPathRooted(filename))
                filename = Path.GetDirectoryName(model.SourceFilename) + "\\" + filename;

            CustomShader shader = new CustomShader();
            shader.Filename = filename;
            
            // Set parameters as given in <instance_effect>
            XmlNode xmlInstanceEffect = xmlMaterial.SelectSingleNode("instance_effect");
            if (xmlInstanceEffect == null) return;

            foreach (XmlNode setparam in xmlInstanceEffect.SelectNodes("setparam"))
            {
                // Name referencing a shader parameter
                string name = setparam.GetAttributeString("ref");

                // value can be color, vector, single or texture
                Object value = null;
                XmlNode valueNode = setparam.FirstChild;
                
                switch (valueNode.Name)
                {
                    case "float":
                        value = XmlUtil.ParseFloats(valueNode.InnerText)[0];
                        break;

                    case "float3":
                        value = XmlUtil.ParseVector3(valueNode.InnerText);
                        break;

                    case "float4":
                        value = XmlUtil.ParseVector4(valueNode.InnerText);
                        break;

                    case "float4x4":
                        value = XmlUtil.ParseMatrix(valueNode.InnerText);
                        break;

                    case "surface":

                        TextureReference texture = new TextureReference();
                        texture.TextureChannel = "CHANNEL1";

                        string imageId = valueNode.SelectSingleNode("init_from").InnerText;

                        // texture/surface -> image
                        XmlNode root = xmlEffect.OwnerDocument.DocumentElement;
                        XmlNode imageInitFrom = root.SelectSingleNode("library_images/image[@id='" +
                            imageId + "']/init_from");
                        if (imageInitFrom == null)
                            throw new Exception("Image not found: " + imageId, null);

                        texture.Filename = imageInitFrom.InnerText.Trim();
                        texture.Filename = texture.Filename.Replace("file://", "");

                        if (!Path.IsPathRooted(texture.Filename))
                        {
                            texture.Filename = Path.GetDirectoryName(model.SourceFilename) + "\\" +
                                texture.Filename;                            
                        }

                        value = texture;                        

                        break;
                }

                if (value != null)
                {
                    shader.Parameters.Add(name, value);
                }
            }

            material.Properties.Add(shader);
        }

        /// <summary>
        /// Reads data from profile_COMMON and technique with sid "common".
        /// Supported are the following default techniques:
        /// - constant
        /// - blinn / phong
        /// - lambert
        /// However, all of these eventuall are converted to phong.
        /// </summary>
        /// <param name="xmlEffect">xml effect node</param>
        /// <param name="material">Material instance to store data in</param>
        /// <exception cref="FormatNotSupportedException"></exception>
        void ReadCommonTechnique(XmlNode xmlEffect, ref Material material)
        {
            XmlNode xmlTechnique = xmlEffect.SelectSingleNode("profile_COMMON/technique");
            if (xmlTechnique == null)
                throw new NotFoundException("No 'common' Technique found in Effect", xmlEffect);

            XmlNode node = xmlTechnique.SelectSingleNode("phong|blinn|constant|lambert");
            if (node == null)
                throw new NotFoundException("No supported material technique found", xmlTechnique);

            // Save lighting model            
            if (node.Name.Equals("phong"))
            {
                material.Properties.Add(new LightingModel(LightingModel.Model.Phong));
            }
            else if (node.Name.Equals("blinn"))
            {
                material.Properties.Add(new LightingModel(LightingModel.Model.Blinn));
            }
            else
            {
                // constant and lambert are simply subsets of phong and blinn, so phong
                // can be used to represent them with just fewer parameters
                material.Properties.Add(new LightingModel(LightingModel.Model.Phong));
            }            

            // Emissive Color
            Object emission = ReadColorOrTexture(xmlEffect, node.SelectSingleNode("emission"));
            AddColorOrTextureProperty<EmissiveColor, EmissiveMap>(emission, material);

            // Ambient Light
            Object ambient = ReadColorOrTexture(xmlEffect, node.SelectSingleNode("ambient"));
            AddColorOrTextureProperty<AmbientColor, AmbientMap>(ambient, material);

            // Diffuse Color
            Object diffuse = ReadColorOrTexture(xmlEffect, node.SelectSingleNode("diffuse"));
            AddColorOrTextureProperty<DiffuseColor, DiffuseMap>(diffuse, material);

            // Specular Color
            Object specular = ReadColorOrTexture(xmlEffect, node.SelectSingleNode("specular"));
            AddColorOrTextureProperty<SpecularColor, SpecularMap>(specular, material);

            // Shininess (n)            
            float? shininess = ReadFloat(node.SelectSingleNode("shininess"));
            if (shininess != null) material.Properties.Add(new SpecularPower((float)shininess));

            // Reflective / Environment Map (Reflective Color is not supported)
            Object reflective = ReadColorOrTexture(xmlEffect, node.SelectSingleNode("reflective"));
            if (reflective != null && reflective is TextureReference)
            {
                material.Properties.Add(new EnvironmentMap((TextureReference)reflective));
            }

            // Reflectivity is ignored right now
            //// float reflectivity = ReadFloat(node.SelectSingleNode("reflectivity"), 0);

            // Opacity (inverse transparency)            
            Object transparent = ReadColorOrTexture(xmlEffect, node.SelectSingleNode("transparent"));
            float? transparency = ReadFloat(node.SelectSingleNode("transparency"));

            // Either take an opacity map or simply set the overall opacity of the material
            if (transparent is TextureReference)
            {
                material.Properties.Add(new OpacityMap((TextureReference)transparent));
            }
            else if (transparency != null && transparent is Color)
            {
                XmlNode xmlTransparent = node.SelectSingleNode("transparent");
                String mode = xmlTransparent.Attributes["Opaque"] != null ? 
                    xmlTransparent.Attributes["opaque"].Value : "A_ONE";

                Vector4 color = ((Color)transparent).ToVector4();
                float alpha = (float)transparency;
                float opacity = 1;

                switch (mode)
                {
                    case "A_ONE":
                        opacity = color.W * alpha;
                        break;

                    case "RGB_ZERO":
                        // only simple opacity (alpha channel) is supported
                        // take average of R,G and B
                        opacity = 1 - ((color.X + color.Y + color.Z) / 3) * alpha;
                        break;

                    case "A_ZERO":
                        opacity = 1 - color.W * alpha;
                        break;

                    case "RGB_ONE":
                        // only simple opacity (alpha channel) is supported
                        // take average of R,G and B
                        opacity = ((color.X + color.Y + color.Z) / 3) * alpha;
                        break;

                    default:
                        throw new FormatNotSupportedException("Unsupported transparency" +
                            " format", xmlTransparent);
                }

                // Check for cases where transparency was given with wrong default mode
                if (opacity == 0 && xmlTransparent.Attributes["Opaque"] == null)
                {                    
                    opacity = 1;
                }

                material.Properties.Add(new Opacity(opacity));
            }

            // Refraction is ignored right now
            //// int refractionIndex = (int)ReadFloat(node.SelectSingleNode("refractionIndex"), 0);
        }

        /// <summary>
        /// Looks for a normal map definition and, if one was found, inserts
        /// it into the data.
        /// </summary>
        /// <param name="xmlEffect">Effect node</param>
        /// <param name="data">data data</param>
        /// <param name="material"></param>
        void SearchNormalMap(XmlNode xmlEffect, ref Material material)
        {
            // TODO: Make compatible with most common normal map definitions

            // ColladaMAX defines a normal map as "bump" and texture (profile: OpenCOLLADA3dsMax)
            XmlNode xmlBumpTexture = xmlEffect.SelectSingleNode(".//extra//bump");
            if (xmlBumpTexture != null)
            {
                Object texture = ReadColorOrTexture(xmlEffect, xmlBumpTexture);
                if (texture != null && texture is TextureReference)
                {
                    material.Properties.Add(new NormalMap((TextureReference)texture));
                }
            }
        }

        #endregion

        #region Helper Methods and XML Parsing

        TextureProperty CreateTextureProperty<T>(TextureReference tex) where T : TextureProperty, new()
        {
            TextureProperty property = new T();
            property.Texture = tex;
            return property;
        }

        ColorProperty CreateColorProperty<T>(Color color) where T : ColorProperty, new()
        {
            ColorProperty property = new T();
            property.Color = color;
            return property;
        }

        /// <summary>
        /// Takes an object parameter and checks for its type.
        /// If it is a color, a color property of the type TColor is added to the material.
        /// If it is a texture, a texture property of the type TMap is added to the material.
        /// Otherwise, no property is added to the material.
        /// </summary>
        /// <typeparam name="TColor">Color Property type</typeparam>
        /// <typeparam name="TMap">Texture Property type</typeparam>
        /// <param name="obj">Property value</param>
        /// <param name="material">Material</param>
        void AddColorOrTextureProperty<TColor, TMap>(Object obj, Material material)
            where TColor : ColorProperty, new()
            where TMap : TextureProperty, new()
        {
            if (obj is Color)
            {
                MaterialProperty property = CreateColorProperty<TColor>((Color)obj);
                material.Properties.Add(property);
            }
            else if (obj is TextureReference)
            {
                MaterialProperty property = CreateTextureProperty<TMap>((TextureReference)obj);
                material.Properties.Add(property);
            }
            else
            {
                // unknown property is not added
            }
        }        

        /// <summary>
        /// Reads either a xmlColor or a texture from a XML node. 
        /// Thus it searches for &lt;xmlcolor&gt; or &lt;texture&gt;.
        /// </summary>
        /// <param name="xmlEffect">XML effect node</param>
        /// <param name="xmlChildNode">XML child node of &lt;effect&gt;</param>
        /// <returns>Color, Texture or null if neither is found</returns>
        Object ReadColorOrTexture(XmlNode xmlEffect, XmlNode xmlChildNode)
        {
            if (xmlChildNode == null) return null;

            XmlNode xmlColor = xmlChildNode.SelectSingleNode("color");
            if (xmlColor != null)
            {
                float[] values = XmlUtil.ParseFloats(xmlColor.InnerText);
                if (values.Length < 3 || values.Length > 4)
                    throw new FormatNotSupportedException("Unsupported Color format encountered",
                        xmlColor);

                if (values.Length == 4)
                {
                    return new Color(values[0], values[1], values[2], values[3]);
                }
                else
                {
                    return new Color(values[0], values[1], values[2]);
                }
            }
            else
            {
                XmlNode xmlTexture = xmlChildNode.SelectSingleNode("texture");
                if (xmlTexture == null) return null;

                TextureReference texture = new TextureReference();
                texture.TextureChannel = xmlTexture.Attributes["texcoord"].Value;

                // texture -> sampler
                string samplerId = xmlTexture.Attributes["texture"].Value;
                string imageId;
                XmlNode xmlSamplerSource = xmlEffect.SelectSingleNode(".//newparam[@sid='" +
                    samplerId + "']/sampler2D/source");
                if (xmlSamplerSource != null)
                {
                    // sampler -> surface
                    string surfaceId = xmlSamplerSource.InnerText.Trim();
                    XmlNode surfaceRef = xmlEffect.SelectSingleNode(".//newparam[@sid='" + surfaceId +
                                                                    "']/surface/init_from");
                    if (surfaceRef == null)
                        throw new NotFoundException("No image reference for texture found", xmlTexture);
                    
                    imageId = surfaceRef.InnerText.Trim();
                }
                else
                {
                    imageId = samplerId;
                }

                // texture/surface -> image
                XmlNode root = xmlEffect.OwnerDocument.DocumentElement;
                XmlNode imageInitFrom = root.SelectSingleNode("library_images/image[@id='" +
                    imageId + "']/init_from");
                if (imageInitFrom == null)
                    throw new NotFoundException("Image not found: " + imageId, xmlTexture);

                texture.Filename = imageInitFrom.InnerText.Trim();
                texture.Filename = texture.Filename.Replace("file://", "");

                if (texture.Filename.Contains(".exr"))
                {
                    // exr-images (32 bit per channel) are not supported right now
                    // silently ignore
                    // TODO: Implement Warning Log messages
                    return null;
                }

                return texture;
            }
        }

        /// <summary>
        /// Reads a float from a XML node.
        /// Thus it searches for &lt;float&gt;.
        /// If the passed xml node is null or does not contain
        /// a float node, the default value is returned.
        /// </summary>
        /// <param name="xmlChildNode">Any XML node</param>
        /// <param name="defaultValue">Default value if no float is found</param>
        /// <returns>Float value or default value if none is found</returns>
        float? ReadFloat(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return null;

            XmlNode xmlFloat = xmlNode.SelectSingleNode("float");
            if (xmlFloat == null)
                return null;

            return float.Parse(xmlFloat.InnerText.Trim(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns the name for the given XML data node.
        /// If no name is defined, the ID is returned instead.
        /// </summary>
        /// <param name="xmlMaterialNode">XML data node</param>
        /// <returns>Preferably Name, otherwise ID</returns>
        string GetName(XmlNode xmlMaterialNode)
        {
            // a name is optional per COLLADA spec
            if (xmlMaterialNode.Attributes["name"] == null)
            {
                // but an ID is required
                return xmlMaterialNode.Attributes["id"].Value;
            }
            else
            {
                return xmlMaterialNode.Attributes["name"].Value;
            }
        }

        /// <summary>
        /// When passed a data XML node this method tries
        /// to find the used effect in the effect library.
        /// If no effect is found null is returned.
        /// </summary>
        /// <param name="xmlMaterialNode">XML data node</param>
        /// <returns>XML effect node</returns>
        XmlNode FindUsedEffect(XmlNode xmlMaterialNode)
        {
            XmlNode xmlEffectInstance = xmlMaterialNode.SelectSingleNode("instance_effect");
            if (xmlEffectInstance == null) return null;

            String effectId = xmlEffectInstance.Attributes["url"].Value.Substring(1);
            XmlNode xmlRoot = xmlMaterialNode.OwnerDocument.DocumentElement;
            XmlNode xmlEffect = xmlRoot.SelectSingleNode("library_effects/effect[@id='" +
                effectId + "']");

            return xmlEffect;
        }

        #endregion
    }    
}
