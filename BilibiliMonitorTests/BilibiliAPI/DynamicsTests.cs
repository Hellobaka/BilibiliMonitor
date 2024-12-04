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
            Config config = new("Config.json");
            config.LoadConfig();

            Dynamics dy = new(3493265644980448);
            dy.FetchDynamicList();
            for (int i = 0; i < 10; i++)
            {
                //var item = dy.DynamicList[i];
                //dy.DownloadPics(item);
                //Console.WriteLine(dy.DrawImage(item));
            }
            var item = dy.DynamicList[0];
            dy.DownloadPics(item);
            Console.WriteLine(dy.DrawImage(item));
        }
    }
}