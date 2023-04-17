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
            string url = @"群:644933097(贪VAN♂难约 舰团群) QQ:857979875(你们老婆真棒) [CQ:rich,type=app,content={""ver"":""1.0.0.19"",""desc"":""哔哩哔哩"",""prompt"":""[QQ小程序]哔哩哔哩"",""config"":{""type"":""normal"",""width"":0,""height"":0,""forward"":1,""autoSize"":0,""ctime"":1681732132,""token"":""3a23f5cd057a1e36841963bc4d07c23f""},""needShareCallBack"":false,""app"":""com.tencent.miniapp_01"",""view"":""view_8C8E89B49BE609866298ADDFF2DBABA4"",""meta"":{""detail_1"":{""appid"":""1109937557"",""appType"":0,""title"":""哔哩哔哩"",""desc"":""来挑战一下！！!"",""icon"":""https:\/\/open.gtimg.cn\/open\/app_icon\/00\/95\/17\/76\/100951776_100_m.png?t=1681383459"",""url"":""m.q.qq.com\/a\/s\/c24182ef72259898503664da812b11a1"",""scene"":1036,""host"":{""uin"":857979875,""nick"":""ソ荒唐彡""},""shareTemplateId"":""8C8E89B49BE609866298ADDFF2DBABA4"",""shareTemplateData"":{},""qqdocurl"":""https:\/\/b23.tv\/iRGTQtZ?share_medium=android&share_source=qq&bbid=XXD0012D12B23F9E80C70E46A5907EBC5135D&ts=1681732130615"",""showLittleTail"":"""",""gamePoints"":"""",""gamePointsUrl"":"""",""preview"":""pubminishare-30161.picsz.qpic.cn\/a4a061c5-8420-42c9-914b-0579d1788cc9""}}}]";
            Console.WriteLine(Videos.ParseURLFromXML(url));
        }
    }
}