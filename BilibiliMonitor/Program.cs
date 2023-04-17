using BilibiliMonitor.BilibiliAPI;
using Newtonsoft.Json.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using static BilibiliMonitor.Models.BangumiModel;
using static BilibiliMonitor.Models.LiveStreamsModel;

namespace BilibiliMonitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string xml = "群:644933097(贪VAN♂难约 舰团群) QQ:857979875(你们老婆真棒) [CQ:rich,type=app,content={\"ver\":\"1.0.0.19\",\"desc\":\"哔哩哔哩\",\"prompt\":\"[QQ小程序]哔哩哔哩\",\"config\":{\"type\":\"normal\",\"width\":0,\"height\":0,\"forward\":1,\"autoSize\":0,\"ctime\":1681732132,\"token\":\"3a23f5cd057a1e36841963bc4d07c23f\"},\"needShareCallBack\":false,\"app\":\"com.tencent.miniapp_01\",\"view\":\"view_8C8E89B49BE609866298ADDFF2DBABA4\",\"meta\":{\"detail_1\":{\"appid\":\"1109937557\",\"appType\":0,\"title\":\"哔哩哔哩\",\"desc\":\"来挑战一下！！!\",\"icon\":\"https:\\/\\/open.gtimg.cn\\/open\\/app_icon\\/00\\/95\\/17\\/76\\/100951776_100_m.png?t=1681383459\",\"url\":\"m.q.qq.com\\/a\\/s\\/c24182ef72259898503664da812b11a1\",\"scene\":1036,\"host\":{\"uin\":857979875,\"nick\":\"ソ荒唐彡\"},\"shareTemplateId\":\"8C8E89B49BE609866298ADDFF2DBABA4\",\"shareTemplateData\":{},\"qqdocurl\":\"https:\\/\\/b23.tv\\/iRGTQtZ?share_medium=android&share_source=qq&bbid=XXD0012D12B23F9E80C70E46A5907EBC5135D&ts=1681732130615\",\"showLittleTail\":\"\",\"gamePoints\":\"\",\"gamePointsUrl\":\"\",\"preview\":\"pubminishare-30161.picsz.qpic.cn\\/a4a061c5-8420-42c9-914b-0579d1788cc9\"}}}]";
            var video = Videos.ParseURLFromXML(xml);
            Console.WriteLine(video);
        //    UpdateChecker.PicPath = @"E:\酷Q机器人插件开发\OPQBot-Native\data\image";
        //    UpdateChecker.BasePath = @"E:\酷Q机器人插件开发\OPQBot-Native\data\app\me.cqp.luohuaming.BilibiliUpdateChecker";
        //    LiveStreams live = new(692283831);
        //    live.FetchRoomInfo();
        //    live.DownloadPics();
        //    string c = live.DrawLiveStreamPic();
        //    Console.WriteLine(c);
            // UpdateChecker update = new("", "");
            // update.DynamicCheckCD = 2;
            // update.OnDynamic += UpdateChecker_OnDynamic;
            // update.OnStream += UpdateChecker_OnStream;
            // update.OnBangumi += UpdateChecker_OnBangumi;
            // update.OnBangumiEnd += Update_OnBangumiEnd;
            // var dynamics = Config.GetConfig<int[]>("Dynamics");
            // var streams = Config.GetConfig<int[]>("Streams");
            // var bangumis = Config.GetConfig<int[]>("Bangumis");
            // foreach (var item in dynamics)
            // {
            //     update.AddDynamic(item);
            // }
            // foreach (var item in streams)
            // {
            //     update.AddStream(item);
            // }
            // foreach (var item in bangumis)
            // {
            //     update.AddBangumi(item);
            // }
            // update.Start();
            // while (true)
            // {
            //     Console.ReadLine();
            // }
        }
        private static void Update_OnBangumiEnd(BilibiliMonitor.BilibiliAPI.Bangumi bangumi)
        {
            int sid = bangumi.SeasonID;
            var bangumis = Config.GetConfig<List<int>>("Bangumis");
            var group = Config.GetConfig<JObject>("Monitor_Bangumis");
            foreach (JProperty item in group.Properties())
            {
                if ((item.Value as JArray).Any(x => {
                    var p = (int)x;
                    return p == sid;
                }))
                {
                    item.Value.Children().FirstOrDefault(x => x.Value<int>() == sid)?.Remove();
                }
            }
            bangumis.Remove(sid);
            Config.WriteConfig("Bangumis", bangumis);
            Config.WriteConfig("Monitor_Bangumis", group);
        }

        private static void UpdateChecker_OnStream(RoomInfo roomInfo, UserInfo userInfo, string picPath)
        {
            var group = Config.GetConfig<JObject>("Monitor_Stream");
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if (o.Any(x => x == userInfo.info.uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{userInfo.info.uname} 开播了, https://live.bilibili.com/{roomInfo.room_id}");
                    sb.Append(picPath);
                    Console.WriteLine($"[{id.Name}]: " + sb.ToString());
                }
            }
        }

        private static void UpdateChecker_OnDynamic(BilibiliMonitor.Models.DynamicModel.Item item, int uid, string picPath)
        {
            var group = Config.GetConfig<JObject>("Monitor_Dynamic");
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if (o.Any(x => x == uid))
                {
                    StringBuilder sb = new();
                    sb.Append($"{item.modules.module_author.name} 更新了动态, https://t.bilibili.com/{item.id_str}");
                    sb.Append(picPath);
                    Console.WriteLine($"[{id.Name}]: " + sb.ToString());
                }
            }
        }
        private static void UpdateChecker_OnBangumi(DetailInfo bangumi, Episode epInfo, string picPath)
        {
            var group = Config.GetConfig<JObject>("Monitor_Bangumis");
            foreach (JProperty id in group.Properties())
            {
                var o = id.Value.ToObject<int[]>();
                if (o.Any(x => x == Convert.ToInt32(bangumi.result.season_id)))
                {
                    StringBuilder sb = new();
                    sb.Append($"{bangumi.result.title} 更新了新的一集, {epInfo.share_url}");
                    sb.Append(picPath);
                    Console.WriteLine($"[{id.Name}]: " + sb.ToString());
                }
            }
        }
    }
}
