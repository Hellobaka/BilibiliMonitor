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
            Config.CustomFont = "Lolita.ttf";
            string url = @"https://www.bilibili.com/video/BV1nNzzYnEQY?p=1&t=1";
            string id = Videos.ParseURL(url);
            Console.WriteLine(id);
            Assert.IsTrue(!string.IsNullOrEmpty(id));

            Console.WriteLine(Videos.DrawVideoPic(id));
        }
    }
}