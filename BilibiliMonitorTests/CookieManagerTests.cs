using Microsoft.VisualStudio.TestTools.UnitTesting;
using BilibiliMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor.Tests
{
    [TestClass()]
    public class CookieManagerTests
    {
        [TestMethod()]
        public void GetCurrentCookieTest()
        {
            Config config = new("Config.json");
            config.LoadConfig();

            bool update = CookieManager.UpdateCookie(true);
            string cookie = CookieManager.GetCurrentCookie();
            Assert.IsTrue(!string.IsNullOrEmpty(cookie));
        }
    }
}