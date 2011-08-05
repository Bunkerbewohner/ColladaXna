using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Omi.Xna.Collada.Importer.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message, XmlNode node)
            : base(message)
        {
            if (node != null)
            {
                Data.Add("XmlNodeName", node.Name);
                Data.Add("XmlNodeValue", node.InnerXml);
            }
        }
    }
}
