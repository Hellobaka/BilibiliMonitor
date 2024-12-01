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
            LiveStreams liveStreams = new LiveStreams(1838190318);
            liveStreams.FetchRoomInfo();
            liveStreams.DownloadPics();
            Console.WriteLine(liveStreams.DrawLiveStreamPic());
        }
    }
}