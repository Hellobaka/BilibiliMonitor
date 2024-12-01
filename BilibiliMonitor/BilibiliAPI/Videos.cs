using BilibiliMonitor.Models;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Videos
    {
        private const string BaseUserInfoURL = "http://api.bilibili.com/x/web-interface/card?mid={0}";

        private const string BaseVideoTagURL = "https://api.bilibili.com/x/tag/archive/tags?bvid={0}";

        private const string BaseVideoURL = "http://api.bilibili.com/x/web-interface/view?{0}";

        private static Dictionary<string, string> ShortURLCache { get; set; } = [];

        public static string DrawVideoPic(string id)
        {
            var video = GetVideoInfo(id);
            if (video == null)
            {
                return string.Empty;
            }

            var tag = GetVideoTag(video.bvid);
            if (tag == null)
            {
                return string.Empty;
            }

            var user = GetUserInfo(video.owner.mid);
            if (user == null)
            {
                return string.Empty;
            }

            DownloadPics(video);

            int width = 720;
            int height = 1980;
            int avatarWidth = 48;
            int smallIconWidth = 16;
            int largeIconWidth = 48;
            float smallFontSize = 14;
            float middleFontSize = 18;
            float largeFontSize = 20;
            SKColor gray = SKColor.Parse("#6d757a");
            SKColor nameColor = string.IsNullOrEmpty(user.card.vip.nickname_color) ? SKColors.Black : SKColor.Parse(user.card.vip.nickname_color);

            using Painting main = new(width, height);
            string avatarPath = Path.Combine(Config.BaseDirectory, "tmp", video.owner.face.GetFileNameFromURL());
            string coverPath = Path.Combine(Config.BaseDirectory, "tmp", video.pic.GetFileNameFromURL());
            using var avatar = main.LoadImage(avatarPath);
            using var cover = main.LoadImage(coverPath);

            float coverResizeHeight = cover.Height / (cover.Width / main.Width);
            main.DrawImage(cover, new SKRect { Right = main.Width, Bottom = coverResizeHeight });
            main.DrawImage(main.CreateCircularImage(avatar, avatarWidth), new SKRect { Left = 10, Top = coverResizeHeight + 12, Size = new() { Width = avatarWidth, Height = avatarWidth } });

            var textArea = new SKRect() { Left = 10, Right = main.Width - 10 };
            var textP = main.DrawRelativeText(video.owner.name, textArea, new SKPoint { X = avatarWidth + 5, Y = coverResizeHeight + 12 }, nameColor, middleFontSize);
            textP = main.DrawRelativeText($"{user.card.fans.ParseNum2Chinese()}粉丝 {user.archive_count.ParseNum2Chinese()}个投稿", textArea, new SKPoint { X = avatarWidth + 5, Y = textP.Y + 7 }, gray, smallFontSize);
            textP = main.DrawRelativeText(video.title, textArea, new SKPoint { Y = textP.Y + 20 }, SKColors.Black, largeFontSize);

            using var play = main.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "play.png"));
            using var danmaku = main.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "danmaku.png"));
            using var like = main.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "like_video.png"));
            using var coin = main.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "coin_video.png"));
            using var favorite = main.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "fav_video.png"));
            using var forward = main.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "forward_video.png"));

            float baseY = textP.Y + 10;
            main.DrawImage(play, new SKRect { Top = baseY, Left = 10, Size = new() { Width = smallIconWidth, Height = smallIconWidth } });
            textP = main.DrawRelativeText(Helper.ParseLongNumber(video.stat.view), textArea, new SKPoint { X = smallIconWidth + 2, Y = textP.Y + 10 }, gray, smallFontSize);
            main.DrawImage(danmaku, new SKRect { Top = baseY, Left = textP.X + 10, Size = new() { Width = smallIconWidth, Height = smallIconWidth } });
            textP = main.DrawRelativeText(Helper.ParseLongNumber(video.stat.danmaku), textArea, new SKPoint { X = textP.X + smallIconWidth + 2, Y = baseY }, gray, smallFontSize);
            textP = main.DrawRelativeText(Helper.TimeStamp2DateTime(video.pubdate).ToString("G"), textArea, new SKPoint { X = textP.X + 10, Y = baseY }, gray, smallFontSize);

            baseY = textP.Y + 10;
            textP = main.DrawRelativeText(video.bvid, textArea, new SKPoint { Y = baseY }, gray, smallFontSize);
            textP = main.DrawRelativeText($"AV{video.aid}", textArea, new SKPoint { X = textP.X + 10, Y = baseY }, gray, smallFontSize);

            textP = main.DrawRelativeText(video.desc, textArea, new SKPoint { Y = textP.Y + 10 }, gray, smallFontSize);

            baseY = textP.Y + 20;
            textP = new(0, baseY);
            SKSize size = new();
            foreach (var item in tag)
            {
                size = main.MeasureString(item.tag_name, smallFontSize);
                if (textP.X + size.Width + 14 > main.Width - 10)
                {
                    baseY += size.Height + 18;
                    textP = new(0, baseY);
                }
                main.DrawRectangle(new SKRect { Left = textP.X + 10, Top = baseY, Size = new() { Width = size.Width + 14, Height = size.Height + 8 } }, SKColor.Parse("#F6F7F8"), SKColors.Black, 0);
                textP = main.DrawRelativeText(item.tag_name, textArea, new SKPoint { X = textP.X + 7, Y = baseY + 4 }, gray, smallFontSize);
                textP = new(textP.X + 7 + 10, baseY);
            }

            // 左右Padding80，间隔 = ((Width-padding*2) - (largeIconWidth * 4)) / 3
            baseY = textP.Y + 40;
            textP = new(0, baseY);
            float padding = ((main.Width - 80 * 2) - (largeIconWidth * 4)) / 3;

            var imgPoint = new SKPoint(80, baseY);
            main.DrawImage(like, new SKRect { Top = imgPoint.Y, Left = imgPoint.X, Size = new SKSize { Height = largeIconWidth, Width = largeIconWidth } });
            string text = Helper.ParseLongNumber(video.stat.like);
            size = main.MeasureString(text, smallFontSize);
            textP = main.DrawRelativeText(text, textArea, new SKPoint { X = imgPoint.X + (largeIconWidth / 2) - (size.Width / 2) - 10, Y = imgPoint.Y + largeIconWidth + 10 }, gray, smallFontSize);

            imgPoint = new(imgPoint.X + padding + largeIconWidth, imgPoint.Y);
            main.DrawImage(coin, new SKRect { Top = imgPoint.Y, Left = imgPoint.X, Size = new SKSize { Height = largeIconWidth, Width = largeIconWidth } });
            text = Helper.ParseLongNumber(video.stat.coin);
            size = main.MeasureString(text, smallFontSize);
            textP = main.DrawRelativeText(text, textArea, new SKPoint { X = imgPoint.X + (largeIconWidth / 2) - (size.Width / 2) - 10, Y = imgPoint.Y + largeIconWidth + 10 }, gray, smallFontSize);

            imgPoint = new(imgPoint.X + padding + largeIconWidth, imgPoint.Y);
            main.DrawImage(favorite, new SKRect { Top = imgPoint.Y, Left = imgPoint.X, Size = new SKSize { Height = largeIconWidth, Width = largeIconWidth } });
            text = Helper.ParseLongNumber(video.stat.favorite);
            size = main.MeasureString(text, smallFontSize);
            textP = main.DrawRelativeText(text, textArea, new SKPoint { X = imgPoint.X + (largeIconWidth / 2) - (size.Width / 2) - 10, Y = imgPoint.Y + largeIconWidth + 10 }, gray, smallFontSize);

            imgPoint = new(imgPoint.X + padding + largeIconWidth, imgPoint.Y);
            main.DrawImage(forward, new SKRect { Top = imgPoint.Y, Left = imgPoint.X, Size = new SKSize { Height = largeIconWidth, Width = largeIconWidth } });
            text = Helper.ParseLongNumber(video.stat.reply);
            size = main.MeasureString(text, smallFontSize);
            textP = main.DrawRelativeText(text, textArea, new SKPoint { X = imgPoint.X + (largeIconWidth / 2) - (size.Width / 2) - 10, Y = imgPoint.Y + largeIconWidth + 10 }, gray, smallFontSize);

            main.Resize((int)main.Width, (int)textP.Y + 20);

            string path = Path.Combine(Config.PicSaveBasePath, "BiliBiliMonitor", "Video");
            Directory.CreateDirectory(path);
            string filename = $"{video.aid}.png";
            main.Save(Path.Combine(path, filename));
            GC.Collect();

            return Path.Combine("BiliBiliMonitor", "Video", filename);
        }

        public static string ParseURL(string url)
        {
            if (url.StartsWith("bv") || url.StartsWith("BV") || url.StartsWith("av") || url.StartsWith("AV"))
            {
                return url;
            }

            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            url = url.Trim();
            //LogHelper.Info("视频解析", url);
            if (url.Contains("b23.tv"))
            {
                url = url.Replace("\\", "");
                var match = Regex.Match(url, "b23\\.tv/.*");
                if (match.Success)
                {
                    url = match.Groups[match.Groups.Count - 1].Value;
                }
                url = url.Replace(" ", "").Replace("\r", "").Replace("\n", "");
                url = url.Split('?').First();

                if (url.StartsWith("http") is false)
                {
                    url = "https://" + url;
                }

                if (ShortURLCache.ContainsKey(url))
                {
                    return ShortURLCache[url];
                }

                using var http = new HttpClient();
                var r = http.GetAsync(url);
                r.Wait();
                string bvid = r.Result.RequestMessage.RequestUri.AbsoluteUri;
                if (!ShortURLCache.ContainsKey(url))
                {
                    ShortURLCache.Add(url, bvid);
                }

                url = bvid;
            }
            //LogHelper.Info("视频解析", url);
            if (url.Contains("bilibili.com/video"))
            {
                string vid = url.Split('/').First(x => x.ToLower().StartsWith("av") || x.StartsWith("BV"));
                if (vid.StartsWith("av"))
                {
                    vid = vid.Substring(2);
                }
                vid = vid.Split('?').First();
                return vid;
            }
            else
            {
                //LogHelper.Info("视频解析", "网址格式无法解析", false);
                return string.Empty;
            }
        }

        public static string ParseURLFromXML(string xml)
        {
            var match = Regex.Match(xml, "(b23\\.tv.*?)\"");
            if (match.Success)
            {
                string res = ParseURL(match.Groups[1].Value);
                if (string.IsNullOrEmpty(res))
                {
                    return string.Empty;
                }

                if (!ShortURLCache.ContainsKey(match.Groups[1].Value))
                {
                    ShortURLCache.Add(match.Groups[1].Value, res);
                }

                return res;
            }
            return string.Empty;
        }

        private static void DownloadPics(VideoModel.Data data)
        {
            if (data == null)
            {
                return;
            }

            _ = Helper.DownloadFile(data.pic, Path.Combine(Config.BaseDirectory, "tmp")).Result;
            _ = Helper.DownloadFile(data.owner.face, Path.Combine(Config.BaseDirectory, "tmp")).Result;
        }

        private static UserInfoModel.Data GetUserInfo(long mid)
        {
            string url = string.Format(BaseUserInfoURL, mid);
            var json = JsonConvert.DeserializeObject<UserInfoModel.Main>(Helper.Get(url).Result);
            // var json = JsonConvert.DeserializeObject<UserInfoModel.Main>(File.ReadAllText(@"E:\DO\e.json"));
            if (json.code == 0)
            {
                return json.data;
            }
            else
            {
                Debug.WriteLine(json.message);
            }
            return null;
        }

        private static VideoModel.Data GetVideoInfo(string bvId)
        {
            string url = string.Format(BaseVideoURL, "bvid=" + bvId);
            if (long.TryParse(bvId, out long aid))
            {
                url = string.Format(BaseVideoURL, "aid=" + aid);
            }
            var json = JsonConvert.DeserializeObject<VideoModel.Main>(Helper.Get(url).Result);
            // var json = JsonConvert.DeserializeObject<VideoModel.Main>(File.ReadAllText(@"E:\DO\video.json"));
            if (json.code == 0)
            {
                return json.data;
            }
            else
            {
                Debug.WriteLine(json.message);
            }
            return null;
        }

        private static VideoTagModel.Datum[] GetVideoTag(string bvid)
        {
            string url = string.Format(BaseVideoTagURL, bvid);
            var json = JsonConvert.DeserializeObject<VideoTagModel.Main>(Helper.Get(url).Result);
            // var json = JsonConvert.DeserializeObject<VideoTagModel.Main>(File.ReadAllText(@"E:\DO\tag.json"));
            if (json.code == 0)
            {
                return json.data;
            }
            else
            {
                Debug.WriteLine(json.message);
            }
            return null;
        }
    }
}