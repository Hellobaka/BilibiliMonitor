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
using System.IO;
using System.Linq;
using System.Timers;
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Bangumi
    {
        private const string BaseEpURL = "https://api.bilibili.com/pgc/web/season/section?season_id=";

        private const string BaseInfoURL = "https://www.biliplus.com/api/bangumi?season=";

        public Bangumi(long seasonId)
        {
            SeasonID = seasonId;
            FetchInfo();
        }

        public static event Action<Bangumi> OnBanguimiEnded;

        public static event Action<BangumiModel.DetailInfo, BangumiModel.Episode, string> OnBanguimiUpdated;

        public BangumiModel.DetailInfo BangumiInfo { get; set; }

        public bool Ended { get; set; }

        public int ErrorCount { get; set; }

        public BangumiModel.Episode LastEp { get; set; }

        public long LastID { get; set; }

        public string Name { get; set; }

        public bool ReFetchFlag { get; set; }

        public long SeasonID { get; set; }

        public List<long> UsedID { get; set; } = [];

        private static List<Bangumi> CheckItems { get; set; } = [];

        private static List<Bangumi> DelayAddItems { get; set; } = [];

        private static List<Bangumi> DelayRemoveItems { get; set; } = [];

        private static Timer UpdateCheck { get; set; }

        private static bool Updating { get; set; }

        public static Bangumi AddBangumi(long seasonId)
        {
            if (CheckItems.Any(x => x.SeasonID == seasonId))
            {
                return CheckItems.First(x => x.SeasonID == seasonId);
            }

            Bangumi ban = new(seasonId);
            if (string.IsNullOrWhiteSpace(ban.Name))
            {
                return null;
            }
            if (Updating)
            {
                DelayAddItems.Add(ban);
            }
            else
            {
                CheckItems.Add(ban);
            }
            StartCheckTimer();
            return ban;
        }

        /// <summary>
        /// 获取当前监听的番剧列表
        /// </summary>
        /// <returns>SeasonID、名称</returns>
        public static List<(long, string)> GetBangumiList()
        {
            List<(long, string)> ls = [];
            foreach (var item in CheckItems)
            {
                ls.Add((item.SeasonID, item.Name));
            }
            return ls;
        }

        public static void RemoveBangumi(long seasonId)
        {
            if (Updating)
            {
                DelayRemoveItems.Add(CheckItems.FirstOrDefault(x => x.SeasonID == seasonId));
            }
            else
            {
                CheckItems.Remove(CheckItems.FirstOrDefault(x => x.SeasonID == seasonId));
            }
        }

        public void DownloadPic()
        {
            if (BangumiInfo == null || LastEp == null)
            {
                return;
            }

            _ = Helper.DownloadFile(LastEp?.cover, Path.Combine(Config.BaseDirectory, "tmp")).Result;
            _ = Helper.DownloadFile(BangumiInfo.result.squareCover, Path.Combine(Config.BaseDirectory, "tmp")).Result;
        }

        public string DrawLastEpPic()
        {
            if (BangumiInfo == null || LastEp == null)
            {
                return string.Empty;
            }

            using Image<Rgba32> main = new(652, 198, Color.White);
            using Image avatar =
                Image.Load(Path.Combine(Config.BaseDirectory, "tmp", BangumiInfo.result.squareCover.GetFileNameFromURL()));
            using Image cover =
                Image.Load(Path.Combine(Config.BaseDirectory, "tmp", LastEp.cover.GetFileNameFromURL()));

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
            Info.Mutate(x => x.DrawText("更新了", smallFont, Rgba32.ParseHex("#99a2aa"), point));
            point = new(10, point.Y + 60);
            option = new TextOptions(bigFont)
            {
                TextAlignment = TextAlignment.Start,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = Info.Width,
                Origin = point
            };
            Info.Mutate(x => x.DrawText(option, LastEp.long_title, Color.Black));
            point = new(10, 178 - 20);
            string epCount = $"第{LastEp.title}话";
            Info.Mutate(x => x.DrawText(epCount, smallFont, Rgba32.ParseHex("#99a2aa"), point));
            option = new TextOptions(smallFont);
            size = TextMeasurer.Measure(epCount, option);
            point = new(point.X + size.Width, point.Y);
            Info.Mutate(x => x.DrawText($" · {DateTime.Now:G}", smallFont, Rgba32.ParseHex("#99a2aa"), point));

            point = new(cover.Width + 10, 10);
            main.Mutate(x => x.DrawImage(Info, (Point)point, 1));

            string path = Path.Combine(Config.PicSaveBasePath, "BiliBiliMonitor", "Bangumi");
            Directory.CreateDirectory(path);
            string filename = $"{LastEp.id}.png";
            main.Save(Path.Combine(path, filename));
            return Path.Combine("BiliBiliMonitor", "Bangumi", filename);
        }

        public bool FetchEPDetail()
        {
            if (BangumiInfo == null)
            {
                return false;
            }

            string text = Helper.Get(BaseEpURL + SeasonID).Result;
            BangumiModel.Main json = null;
            try
            {
                json = JsonConvert.DeserializeObject<BangumiModel.Main>(text);
            }
            catch
            {
                if (Config.DebugMode)
                {
                    LogHelper.Info("拉取番剧状态", $"Name={Name}, json={text}");
                }
                return false;
            }
            if (json.code == 0)
            {
                if (Config.DebugMode)
                {
                    LogHelper.Info("番剧检查", $"{Name}番剧信息更新成功");
                }

                if (json.result.main_section == null)
                {
                    return false;
                }

                LastEp = json.result.main_section.episodes.Last();
                if ((ReFetchFlag || LastEp.id != LastID) && UsedID.Any(x => x == LastID) is false)
                {
                    LogHelper.Info("更新番剧信息", "新的剧集出现了");
                    LastID = json.result.main_section.episodes.Last().id;
                    UsedID.Add(LastID);
                    FetchInfo();
                    ReFetchFlag = false;
                    return true;
                }
                return false;
            }
            if (Config.DebugMode)
            {
                LogHelper.Info("更新番剧信息", text, false);
            }
            return false;
        }

        public bool FetchInfo()
        {
            string text = Helper.Get(BaseInfoURL + SeasonID).Result;
            var json = JsonConvert.DeserializeObject<BangumiModel.DetailInfo>(text);
            if (json == null)
            {
                if (Config.DebugMode)
                {
                    LogHelper.Info("拉取番剧详情", $"name={Name}, json={text}", false);
                }
                return false;
            }
            if (json is { code: 0 })
            {
                BangumiInfo = json;
                Name = json.result.title;
                if (Config.DebugMode)
                {
                    LogHelper.Info("LastID", $"{json.result.episodes.Length}: {LastID}");
                    LogHelper.Info("拉取番剧信息", $"{Name} 番剧信息拉取成功");
                }
                return true;
            }
            LogHelper.Info("拉取番剧信息", text, false);
            return false;
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

            foreach (var bangumi in CheckItems.Where(x => !x.Ended))
            {
                try
                {
                    if (bangumi.FetchEPDetail())
                    {
                        bangumi.DownloadPic();
                        string pic = bangumi.DrawLastEpPic();
                        GC.Collect();
                        bangumi.ReFetchFlag = false;
                        bangumi.ErrorCount = 0;

                        LogHelper.Info("番剧更新", $"{bangumi.Name} 更新了，路径={pic}");
                        OnBanguimiUpdated?.Invoke(bangumi.BangumiInfo, bangumi.LastEp, pic);
                    }
                    if (bangumi.BangumiInfo.result.is_finish == "1")
                    {
                        LogHelper.Info("番剧完结", $"{bangumi.Name} 已完结，清除监测");
                        bangumi.Ended = true;
                        OnBanguimiEnded?.Invoke(bangumi);
                    }
                }
                catch (Exception exc)
                {
                    bangumi.ReFetchFlag = true;
                    LogHelper.Info("番剧更新", exc.Message + exc.StackTrace, false);
                    bangumi.ErrorCount++;
                    if (bangumi.ErrorCount >= Config.BangumiRetryCount)
                    {
                        bangumi.ReFetchFlag = false;
                        bangumi.ErrorCount = 1;
                    }
                }
            }
            Updating = false;
            CheckItems.RemoveAll(x => x.Ended);
        }
    }
}