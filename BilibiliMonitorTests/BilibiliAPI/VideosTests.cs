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
    public class VideosTests
    {
        [TestMethod()]
        public void ParseURLTest()
        {
            string url = @"https://www.bilibili.com/video/BV1ze41137ye/?spm_id_from=333.788.recommend_more_video.2";
            Console.WriteLine(Videos.ParseURL(url));
        }
    }
}