using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventSimulator.SellerSite;

namespace EventSimulatorTests.SellerSite
{
    /// <summary>
    /// Summary description for SiteHelperTests
    /// </summary>
    [TestClass]
    public class SiteHelperTests
    {
        public SiteHelperTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion


        #region IsUrlAProductPageTests

        [TestMethod]
        public void IsUrlAProductPageTrueTest()
        {
            Assert.IsTrue(SiteHelper.IsUrlAProductPage("/products/2"));
        }

        [TestMethod]
        public void IsUrlAProductPageFalseTest()
        {
            Assert.IsFalse(SiteHelper.IsUrlAProductPage("/products/2/hello"));
        }

        [TestMethod]
        public void IsUrlAProductPageFalse2Test()
        {
            Assert.IsFalse(SiteHelper.IsUrlAProductPage("/notproducts/2/hello"));
        }

        [TestMethod]
        public void IsUrlAProductPageFalse3Test()
        {
            Assert.IsFalse(SiteHelper.IsUrlAProductPage("/"));
        }

        #endregion

    }
}
