using Microsoft.VisualStudio.TestTools.UnitTesting;
using BilibiliMonitor.BilibiliAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor.BilibiliAPI.Tests
{
    [TestClass()]
    public class DynamicsTests
    {
        [TestMethod()]
        public void FetchDynamicListTest()
        {
            Dynamics dy = new(692283831);
            dy.FetchDynamicList();
            Console.WriteLine(dy.LastDynamicID);
        }
    }
}