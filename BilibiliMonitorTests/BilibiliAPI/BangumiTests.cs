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
    public class BangumiTests
    {
        [TestMethod()]
        public void FetchEPDetailTest()
        {
            Config.CustomFont = "Lolita.ttf";

            Bangumi bangumi = new Bangumi(48029);
            bangumi.FetchEPDetail();
            Console.WriteLine(bangumi.Name);
            if (bangumi.BangumiInfo.result.is_finish == "1")
            {
                Console.WriteLine($"番剧完结{bangumi.Name} 已完结，清除监测");
            }
            bangumi.DownloadPic();
            Console.WriteLine(bangumi.DrawLastEpPic());
        }
    }
}