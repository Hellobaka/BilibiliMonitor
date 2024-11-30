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
using System.Net.Http;
using System.Text.RegularExpressions;
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Videos
    {
        private static string BaseUserInfoURL = "http://api.bilibili.com/x/web-interface/card?mid={0}";

        private static string BaseVideoTagURL = "https://api.bilibili.com/x/tag/archive/tags?bvid={0}";

        private static string BaseVideoURL = "http://api.bilibili.com/x/web-interface/view?{0}";

        private static Dictionary<string, string> shortURLCache { get; set; } = new();

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
            using Image<Rgba32> main = new(652, 1980, Color.White);
            using Image cover =
                Image.Load(Path.Combine(Config.BaseDirectory, "tmp", video.pic.GetFileNameFromURL()));
            using Image avatar =
                Image.Load(Path.Combine(Config.BaseDirectory, "tmp", video.owner.face.GetFileNameFromURL()));
            int height = (int)(cover.Height / (cover.Width / 652.0));
            cover.Mutate(x => x.Resize(652, height));
            main.Mutate(x => x.DrawImage(cover, 1));
            Color gray = Rgba32.ParseHex("#6d757a");
            PointF point = new(10, height + 12);

            using Image<Rgba32> avatarFrame = new(48, 48, new Rgba32(255, 255, 255, 0));
            avatar.Mutate(x => x.Resize(48, 48));
            IPath circle = new EllipsePolygon(avatarFrame.Width / 2, avatarFrame.Height / 2, avatarFrame.Width / 2);
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            main.Mutate(x => x.DrawImage(avatarFrame, (Point)point, 1));

            Font smallFont = SystemFonts.CreateFont("Microsoft YaHei", 14);
            Font midFont = SystemFonts.CreateFont("Microsoft YaHei", 18);
            Font bigFont = SystemFonts.CreateFont("Microsoft YaHei", 20);
            TextOptions option = new(smallFont);
            point = new(10 + 48 + 5, height + 12);
            var size = TextMeasurer.Measure(video.owner.name, option);
            Color nameColor = string.IsNullOrEmpty(user.card.vip.nickname_color) ? Color.Black : Rgba32.ParseHex(user.card.vip.nickname_color);
            main.Mutate(x => x.DrawText(video.owner.name, midFont, nameColor, point));
            point = new(point.X, point.Y + size.Height + 7);
            main.Mutate(x => x.DrawText($"{user.card.fans.ParseNum2Chinese()}粉丝 {user.archive_count.ParseNum2Chinese()}个投稿", smallFont, gray, point));
            point = new(10, point.Y + 30);

            option = new TextOptions(bigFont)
            {
                TextAlignment = TextAlignment.Start,
                VerticalAlignment = VerticalAlignment.Center,
                WrappingLength = main.Width - 10,
                Origin = point
            };
            size = TextMeasurer.Measure(video.title, option);
            int padding = (int)point.X, chargap = 1, maxWidth = 632;
            float maxCharWidth = 0, charHeight = 0;
            main.Mutate(x =>
            {
                foreach (var c in video.title)
                {
                    DrawString(x, c, Color.Black, ref point, option, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                }
            });
            // main.Mutate(x => x.DrawText(video.title, bigFont, Color.Black, point));
            point = new PointF(10, point.Y + 9 + charHeight);

            using Image play = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "play.png"));
            play.Mutate(x => x.Resize(16, 16));
            using Image danmaku = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "danmaku.png"));
            danmaku.Mutate(x => x.Resize(16, 16));
            using Image like = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "like_video.png"));
            like.Mutate(x => x.Resize(48, 48));
            using Image coin = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "coin_video.png"));
            coin.Mutate(x => x.Resize(48, 48));
            using Image favorite = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "fav_video.png"));
            favorite.Mutate(x => x.Resize(48, 48));
            using Image forward = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "forward_video.png"));
            forward.Mutate(x => x.Resize(48, 48));

            main.Mutate(x => x.DrawImage(play, (Point)point, 1));
            point = new PointF(point.X + play.Width + 2, point.Y);
            string playNum = Helper.ParseLongNumber(video.stat.view);
            option = new TextOptions(smallFont);
            size = TextMeasurer.Measure(playNum, option);
            main.Mutate(x => x.DrawText(playNum, smallFont, gray, point));
            point = new(point.X + 10 + size.Width, point.Y);
            main.Mutate(x => x.DrawImage(danmaku, (Point)point, 1));
            point = new(point.X + danmaku.Width + 2, point.Y);
            string danmakuNum = Helper.ParseLongNumber(video.stat.danmaku);
            size = TextMeasurer.Measure(danmakuNum, option);
            main.Mutate(x => x.DrawText(danmakuNum, smallFont, gray, point));
            point = new PointF(point.X + size.Width + 10, point.Y);
            main.Mutate(x => x.DrawText(Helper.TimeStamp2DateTime(video.pubdate).ToString("G"), smallFont, gray, point));

            point = new(10, point.Y + 16 + 10);
            size = TextMeasurer.Measure(video.bvid, option);
            main.Mutate(x => x.DrawText(video.bvid, smallFont, gray, point));
            point = new(point.X + size.Width + 10, point.Y);
            main.Mutate(x => x.DrawText($"AV{video.aid}", smallFont, gray, point));

            point = new(10, point.Y + 30);
            padding = (int)point.X;
            chargap = 1;
            maxWidth = 632;
            maxCharWidth = 0;
            charHeight = 0;
            main.Mutate(x =>
            {
                foreach (var c in video.desc)
                {
                    DrawString(x, c, Rgba32.ParseHex("#6d757a"), ref point, option, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                }
            });
            point = new(10, point.Y + charHeight + 20);

            foreach (var item in tag)
            {
                size = TextMeasurer.Measure(item.tag_name, option);
                if (point.X + size.Width + 14 > maxWidth)
                {
                    point = new(10, point.Y + size.Height + 8 + 10);
                }
                IPath container = new RectangularPolygon(point.X, point.Y, size.Width + 14, size.Height + 8);
                main.Mutate(x => x.Fill(Color.ParseHex("#F6F7F8"), container));
                point = new(point.X + 7, point.Y + 4);
                main.Mutate(x => x.DrawText(item.tag_name, smallFont, gray, point));
                point = new(point.X + container.Bounds.Width + 10, point.Y - 4);
            }
            point = new(10, point.Y + 40);

            PointF imgPoint = new(point.X + 20, point.Y + 20);
            main.Mutate(x => x.DrawImage(like, (Point)imgPoint, 1));
            string text = Helper.ParseLongNumber(video.stat.like);
            size = TextMeasurer.Measure(text, option);
            main.Mutate(x => x.DrawText(text, smallFont, gray, (Point)new PointF(imgPoint.X + 24 - (size.Width / 2), point.Y + 78)));
            imgPoint = new(imgPoint.X + 48 + 20 + 110, imgPoint.Y);
            main.Mutate(x => x.DrawImage(coin, (Point)imgPoint, 1));
            text = Helper.ParseLongNumber(video.stat.coin);
            size = TextMeasurer.Measure(text, option);
            main.Mutate(x => x.DrawText(text, smallFont, gray, (Point)new PointF(imgPoint.X + 24 - (size.Width / 2), point.Y + 78)));
            imgPoint = new(imgPoint.X + 48 + 20 + 110, imgPoint.Y);
            main.Mutate(x => x.DrawImage(favorite, (Point)imgPoint, 1));
            text = Helper.ParseLongNumber(video.stat.favorite);
            size = TextMeasurer.Measure(text, option);
            main.Mutate(x => x.DrawText(text, smallFont, gray, (Point)new PointF(imgPoint.X + 24 - (size.Width / 2), point.Y + 78)));
            imgPoint = new(imgPoint.X + 48 + 20 + 110, imgPoint.Y);
            main.Mutate(x => x.DrawImage(forward, (Point)imgPoint, 1));
            text = Helper.ParseLongNumber(video.stat.reply);
            size = TextMeasurer.Measure(text, option);
            main.Mutate(x => x.DrawText(text, smallFont, gray, (Point)new PointF(imgPoint.X + 24 - (size.Width / 2), point.Y + 78)));

            point = new(10, imgPoint.Y + 48 + 20 + 20);
            main.Mutate(x => x.Crop(652, (int)point.Y));

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

                if (shortURLCache.ContainsKey(url))
                {
                    return shortURLCache[url];
                }

                using var http = new HttpClient();
                var r = http.GetAsync(url);
                r.Wait();
                string bvid = r.Result.RequestMessage.RequestUri.AbsoluteUri;
                if (!shortURLCache.ContainsKey(url))
                {
                    shortURLCache.Add(url, bvid);
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

                if (!shortURLCache.ContainsKey(match.Groups[1].Value))
                {
                    shortURLCache.Add(match.Groups[1].Value, res);
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

        /// <summary>
        /// 分字符绘制，处理emoji
        /// </summary>
        /// <returns>总字符高度</returns>
        private static float DrawString(IImageProcessingContext img, char c, Color color, ref PointF point, TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight, float totalHeight = 0)
        {
            return DrawString(img, c.ToString(), color, ref point, option, padding, charGap, ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
        }

        /// <summary>
        /// 文本绘制
        /// </summary>
        /// <returns>总字符高度</returns>
        private static float DrawString(IImageProcessingContext img, string text, Color color, ref PointF point, TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight, float totalHeight = 0)
        {
            if (string.IsNullOrEmpty(text))
            {
                return totalHeight;
            }

            FontRectangle charSize = new();
            try
            {
                charSize = TextMeasurer.Measure(text, option);
            }
            catch
            {
                return totalHeight;
            }
            charHeight = Math.Max(charSize.Height, charHeight);
            if (totalHeight == 0)
            {
                totalHeight = charHeight;
            }

            if (text == "\n")
            {
                point.X = padding;
                point.Y += charHeight + 2;
                totalHeight += charHeight + 2;
                return totalHeight;
            }
            maxCharWidth = Math.Max(maxCharWidth, charSize.Width);
            var pointClone = new PointF(point.X, point.Y);//在表达式内无法使用ref
            img.DrawText(text.ToString(), option.Font, color, pointClone);
            totalHeight = WrapTest(maxWidth, padding, charGap, charSize, ref point, totalHeight);
            return totalHeight;
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

        /// <summary>
        /// 换行
        /// </summary>
        private static float WrapTest(int maxWidth, int padding, int charGap, FontRectangle charSize, ref PointF point, float totalHeight)
        {
            if (point.X + charSize.Width >= maxWidth)
            {
                point.X = padding;
                point.Y += charSize.Height + 2;
                totalHeight += charSize.Height + 2;
            }
            else
            {
                point.X += charSize.Width;
            }
            return totalHeight;
        }
    }
}