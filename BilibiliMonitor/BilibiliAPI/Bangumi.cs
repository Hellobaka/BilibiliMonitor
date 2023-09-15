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
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Bangumi
    {
        private string baseEpURL = "https://api.bilibili.com/pgc/web/season/section?season_id=";

        private string baseInfoURL = "https://www.biliplus.com/api/bangumi?season=";

        private bool init = true;

        public Bangumi(int seasonId)
        {
            SeasonID = seasonId;
            FetchInfo();
        }

        public BangumiModel.DetailInfo BangumiInfo { get; set; }

        public BangumiModel.Episode LastEp { get; set; }

        public int LastID { get; set; }

        public string Name { get; set; }

        public bool ReFetchFlag { get; set; }

        public int SeasonID { get; set; }

        public List<int> UsedID { get; set; } = new();

        public void DownloadPic()
        {
            if (BangumiInfo == null || LastEp == null)
            {
                return;
            }

            _ = Helper.DownloadFile(LastEp?.cover, Path.Combine(UpdateChecker.BasePath, "tmp")).Result;
            _ = Helper.DownloadFile(BangumiInfo.result.squareCover, Path.Combine(UpdateChecker.BasePath, "tmp")).Result;
        }

        public string DrawLastEpPic()
        {
            if (BangumiInfo == null || LastEp == null)
            {
                return string.Empty;
            }

            using Image<Rgba32> main = new(652, 198, Color.White);
            using Image avatar =
                Image.Load(Path.Combine(UpdateChecker.BasePath, "tmp", BangumiInfo.result.squareCover.GetFileNameFromURL()));
            using Image cover =
                Image.Load(Path.Combine(UpdateChecker.BasePath, "tmp", LastEp.cover.GetFileNameFromURL()));

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

            string path = Path.Combine(UpdateChecker.PicPath, "BiliBiliMonitor", "Bangumi");
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

            string text = Helper.Get(baseEpURL + SeasonID).Result;
            BangumiModel.Main json = null;
            try
            {
                json = JsonConvert.DeserializeObject<BangumiModel.Main>(text);
            }
            catch
            {
                if (UpdateChecker.Instance.DebugMode)
                {
                    LogHelper.Info("拉取番剧状态", $"Name={Name}, json={text}");
                }
                return false;
            }
            if (json.code == 0)
            {
                if (UpdateChecker.Instance.DebugMode)
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
                    if (init)//初始化
                    {
                        init = false;
                        return false;
                    }
                    return true;
                }
                return false;
            }
            if (UpdateChecker.Instance.DebugMode)
            {
                LogHelper.Info("更新番剧信息", text, false);
            }
            return false;
        }

        public bool FetchInfo()
        {
            string text = Helper.Get(baseInfoURL + SeasonID).Result;
            var json = JsonConvert.DeserializeObject<BangumiModel.DetailInfo>(text);
            if (json == null)
            {
                if (UpdateChecker.Instance.DebugMode)
                {
                    LogHelper.Info("拉取番剧详情", $"name={Name}, json={text}", false);
                }
                return false;
            }
            if (json is { code: 0 })
            {
                BangumiInfo = json;
                Name = json.result.title;
                if (UpdateChecker.Instance.DebugMode)
                {
                    LogHelper.Info("LastID", $"{json.result.episodes.Length}: {LastID}");
                    LogHelper.Info("拉取番剧信息", $"{Name} 番剧信息拉取成功");
                }
                return true;
            }
            LogHelper.Info("拉取番剧信息", text, false);
            return false;
        }
    }
}