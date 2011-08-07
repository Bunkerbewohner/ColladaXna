using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

namespace Base_Tests
{
    class TestDataLoader
    {
        static String testDataDir = "../../../Base Tests/TestData/";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename">Filename / path relative to TestData dir</param>
        /// <returns></returns>
        public static String GetTestFilePath(String filename)
        {
            return testDataDir + filename;
        }

        public static XmlDocument LoadDocument(String filename)
        {
            if (!File.Exists(filename))
                filename = GetTestFilePath(filename);

            // Namespaces are handled wrongly by XPath 1.0 and also we don't need
            // them anyway, so all namespaces are simply removed
            string xmlWithoutNamespaces = Regex.Replace(File.ReadAllText(filename),
                @"xmlns="".+?""", "");

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlWithoutNamespaces);            

            return xml;
        }
    }
}
