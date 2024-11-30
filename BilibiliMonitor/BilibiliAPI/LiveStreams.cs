using BilibiliMonitor.Models;
using Newtonsoft.Json;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using Image = SixLabors.ImageSharp.Image;
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class LiveStreams
    {
        private const string BasePicURL = "https://api.live.bilibili.com/room/v1/Room/get_info?room_id=";

        private const string BaseRoomInfoURL = "https://api.live.bilibili.com/room/v1/Room/get_info?room_id=";

        private const string BaseUIDQueryURL = "https://api.live.bilibili.com/live_user/v1/Master/info?uid=";

        public LiveStreams(long uid)
        {
            UID = uid;
            FetchUserInfo();
        }

        public static event Action<LiveStreamsModel.RoomInfo, LiveStreamsModel.UserInfo, string> OnLiveStreamUpdated;

        public int ErrorCount { get; set; }

        public string Name { get; set; }

        public bool ReFetchFlag { get; set; }

        public int RoomID { get; set; }

        public LiveStreamsModel.RoomInfo RoomInfo { get; set; }

        public bool Streaming { get => StreamingStatus == 1; }

        public long UID { get; set; }

        public List<long> UsedID { get; set; } = [];

        public LiveStreamsModel.UserInfo UserInfo { get; set; }

        private static List<LiveStreams> CheckItems { get; set; } = [];

        private static List<LiveStreams> DelayAddItems { get; set; } = [];

        private static List<LiveStreams> DelayRemoveItems { get; set; } = [];

        private static Timer UpdateCheck { get; set; }

        private static bool Updating { get; set; }

        private int StreamingStatus { get; set; }

        public static LiveStreams AddStream(long uid)
        {
            if (CheckItems.Any(x => x.UID == uid))
            {
                return CheckItems.First(x => x.UID == uid);
            }
            var live = new LiveStreams(uid);
            live.FetchRoomInfo();
            if (Updating)
            {
                DelayAddItems.Add(live);
            }
            else
            {
                CheckItems.Add(live);
            }
            StartCheckTimer();
            return live;
        }

        /// <summary>
        /// 获取监听的动态列表
        /// </summary>
        /// <returns>UID、用户名、是否在直播</returns>
        public static List<(long, string, bool)> GetStreamList()
        {
            List<(long, string, bool)> ls = new();
            foreach (var item in CheckItems)
            {
                ls.Add((item.UID, item.Name, item.Streaming));
            }
            return ls;
        }

        public static void RemoveStream(long uid)
        {
            if (Updating)
            {
                DelayRemoveItems.Add(CheckItems.FirstOrDefault(x => x.UID == uid));
            }
            else
            {
                CheckItems.Remove(CheckItems.FirstOrDefault(x => x.UID == uid));
            }
        }

        public void DownloadPics()
        {
            if (RoomInfo == null || UserInfo == null)
            {
                return;
            }

            _ = Helper.DownloadFile(RoomInfo.user_cover, Path.Combine(Config.BaseDirectory, "tmp")).Result;
            _ = Helper.DownloadFile(UserInfo.info.face, Path.Combine(Config.BaseDirectory, "tmp")).Result;
        }

        public string DrawLiveStreamPic()
        {
            if (RoomInfo == null || UserInfo == null)
            {
                return string.Empty;
            }

            using Image<Rgba32> main = new(652, 198, Color.White);
            string avatarPath = Path.Combine(Config.BaseDirectory, "tmp", UserInfo.info.face.GetFileNameFromURL());
            string coverPath = Path.Combine(Config.BaseDirectory, "tmp", RoomInfo.user_cover.GetFileNameFromURL());

            Image avatar = null;
            Image cover = null;
            try
            {
                avatar = Image.Load(avatarPath);
                cover = Image.Load(coverPath);
            }
            catch (Exception e)
            {
                LogHelper.Info("生成直播图像", $"name={Name}, avatar={avatarPath}, cover={coverPath}");
                throw e;
            }

            avatar.Mutate(x => x.Resize(48, 48));
            cover.Mutate(x => x.Resize((int)(cover.Width / (cover.Height / 198.0)), 198));

            main.Mutate(x => x.DrawImage(cover, new Point(0, 0), 1));
            using Image<Rgba32> Info = new(652 - cover.Width - 20, 178, Color.White);

            using Image<Rgba32> avatarFrame = new(48, 48, new Rgba32(255, 255, 255, 0));
            IPath circle = new EllipsePolygon(avatarFrame.Width / 2, avatarFrame.Height / 2, avatarFrame.Width / 2);
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            Info.Mutate(x => x.DrawImage(avatarFrame, new Point(0, 0), 1));

            Font smallFont = SystemFonts.CreateFont("Microsoft YaHei", 14);
            Font bigFont = SystemFonts.CreateFont("Microsoft YaHei", 20);
            TextOptions option = new(smallFont);
            PointF point = new(48 + 5, 15);
            var size = TextMeasurer.Measure(Name, option);
            Info.Mutate(x => x.DrawText(Name, smallFont, Color.Black, point));
            point = new(point.X + size.Width + 5, point.Y);
            Info.Mutate(x => x.DrawText("开播了", smallFont, Rgba32.ParseHex("#99a2aa"), point));
            point = new(10, point.Y + 60);
            option = new TextOptions(bigFont)
            {
                TextAlignment = TextAlignment.Start,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = Info.Width - 10,
                Origin = point
            };
            Info.Mutate(x => x.DrawText(option, RoomInfo.title, Color.Black));
            point = new(10, 178 - 20);
            Info.Mutate(x => x.DrawText(RoomInfo.area_name, smallFont, Rgba32.ParseHex("#99a2aa"), point));
            option = new TextOptions(smallFont);
            size = TextMeasurer.Measure(RoomInfo.area_name, option);
            point = new(point.X + size.Width, point.Y);
            Info.Mutate(x => x.DrawText($" · {RoomInfo.live_time}", smallFont, Rgba32.ParseHex("#99a2aa"), point));

            point = new(cover.Width + 10, 10);
            main.Mutate(x => x.DrawImage(Info, (Point)point, 1));

            string path = Path.Combine(Config.PicSaveBasePath, "BiliBiliMonitor", "LiveStream");
            Directory.CreateDirectory(path);
            string filename = $"{RoomID}.png";
            main.Save(Path.Combine(path, filename));
            avatar.Dispose();
            cover.Dispose();
            return Path.Combine("BiliBiliMonitor", "LiveStream", filename);
        }

        public bool FetchRoomInfo()
        {
            if (ReFetchFlag) { ReFetchFlag = false; return true; }
            string text = Helper.Get(BaseRoomInfoURL + RoomID).Result;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            LiveStreamsModel.RoomInfo_Main json = null;
            try
            {
                json = JsonConvert.DeserializeObject<LiveStreamsModel.RoomInfo_Main>(text);
                if (json == null)
                {
                    throw new Exception("json err");
                }
            }
            catch
            {
                if (Config.DebugMode)
                {
                    LogHelper.Info("拉取直播状态", $"Name={Name}, json={text}");
                }
                return false;
            }
            if (json.code == 0)
            {
                RoomInfo = json.data;
                if (json.data.live_status == 1 && json.data.live_time.StartsWith("0000"))
                {
                    return false;
                }

                if (json.data.live_status != StreamingStatus)
                {
                    StreamingStatus = json.data.live_status;
                    if (Streaming)
                    {
                        LogHelper.Info("直播状态变更", $"开播了，{Name} - {RoomInfo.title}");
                        return true;
                    }
                }
                if (Config.DebugMode)
                {
                    LogHelper.Info("直播检查", $"{Name}直播状态更新成功");
                }
            }
            else
            {
                Debug.WriteLine(json.message);
            }
            return false;
        }

        public void FetchUserInfo()
        {
            string text = Helper.Get(BaseUIDQueryURL + UID).Result;
            //string text = File.ReadAllText(@"E:\DO\live.txt");
            var json = JsonConvert.DeserializeObject<LiveStreamsModel.UserInfo_Main>(text);
            if (json != null && json.code == 0)
            {
                Name = json.data.info.uname;
                RoomID = json.data.room_id;
                UserInfo = json.data;
            }
        }

        private static void StartCheckTimer()
        {
            if (UpdateCheck == null)
            {
                UpdateCheck = new() { Interval = Config.RefreshInterval, AutoReset = true };
                UpdateCheck.Elapsed += UpdateCheck_Elapsed;
                UpdateCheck.Start();
            }
        }

        private static void UpdateCheck_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Updating)
            {
                return;
            }
            Updating = true;
            foreach (var item in DelayAddItems)
            {
                if (item != null)
                {
                    CheckItems.Add(item);
                }
            }
            foreach (var item in DelayRemoveItems)
            {
                CheckItems.Remove(item);
            }
            DelayAddItems = [];
            DelayRemoveItems = [];

            foreach (var live in CheckItems)// 直播检查
            {
                try
                {
                    if (live.FetchRoomInfo())
                    {
                        live.DownloadPics();
                        string pic = live.DrawLiveStreamPic();
                        GC.Collect();
                        live.ReFetchFlag = false;
                        live.ErrorCount = 0;

                        LogHelper.Info("开播", $"{live.UserInfo.info.uname}开播了，路径={pic}");
                        OnLiveStreamUpdated?.Invoke(live.RoomInfo, live.UserInfo, pic);
                    }
                }
                catch (Exception exc)
                {
                    live.ReFetchFlag = true;
                    LogHelper.Info("直播更新", exc.Message + exc.StackTrace, false);
                    live.ErrorCount++;
                    if (live.ErrorCount >= Config.LiveStreamRetryCount)
                    {
                        live.ReFetchFlag = false;
                        live.ErrorCount = 1;
                    }
                }
            }
            Updating = false;
        }
    }
}