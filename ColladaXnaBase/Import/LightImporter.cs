using System;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ColladaXna.Base.Import
{
    using DirectionalLight = ColladaXna.Base.Lighting.DirectionalLight;
    using ColladaXna.Base.Lighting;
    using ColladaXna.Base.Util;

    public class LightImporter : IColladaImporter
    {
        #region IColladaImporter Member

        public void Import(XmlNode xmlRoot, ColladaModel model)
        {
            XmlNodeList xmlLightInstances = xmlRoot.SelectNodes(".//instance_light");
            if (xmlLightInstances == null || xmlLightInstances.Count == 0)
            {
                // silently quit (lights are not vital)
                return;
            }

            model.Lights = (from XmlNode xmlInstance in xmlLightInstances
                            where CanImport(xmlInstance)
                            select ImportLight(xmlInstance)).ToList();            
        }

        #endregion

        #region ILightImporter Member

        /// <summary>
        /// When given an instance_light XML node this method returns the referenced
        /// light XML node from the light library.
        /// </summary>
        /// <param name="xmlInstanceLight"></param>
        /// <returns></returns>
        XmlNode GetLightFromInstance(XmlNode xmlInstanceLight)
        {
            // Find Light definition
            string lightId = xmlInstanceLight.Attributes["url"].Value.Substring(1);
            XmlNode xmlRoot = xmlInstanceLight.OwnerDocument.DocumentElement;
            XmlNode xmlLight = xmlRoot.SelectSingleNode(".//light[@id='" + lightId + "']");
            return xmlLight;
        }

        /// <summary>
        /// Determines whether the light can be imported (i.e. supported).
        /// Currently supported light types are:
        /// - directional
        /// - ambient
        /// </summary>
        /// <param name="xmlLightInstance">instance_light XML node</param>
        /// <returns>True if the light type is supported</returns>
        bool CanImport(XmlNode xmlLightInstance)
        {
            XmlNode xmlLightNode = GetLightFromInstance(xmlLightInstance);
            if (xmlLightNode == null) return false;
            XmlNode xmlTechnique = xmlLightNode.SelectSingleNode("technique_common");
            if (xmlTechnique == null) return false;
            XmlNode xmlNode = xmlTechnique.SelectSingleNode("directional|ambient"); //|point|spot
            return xmlNode != null;
        }

        /// <summary>
        /// Imports a light referenced by a light instance.
        /// Inputs are assumed to be supported (CanImport must return true for them).
        /// </summary>
        /// <param name="xmlInstanceLight">instance_light XML node</param>
        /// <returns>The imported light</returns>
        Light ImportLight(XmlNode xmlInstanceLight)
        {            
            string lightId = xmlInstanceLight.Attributes["url"].Value.Substring(1);
            XmlNode xmlRoot = xmlInstanceLight.OwnerDocument.DocumentElement;
            XmlNode xmlTechnique = xmlRoot.SelectSingleNode(".//light[@id='" + lightId + "']/technique_common");
            XmlNode xmlNode = xmlTechnique.SelectSingleNode("directional|ambient|point|spot");
            
            Light light;
            int countDirectional = 0;            

            switch (xmlNode.Name)
            {
                case "directional":
                    DirectionalLight dirLight = new DirectionalLight();
                    dirLight.Name = "DirLight" + (++countDirectional);

                    // Color
                    XmlNode xmlColor = xmlNode.SelectSingleNode("color");
                    Vector3 values = XmlUtil.ParseVector3(xmlColor.InnerText);
                    dirLight.Color = new Color(values);

                    // Direction
                    Matrix transform = XmlUtil.CreateNodeTransform(xmlInstanceLight);
                    Vector3 dir = Vector3.Transform(new Vector3(0, 0, -1), transform);
                    dirLight.Direction = dir;

                    light = dirLight;

                    break;

                case "ambient":
                    AmbientLight ambientLight = new AmbientLight();
                    ambientLight.Name = "AmbientLight";

                    // Color
                    xmlColor = xmlNode.SelectSingleNode("color");
                    values = XmlUtil.ParseVector3(xmlColor.InnerText);
                    ambientLight.Color = new Color(values);

                    light = ambientLight;

                    break;

                default:
                    throw new NotSupportedException("Lights of type '" + 
                        xmlNode.Name + "' are not supported");
            }

            // Name: Bad Idea (if unique names are used a new shader has to be generated
            // for each name variant)
            //XmlNode xmlName = xmlRoot.SelectSingleNode(".//light[@id='" + lightId +
            //    "']/@name");
            //if (xmlName != null) light.Name = xmlName.Value;

            return light;
        }

        #endregion
    }
}
