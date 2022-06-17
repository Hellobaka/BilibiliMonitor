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
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Bangumi
    {
        string baseInfoURL = "http://api.bilibili.com/pgc/view/web/season?season_id=";
        string baseEpURL = "https://api.bilibili.com/pgc/web/season/section?season_id=";
        public int SeasonID { get; set; }
        public string Name { get; set; }
        public int LastID { get; set; }
        public BangumiModel.DetailInfo BangumiInfo { get; set; }
        public BangumiModel.Detail_Episode LastEp { get; set; }
        public Bangumi(int seasonId)
        {
            SeasonID = seasonId;
            FetchInfo();
        }
        public bool FetchInfo()
        {
            string text = Helper.Get(baseInfoURL + SeasonID).Result;
            var json = JsonConvert.DeserializeObject<BangumiModel.DetailInfo>(text);
            if (json.code == 0)
            {
                BangumiInfo = json;
                Name = json.result.season_title;
                LastID = json.result.episodes.Last().id;
                LastEp = json.result.episodes.Last();
                LogHelper.Info("拉取番剧信息", $"{Name} 番剧信息拉取成功");
                return true;
            }
            LogHelper.Info("拉取番剧信息", text, false);
            return false;
        }
        public bool FetchEPDetail()
        {
            if (BangumiInfo == null) return false;
            string text = Helper.Get(baseEpURL + SeasonID).Result;
            var json = JsonConvert.DeserializeObject<BangumiModel.Main>(text);
            if(json.code == 0)
            {
                if(json.result.main_section.episodes.Last().id != LastID)
                {
                    LogHelper.Info("更新番剧信息", "新的剧集出现了");
                    LastID = json.result.main_section.episodes.Last().id;
                    FetchInfo();
                    return true;
                }
                return false;
            }
            LogHelper.Info("更新番剧信息", text, false);
            return false;
        }
        public void DownloadPic()
        {
            if (BangumiInfo == null) return;
            _ = Helper.DownloadFile(LastEp.cover, Path.Combine(UpdateChecker.BasePath, "tmp")).Result;
            _ = Helper.DownloadFile(BangumiInfo.result.square_cover, Path.Combine(UpdateChecker.BasePath, "tmp")).Result;
        }
        public string DrawLastEpPic()
        {
            if (BangumiInfo == null || LastEp == null) return string.Empty;

            using Image<Rgba32> main = new(652, 198, Color.White);
            using Image avatar =
                Image.Load(Path.Combine(UpdateChecker.BasePath, "tmp", BangumiInfo.result.square_cover.GetFileNameFromURL()));
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
            TextOptions option = new TextOptions(smallFont);
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
            Info.Mutate(x => x.DrawText($" · {Helper.TimeStamp2DateTime(LastEp.pub_time):G}", smallFont, Rgba32.ParseHex("#99a2aa"), point));
            
            point = new(cover.Width + 10, 10);
            main.Mutate(x => x.DrawImage(Info, (Point)point, 1));

            string path = Path.Combine(UpdateChecker.PicPath, "BiliBiliMonitor", "Bangumi");
            Directory.CreateDirectory(path);
            string filename = $"{LastEp.id}.png";
            main.Save(Path.Combine(path, filename));
            return Path.Combine("BiliBiliMonitor", "Bangumi", filename);
        }
    }
}
