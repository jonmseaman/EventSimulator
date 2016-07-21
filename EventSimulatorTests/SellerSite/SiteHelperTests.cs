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
