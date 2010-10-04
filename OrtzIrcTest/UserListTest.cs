using System.Diagnostics;
using FlamingIRC;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OrtzIRCTest
{


    /// <summary>
    ///This is a test class for UserListTest and is intended
    ///to contain all UserListTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UserListTest
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
        ///A test for Sort
        ///</summary>
        [TestMethod()]
        public void SortTest()
        {
            UserList target = new UserList();
            target.Add(new User { Nick = "Ortzinator"});
            target.Add(new User { Nick = "P90" });
            target.Add(new User { Nick = "gparent" });
            target.Add(new User { Nick = "DEADBEEF" });
            target.Add(new User { Nick = "@ChanServ" });
            target.Add(new User { Nick = "+SteveUK" });
            target.Add(new User { Nick = "@SpamServ" });
            target.Add(new User { Nick = "+HTF" });
            target.Add(new User { Nick = "+ninjers" });
            Comparison<User> comparison = (user1, user2) => user1.CompareTo(user2);
            target.Sort(comparison);
            foreach (User user in target)
            {
                Debug.WriteLine(user.Nick);
            }
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
