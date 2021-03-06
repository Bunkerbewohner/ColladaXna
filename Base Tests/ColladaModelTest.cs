﻿using ColladaXna.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Base_Tests
{
    
    
    /// <summary>
    ///This is a test class for ColladaModelTest and is intended
    ///to contain all ColladaModelTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ColladaModelTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ColladaModel Constructor
        ///</summary>
        [TestMethod()]
        public void ColladaModelConstructorTest()
        {
            string filename = TestDataLoader.GetTestFilePath("APC_animation.DAE");
            Assert.IsTrue(File.Exists(filename), (new FileInfo(filename).FullName) + " does not exist");
            
            ColladaModel target = new ColladaModel(filename);            
        }
    }
}
