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
            string url = @"[CQ:image,file=114514]b23.tv/BV1uZ421T7W8
这个游戏 应该什么掌机都能玩";
            string id = Videos.ParseURL(url);
            Console.WriteLine(id);
            Assert.IsTrue(!string.IsNullOrEmpty(id));

            Console.WriteLine(Videos.DrawVideoPic(id));
        }
    }
}