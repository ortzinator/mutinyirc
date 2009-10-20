using System.Linq;
using System.Xml.Linq;
using FlamingIRC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using OrtzIRC.Common;
using System;
using System.Diagnostics;

namespace FlamingIrcTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class DccUtilTest
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

        [TestMethod]
        public void IPAddressToLongTest1()
        {
            System.Net.IPAddress comp = System.Net.IPAddress.Parse("127.0.0.1");

            long result = DccUtil.IPAddressToLong(comp);
            Assert.AreEqual(DccUtil.NetworkUnsignedLong(comp.Address), result);
        }

        [TestMethod]
        public void IPAddressToLongTest2()
        {
            System.Net.IPAddress comp = new System.Net.IPAddress(2130706433);

            long result = DccUtil.IPAddressToLong(comp);
            Assert.AreEqual(DccUtil.NetworkUnsignedLong(comp.Address), result);
        }

        [TestMethod]
        public void LoopbackTest()
        {
            Assert.AreEqual(System.Net.IPAddress.Parse("127.0.0.1"), DccUtil.LocalHost());
        }
    }
}
