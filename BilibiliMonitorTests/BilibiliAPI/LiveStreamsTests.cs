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
    public class LiveStreamsTests
    {
        [TestMethod()]
        public void FetchLiveStreamTest()
        {
            LiveStreams.AddUID(692283831);
            Console.WriteLine(LiveStreams.LiveStreamData[692283831].live_status);
        }
    }
}