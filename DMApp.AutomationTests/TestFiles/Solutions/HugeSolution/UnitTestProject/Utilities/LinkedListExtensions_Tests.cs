namespace UnitTestProject.Utilities
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;

    [TestClass]
    public class LinkedListExtensions_Tests
    {
        [TestMethod]
        public void IndexOf_Test1()
        {
            LinkedList<int> list = new LinkedList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            int index = list.IndexOf(3);
            Assert.AreEqual(3, index);
        }

        [TestMethod]
        public void IndexOf_Test2()
        {
            LinkedList<int> list = new LinkedList<int>(new int[] { 0, 1, 2, 3, 4, 5, 6 });
            int index = list.IndexOf(7);
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void IndexOf_Test3()
        {
            LinkedList<int> list = new LinkedList<int>();
            int index = list.IndexOf(7);
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void IndexOf_Test4()
        {
            LinkedList<string> list = new LinkedList<string>(new string[] { "0", "1", null, "4" });
            int index = list.IndexOf("1");
            Assert.AreEqual(1, index);
        }

        [TestMethod]
        public void IndexOf_Test5()
        {
            LinkedList<string> list = new LinkedList<string>(new string[] { "0", "1", null, "4" });
            int index = list.IndexOf(null);
            Assert.AreEqual(2, index);
        }

        [TestMethod]
        public void IndexOf_Test6()
        {
            LinkedList<string> list = new LinkedList<string>(new string[] { "0", "1", null, "4" });
            int index = list.IndexOf("4");
            Assert.AreEqual(3, index);
        }
    }
}
