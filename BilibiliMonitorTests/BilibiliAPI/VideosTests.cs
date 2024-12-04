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
            string url = @"https://www.bilibili.com/video/BV1t7zhYgE7u/";
            string id = Videos.ParseURL(url);
            Console.WriteLine(id);
            Assert.IsTrue(!string.IsNullOrEmpty(id));

            Console.WriteLine(Videos.DrawVideoPic(id));
        }
    }
}