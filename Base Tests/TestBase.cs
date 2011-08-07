using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using ColladaXna.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Base_Tests
{
    public class TestBase
    {        
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; protected set; }
        
        public static XmlDocument LoadDocument(String filename)
        {
            return TestDataLoader.LoadDocument(filename);                        
        }        
    }
}
