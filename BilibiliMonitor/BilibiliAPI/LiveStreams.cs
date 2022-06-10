using BilibiliMonitor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public static class LiveStreams
    {
        private static string BaseURL = "http://api.live.bilibili.com/room/v1/Room/get_status_info_by_uids";
        public static Dictionary<int, LiveStreamsModel.RoomInfo> LiveStreamData { get; set; } = new();

        public static void AddUID(int uid)
        {
            FetchLiveStream(new List<int> {uid});
        }

        public static void RemoveUID(int uid)
        {
            LiveStreamData.Remove(uid);
        }

        public static List<int> FetchLiveStream()
        {
            return FetchLiveStream(LiveStreamData.Keys.ToList());
        }
        /// <summary>
        /// 根据传入的UID来更新直播列表
        /// </summary>
        /// <param name="uids">需要拉取的UID</param>
        /// <returns>有状态更新的用户</returns>
        public static List<int> FetchLiveStream(List<int> uids)
        {
            if (uids == null || uids.Count == 0) return new();
            string text = Helper.Post(BaseURL, new {uids}).Result;
            var json = JsonConvert.DeserializeObject<LiveStreamsModel.Main>(text);
            List<int> update = new();
            if (json.code == 0)
            {
                foreach (var item in uids)
                {
                    var t = json.data[item.ToString()].ToObject<LiveStreamsModel.RoomInfo>();
                    if (LiveStreamData.ContainsKey(item))
                    {
                        if (t.live_status != LiveStreamData[item].live_status)
                        {
                            update.Add(item);
                            var room = LiveStreamData[item];
                            if (room.live_status == 1)
                            {
                                LogHelper.Info("直播状态变更", $"开播了，{room.uname} - {room.title}");
                            }
                        }

                        LiveStreamData[item] = t;
                    }
                    else
                    {
                        LiveStreamData.Add(item, t);
                    }
                    LogHelper.Info("直播检查", $"{t.uname}直播状态更新成功");
                }
            }
            else
            {
                Debug.WriteLine(json.message);
            }

            return update;
        }

        public static string DrawLiveStreamPic(int uid)
        {
            if (!LiveStreamData.ContainsKey(uid) || LiveStreamData[uid] == null)
            {
                return string.Empty;
            }

            return DrawLiveStreamPic(LiveStreamData[uid]);
        }

        public static string DrawLiveStreamPic(LiveStreamsModel.RoomInfo item)
        {
            if (item == null) return string.Empty;
            using Image<Rgba32> main = new(652, 198, Color.White);
            using Image avatar =
                Image.Load(Path.Combine(UpdateChecker.BasePath, "tmp", item.face.GetFileNameFromURL()));
            using Image cover =
                Image.Load(Path.Combine(UpdateChecker.BasePath, "tmp", item.cover_from_user.GetFileNameFromURL()));

            avatar.Mutate(x => x.Resize(48, 48));
            cover.Mutate(x => x.Resize((int) (cover.Width / (cover.Height / 198.0)), 198));

            main.Mutate(x => x.DrawImage(cover, new Point(0, 0), 1));
            using Image<Rgba32> Info = new(652 - cover.Width - 20, 178, Color.White);
            
            using Image<Rgba32> avatarFrame = new(48, 48, new Rgba32(255, 255, 255, 0));
            IPath circle = new EllipsePolygon(avatarFrame.Width / 2, avatarFrame.Height / 2, avatarFrame.Width / 2);
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            Info.Mutate(x=>x.DrawImage(avatarFrame, new Point(0,0), 1));

            Font smallFont = SystemFonts.CreateFont("Microsoft YaHei", 14);
            Font bigFont = SystemFonts.CreateFont("Microsoft YaHei", 20);
            TextOptions option = new TextOptions(smallFont);
            PointF point = new(48 + 5, 15);
            var size = TextMeasurer.Measure(item.uname, option);
            Info.Mutate(x=>x.DrawText(item.uname, smallFont, Color.Black, point));
            point = new(point.X + size.Width + 5, point.Y);
            Info.Mutate(x=>x.DrawText("开播了", smallFont, Rgba32.ParseHex("#99a2aa"), point));
            point = new(10, point.Y + 60);
            option = new TextOptions(bigFont)
            {
                TextAlignment = TextAlignment.Start,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = Info.Width,
                Origin = point
            };
            Info.Mutate(x=>x.DrawText(option, item.title, Color.Black));
            point = new(10, 178 - 20);
            Info.Mutate(x=>x.DrawText(item.area_v2_name, smallFont, Rgba32.ParseHex("#99a2aa"), point));
            option = new TextOptions(smallFont);
            size = TextMeasurer.Measure(item.area_v2_name, option);
            point = new(point.X + size.Width, point.Y);
            Info.Mutate(x=>x.DrawText($" · {Helper.TimeStamp2DateTime(item.live_time):G}", smallFont, Rgba32.ParseHex("#99a2aa"), point));

            point = new(cover.Width + 10 ,10);
            main.Mutate(x=>x.DrawImage(Info, (Point) point, 1));

            string path = Path.Combine(UpdateChecker.PicPath, "BiliBiliMonitor", "LiveStream");
            Directory.CreateDirectory(path);
            string filename = $"{item.room_id}.png";
            main.Save(Path.Combine(path, filename));
            return Path.Combine("BiliBiliMonitor", "LiveStream", filename);
        }

        public static void DownloadPics(int uid)
        {
            if (!LiveStreamData.ContainsKey(uid) || LiveStreamData[uid] == null)
            {
                return;
            }

            DownloadPics(LiveStreamData[uid]);
        }

        public static void DownloadPics(LiveStreamsModel.RoomInfo item)
        {
            if (item == null) return;
            _ = Helper.DownloadFile(item.cover_from_user, Path.Combine(UpdateChecker.BasePath, "tmp")).Result;
            _ = Helper.DownloadFile(item.face, Path.Combine(UpdateChecker.BasePath, "tmp")).Result;
        }
    }
}