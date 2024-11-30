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
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Dynamics
    {
        private static string emojiStore = "";

        public Dynamics(long uid)
        {
            UID = uid;
            EmojiFont = new FontCollection().Add(Path.Combine(Config.BaseDirectory, "Assets", "seguiemj.ttf"));
            FanNumFont = new FontCollection().Add(Path.Combine(Config.BaseDirectory, "Assets", "fannum.ttf"));
        }

        public static event Action<DynamicModel.Item, long, string> OnDynamicUpdated;

        public List<(string, DateTime)> Cached { get; set; } = new();

        public List<DynamicModel.Item> DynamicList { get; set; } = new();

        public int ErrorCount { get; set; }

        public string LastDynamicID { get; set; }

        public DynamicModel.Item LatestDynamic
        {
            get { return DynamicList.Find(x => x.id_str == LastDynamicID); }
        }

        public bool ReFetchFlag { get; set; } = false;

        public long UID { get; set; }

        public string UserName { get; set; }

        private static string BaseUrl { get; set; } = "https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space?offset=&host_mid={0}";

        private static List<Dynamics> CheckItems { get; set; } = [];

        private static List<Dynamics> DelayAddItems { get; set; } = [];

        private static List<Dynamics> DelayRemoveItems { get; set; } = [];

        private static FontFamily EmojiFont { get; set; }

        private static FontFamily FanNumFont { get; set; }

        private static Timer UpdateCheck { get; set; }

        private static bool Updating { get; set; }

        public static Dynamics AddDynamic(long uid)
        {
            if (CheckItems.Any(x => x.UID == uid))
            {
                return CheckItems.First(x => x.UID == uid);
            }
            var dy = new Dynamics(uid);
            dy.FetchDynamicList();
            if (Updating)
            {
                DelayAddItems.Add(dy);
            }
            else
            {
                CheckItems.Add(dy);
            }
            StartCheckTimer();
            return dy;
        }

        public static Dynamics? GetDynamic(long uid)
        {
            foreach (var item in CheckItems)
            {
                if (item.UID == uid)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取监听的动态列表
        /// </summary>
        /// <returns>UID、用户名</returns>
        public static List<(long, string)> GetDynamicList()
        {
            List<(long, string)> ls = [];
            foreach (var item in CheckItems)
            {
                ls.Add((item.UID, item.UserName));
            }
            return ls;
        }

        public static string GetMaxDynamicID(List<string> id)
        {
            if (id.Count == 0)
            {
                return string.Empty;
            }

            if (id.Count == 1)
            {
                return id[0];
            }

            string max = id[0];
            for (int i = 1; i < id.Count; i++)
            {
                if (Helper.CompareNumString(id[i], max))
                {
                    max = id[i];
                }
            }

            return max;
        }

        public static void RemoveDynamic(long uid)
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

        /// <summary>
        /// 拉取最新动态的图片
        /// </summary>
        /// <returns></returns>
        public bool DownloadPics()
        {
            return DownloadPics(LastDynamicID);
        }

        /// <summary>
        /// 指定动态ID拉取图片
        /// </summary>
        /// <param name="id">动态ID</param>
        /// <returns></returns>
        public bool DownloadPics(string id)
        {
            return DownloadPics(DynamicList.First(x => x.id_str == id));
        }

        /// <summary>
        /// 拉取指定动态的图片
        /// </summary>
        /// <param name="item">动态对象</param>
        /// <returns></returns>
        public bool DownloadPics(DynamicModel.Item item)
        {
            if (item == null)
            {
                return false;
            }

            try
            {
                _ = Helper.DownloadFile(item.modules.module_author.face, Path.Combine(Config.BaseDirectory, "tmp"))
                    .Result;
                _ = Helper.DownloadFile(item.modules.module_author.vip?.avatar_subscript_url,
                    Path.Combine(Config.BaseDirectory, "tmp")).Result;
                _ = Helper.DownloadFile(item.modules.module_author.decorate?.card_url,
                    Path.Combine(Config.BaseDirectory, "tmp")).Result;
                _ = Helper.DownloadFile(item.modules.module_author.pendant?.image,
                    Path.Combine(Config.BaseDirectory, "tmp")).Result;
                if (item.modules.module_dynamic.major?.archive != null)
                {
                    if (!item.modules.module_dynamic.major.archive.cover.EndsWith(".webp"))
                    {
                        item.modules.module_dynamic.major.archive.cover += "@203w_127h_1c.webp";
                    }

                    _ = Helper.DownloadFile(item.modules.module_dynamic.major.archive.cover,
                        Path.Combine(Config.BaseDirectory, "tmp")).Result;
                }
                if (item.modules.module_dynamic.major?.article != null)
                {
                    if (!item.modules.module_dynamic.major.article.covers[0].EndsWith(".webp"))
                    {
                        item.modules.module_dynamic.major.article.covers[0] += "@518w_120h_1c.webp";
                    }

                    _ = Helper.DownloadFile(item.modules.module_dynamic.major.article.covers[0],
                        Path.Combine(Config.BaseDirectory, "tmp")).Result;
                }

                if (item.modules.module_dynamic?.desc?.rich_text_nodes != null)
                {
                    foreach (var i in item.modules.module_dynamic.desc.rich_text_nodes)
                    {
                        _ = Helper.DownloadFile(i.emoji?.icon_url, Path.Combine(Config.BaseDirectory, "tmp")).Result;
                    }
                }

                if (item.modules?.module_interaction?.items != null)
                {
                    foreach (var i in item.modules.module_interaction.items)
                    {
                        foreach(var j in i?.desc?.rich_text_nodes)
                        {
                            _ = Helper.DownloadFile(j.emoji?.icon_url, Path.Combine(Config.BaseDirectory, "tmp")).Result;
                        }
                    }
                }

                int picCount = item.modules.module_dynamic.major?.draw?.items.Length ?? 0;
                if (picCount != 0)
                {
                    foreach (var i in item.modules.module_dynamic.major?.draw?.items)
                    {
                        if (i.src.Contains(".gif"))
                        {
                            continue;
                        }

                        string webp = ".webp";
                        if (i.height / (double)i.width > 3)
                        {
                            webp = picCount == 1 ? "240w_320h_!header" + webp : "104w_104h_!header" + webp;
                        }
                        else
                        {
                            if (picCount == 1)
                            {
                                //if (i.width > i.height)
                                //{
                                //    webp = "320w_180h_1e_1c" + webp;
                                //}
                                //else
                                //{
                                //    webp = "480w_640h_1e_1c" + webp;
                                //}
                            }
                            else
                            {
                                webp = "104w_104h_1e_1c" + webp;
                            }
                        }

                        i.src += "@" + webp;
                        _ = Helper.DownloadFile(i.src, Path.Combine(Config.BaseDirectory, "tmp")).Result;
                    }
                }

                if (item.type == "DYNAMIC_TYPE_FORWARD")
                {
                    DownloadPics(item.orig);
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 绘制最新动态的图片
        /// </summary>
        public string DrawImage()
        {
            return DrawImage(LastDynamicID);
        }

        /// <summary>
        /// 绘制动态ID动态的图片
        /// </summary>
        /// <param name="id">动态ID</param>
        public string DrawImage(string id)
        {
            DynamicModel.Item item = DynamicList.First(x => x.id_str == id);
            return item == null ? string.Empty : DrawImage(item);
        }

        /// <summary>
        /// 绘制指定动态的图片
        /// </summary>
        /// <param name="item">动态对象</param>
        public string DrawImage(DynamicModel.Item item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            int padding = 10;

            using Image<Rgba32> main = new(652, 30000, new Rgba32(244, 245, 247));
            //TODO: 未知内存溢出
            using Image<Rgba32> background = new(632, 30000, Color.White);

            int left = 78;
            //TODO: 提取方法
            //头像
            Size avatarSize = new(48);
            using Image avatar = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                item.modules.module_author.face.GetFileNameFromURL()));
            avatar.Mutate(x => x.Resize(avatarSize));
            using Image<Rgba32> avatarFrame = new(48, 48, new Rgba32(255, 255, 255, 0));
            IPath circle = new EllipsePolygon(avatarFrame.Width / 2, avatarFrame.Height / 2, avatarFrame.Width / 2);
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            background.Mutate(x => x.DrawImage(avatarFrame, new Point(14, 14), 1));

            if (!string.IsNullOrWhiteSpace(item.modules.module_author.pendant.image))
            {
                using Image pendant = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                    item.modules.module_author.pendant.image.GetFileNameFromURL()));
                pendant.Mutate(x => x.Resize(new Size(72, 72)));
                background.Mutate(x => x.DrawImage(pendant, new Point(2, 2), 1));
            }
            //认证

            //标题
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 16);
            Color nameColor = Color.Black;
            if (item.modules.module_author.vip?.status == 1)
            {
                try
                {
                    nameColor = Color.ParseHex(item.modules.module_author.vip.nickname_color);
                }
                catch
                {
                    LogHelper.Info("颜色异常", $"color: {item.modules.module_author.vip.nickname_color}", false);
                    nameColor = Color.Black;
                }
            }

            background.Mutate(x => x.DrawText(item.modules.module_author.name, font, nameColor, new PointF(left, 27)));
            font = SystemFonts.CreateFont("Microsoft YaHei", 12);
            string text =
                $"{Helper.TimeStamp2DateTime(item.modules.module_author.pub_ts):G}{(string.IsNullOrWhiteSpace(item.modules.module_author.pub_action) ? "" : " · ")}{item.modules.module_author.pub_action}";
            background.Mutate(x => x.DrawText(text, font, new Rgba32(153, 162, 170), new PointF(left, 27 + 24)));
            //装扮
            if (item.modules.module_author.decorate != null)
            {
                using Image decorate = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                    item.modules.module_author.decorate?.card_url.GetFileNameFromURL()));
                switch (item.modules.module_author.decorate.type)
                {
                    case 3:
                        decorate.Mutate(x => x.Resize(146, 44));
                        if (item.modules.module_author.decorate?.fan != null)
                        {
                            decorate.Mutate(x => x.DrawText(item.modules.module_author.decorate.fan.num_str,
                                FanNumFont.CreateFont(12),
                                Color.ParseHex(item.modules.module_author.decorate.fan.color), new PointF(48, 17)));
                        }

                        background.Mutate(x => x.DrawImage(decorate,
                            new Point(background.Width - padding - 24 - decorate.Width, 18), 1));
                        break;

                    case 1:
                        decorate.Mutate(x => x.Resize(60, 34));
                        background.Mutate(x => x.DrawImage(decorate,
                            new Point(background.Width - padding - 24 - decorate.Width, 18), 1));
                        break;

                    default:
                        break;
                }
            }

            //文本
            PointF point = new(78, 73);
            background.Mutate(x => RenderRichText(item, x, ref point));
            point = new(78, point.Y + 5);
            switch (item.type)
            {
                case "DYNAMIC_TYPE_DRAW":
                    point = new(78, point.Y + 10);
                    background.Mutate(x => DrawMajorImage(item.modules.module_dynamic.major.draw, x, ref point));
                    break;

                case "DYNAMIC_TYPE_AV":
                    point = new(78, point.Y + 10);
                    background.Mutate(x => DrawVideoElement(item.modules.module_dynamic.major.archive, x, ref point));
                    break;

                case "DYNAMIC_TYPE_FORWARD":
                    point = new(78, point.Y + 10);
                    background.Mutate(x => DrawForward(item.orig, x, ref point));
                    break;

                case "DYNAMIC_TYPE_ARTICLE":
                    point = new(78, point.Y + 10);
                    background.Mutate(x => DrawArticle(item.modules.module_dynamic.major.article, x, ref point));
                    break;
            }

            if (item.modules.module_dynamic.additional != null)
            {
                point = new(78, point.Y + 20);
                background.Mutate(x => DrawAddition(item.modules.module_dynamic.additional, x, ref point));
            }

            background.Mutate(x => DrawInteractive(item.modules.module_interaction, x, ref point));
            background.Mutate(x => DrawStat(item.modules.module_stat, x, ref point));

            background.Mutate(x => x.Crop(background.Width, (int)point.Y + padding));
            main.Mutate(x => x.Crop(main.Width, background.Height + (padding * 2)));
            main.Mutate(x => x.DrawImage(background, new Point(padding, padding), 1));

            string path = Path.Combine(Config.PicSaveBasePath, "BiliBiliMonitor", "Dynamic");
            Directory.CreateDirectory(path);
            string filename = $"{item.id_str}.png";
            main.Save(Path.Combine(path, filename));
            return Path.Combine("BiliBiliMonitor", "Dynamic", filename);
        }

        /// <summary>
        /// 拉取动态列表并比对动态ID来获取最新动态
        /// </summary>
        /// <returns>是否有变化</returns>
        public bool FetchDynamicList()
        {
            if (ReFetchFlag)
            {
                ReFetchFlag = false;
                return true;
            }

            string url = string.Format(BaseUrl, UID);
            string text = Helper.Get(url, CookieManager.GetCurrentCookie()).Result;
            //string text = File.ReadAllText(@"E:\DO\dy.txt");
            DynamicModel.Main json = null;
            try
            {
                json = JsonConvert.DeserializeObject<DynamicModel.Main>(text);
                if (json == null)
                {
                    throw new Exception("json err");
                }
            }
            catch
            {
                if (Config.DebugMode)
                {
                    LogHelper.Info("拉取动态列表异常", $"username={UserName}, json={text}");
                }
                return false;
            }

            if (json.code == 0)
            {
                DynamicList = json.data.items.ToList();
                string max = GetMaxDynamicID(DynamicList.Where(x => x.type != "DYNAMIC_TYPE_LIVE_RCMD")
                    .Select(x => x.id_str).ToList());
                if (DynamicList.Count > 0 && string.IsNullOrEmpty(LastDynamicID))
                {
                    LastDynamicID = max;
                    if (DynamicList.First(x => x.id_str == max).modules.module_author != null)
                    {
                        UserName = DynamicList[0].modules.module_author.name;
                    }
                }
                if (Config.DebugMode)
                {
                    LogHelper.Info("动态检查", $"{UserName}的动态列表拉取成功");
                }
                if (LastDynamicID != max)
                {
                    LastDynamicID = max;
                    if (Cached.Any(item => item.Item1 == max))
                    {
                        return false;
                    }
                    Cached.Add((LastDynamicID, DateTime.Now));
                    return true;
                }
            }
            else
            {
                Debug.WriteLine(json.message);
            }

            return false;
        }

        /// <summary>
        /// 分字符绘制，处理emoji
        /// </summary>
        /// <returns>总字符高度</returns>
        private static float DrawString(IImageProcessingContext img, char c, Color color, ref PointF point,
            TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight,
            float totalHeight = 0)
        {
            string target;
            if (c.JudgeEmoji())
            {
                emojiStore += c;
                return totalHeight;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(emojiStore))
                {
                    target = c.ToString();
                }
                else
                {
                    emojiStore += c;
                    target = emojiStore;
                    option = new(EmojiFont.CreateFont(option.Font.Size));
                }

                emojiStore = "";
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                target = c.ToString();
            }

            return DrawString(img, target, color, ref point, option, padding, charGap, ref maxCharWidth, maxWidth,
                ref charHeight, totalHeight);
        }

        /// <summary>
        /// 文本绘制
        /// </summary>
        /// <returns>总字符高度</returns>
        private static float DrawString(IImageProcessingContext img, string text, Color color, ref PointF point,
            TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight,
            float totalHeight = 0)
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
            var pointClone = new PointF(point.X, point.Y); //在表达式内无法使用ref
            img.DrawText(text, option.Font, color, pointClone);
            totalHeight = WrapTest(maxWidth, padding, charGap, charSize, ref point, totalHeight);
            return totalHeight;
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

            foreach (var item in CheckItems)
            {
                try
                {
                    for (int i = 0; i < item.Cached.Count; i++)
                    {
                        var cache = item.Cached[i];
                        if (cache.Item2.AddDays(1) < DateTime.Now)
                        {
                            item.Cached.Remove(cache);
                            i--;
                        }
                    }
                    if (item.FetchDynamicList())
                    {
                        item.DownloadPics();
                        string pic = item.DrawImage();
                        GC.Collect();
                        item.ReFetchFlag = false;
                        item.ErrorCount = 0;

                        LogHelper.Info("动态更新", $"{item.UserName}的动态有更新，id={item.LastDynamicID}，路径={pic}");
                        OnDynamicUpdated?.Invoke(item.LatestDynamic, item.UID, pic);
                    }
                }
                catch (Exception exc)
                {
                    item.ReFetchFlag = true;
                    LogHelper.Info("动态更新", exc.Message + exc.StackTrace, false);
                    item.ErrorCount++;

                    if (item.ErrorCount >= Config.DynamicRetryCount)
                    {
                        item.ReFetchFlag = false;
                        item.ErrorCount = 1;
                    }
                }
            }
            Updating = false;
        }

        /// <summary>
        /// 换行
        /// </summary>
        private static float WrapTest(int maxWidth, int padding, int charGap, FontRectangle charSize, ref PointF point,
            float totalHeight)
        {
            if (point.X + charSize.Width >= maxWidth)
            {
                point.X = padding;
                point.Y += charSize.Height + 2;
                totalHeight += charSize.Height + 2;
            }
            else
            {
                point.X += charSize.Width + charGap;
            }

            return totalHeight;
        }

        /// <summary>
        /// 绘制附加元素
        /// </summary>
        private IImageProcessingContext DrawAddition(DynamicModel.Additional item, IImageProcessingContext img,
            ref PointF point)
        {
            PointF initalPoint = new(point.X, point.Y);
            if (item == null)
            {
                return img;
            }

            Font font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = 0, chargap = 0, maxWidth = 470;
            float maxCharWidth = 0, charHeight = 0, totalHeight = 0;
            using Image<Rgba32> textImg = new(maxWidth, 100, Rgba32.ParseHex("#FFFFFF00"));
            textImg.Mutate(x =>
            {
                PointF point = new(padding, 0);
                switch (item.type)
                {
                    case "ADDITIONAL_TYPE_RESERVE":
                        foreach (var c in item.reserve.title)
                        {
                            totalHeight = DrawString(x, c, Color.Black, ref point, options, padding, chargap,
                                ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }

                        point = new(padding, point.Y + charHeight);
                        totalHeight += charHeight;
                        font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in item.reserve.desc1.text)
                        {
                            totalHeight = DrawString(x, c, Color.Black, ref point, options, padding, chargap,
                                ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }

                        break;

                    case "ADDITIONAL_TYPE_VOTE":
                        foreach (var c in item.vote.desc)
                        {
                            totalHeight = DrawString(x, c, Color.Black, ref point, options, padding, chargap,
                                ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }

                        point = new(padding, point.Y + charHeight);
                        totalHeight += charHeight;
                        font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in "结束时间：" + Helper.TimeStamp2DateTime(item.vote.end_time))
                        {
                            totalHeight = DrawString(x, c, Color.Black, ref point, options, padding, chargap,
                                ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }

                        break;
                }
            });
            point = new(initalPoint.X, point.Y + 5);
            IPath container = new RectangularPolygon(initalPoint.X, initalPoint.Y + 5, 532, totalHeight + 15);
            img.Fill(new Rgba32(244, 245, 247), container);
            point = new(point.X + 10, point.Y + 10);
            img.DrawImage(textImg, (Point)point, 1);

            point = new(initalPoint.X, initalPoint.Y + totalHeight + 10);
            return img;
        }

        private IImageProcessingContext DrawArticle(DynamicModel.Article item, IImageProcessingContext img, ref PointF point, int startX = 78, int elementWidth = 520)
        {
            Point initialPoint = (Point)point;
            IPath container = new RectangularPolygon(startX, point.Y, elementWidth, 219);
            img.Fill(Color.White, container);
            img.Draw(Pens.Solid(new Rgba32(229, 233, 239), 1), container);
            using var cover = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                item.covers[0].GetFileNameFromURL()));
            img.DrawImage(cover, (Point)point, 1);

            point = new(startX + 16, point.Y + 120 + 5);

            Font font = SystemFonts.CreateFont("Microsoft YaHei", 16, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 0, maxWidth = elementWidth - 28 + startX;
            float maxCharWidth = 0, charHeight = 0;
            if (item.title.Length > 50)
            {
                item.title = item.title.Substring(0, 48) + "..";
            }

            foreach (var c in item.title)
            {
                DrawString(img, c, Color.Black, ref point, options, padding, chargap, ref maxCharWidth, maxWidth,
                    ref charHeight);
            }

            point = new(padding, point.Y + charHeight + 5);

            if (item.desc.Length > 74)
            {
                item.desc = item.desc.Substring(0, 74) + "..";
            }

            if (item.desc.Count(x => x == '\n') >= 2)
            {
                int first = item.desc.IndexOf('\n');
                item.desc = item.desc.Substring(0, item.desc.IndexOf('\n', first + 1)) + "..";
            }

            font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
            options = new(font);
            foreach (var c in item.desc)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap,
                    ref maxCharWidth, maxWidth, ref charHeight);
            }

            point = new Point(startX, initialPoint.Y + 219);
            return img;
        }

        /// <summary>
        /// 绘制转发
        /// </summary>
        private IImageProcessingContext DrawForward(DynamicModel.Item item, IImageProcessingContext img,
            ref PointF point)
        {
            PointF initalPoint = new(point.X, point.Y);
            if (item == null)
            {
                return img;
            }

            using Image<Rgba32> main = new(499, 10000, Color.Transparent);

            PointF p = new(0, 0);
            using var avatar = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                item.modules.module_author.face.GetFileNameFromURL()));
            avatar.Mutate(x => x.Resize(new Size(24, 24)));
            IPath circle = new EllipsePolygon(avatar.Width / 2, avatar.Height / 2, avatar.Width / 2);
            using Image<Rgba32> avatarFrame = new(24, 24, new Rgba32(255, 255, 255, 0));
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            main.Mutate(x => x.DrawImage(avatarFrame, (Point)p, 1));

            p = new(p.X + 24 + 8, p.Y + 3);
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            main.Mutate(x => x.DrawText(item.modules.module_author.name, font, new Rgba32(0, 161, 214), p));
            var charSize = TextMeasurer.Measure(item.modules.module_author.name, new TextOptions(font));
            p = new(p.X + (int)charSize.Width + 8, p.Y);

            main.Mutate(x => x.DrawText(item.modules.module_author.pub_action, font, Color.Black, p));

            p = new(0, p.Y + charSize.Height + 5);

            main.Mutate(x => RenderRichText(item, x, ref p, 0));

            p = new(0, p.Y + 5);
            switch (item.type)
            {
                case "DYNAMIC_TYPE_DRAW":
                    main.Mutate(x => DrawMajorImage(item.modules.module_dynamic.major.draw, x, ref p, 0));
                    break;

                case "DYNAMIC_TYPE_AV":
                    main.Mutate(x => DrawVideoElement(item.modules.module_dynamic.major.archive, x, ref p, 0, 495));
                    break;
            }

            IPath container = new RectangularPolygon(initalPoint.X, initalPoint.Y, 532, p.Y + 20);
            img.Fill(new Rgba32(244, 245, 247), container);
            img.DrawImage(main, (Point)new PointF(initalPoint.X + 10, initalPoint.Y + 10), 1);
            point = new(initalPoint.X, initalPoint.Y + p.Y + 10);
            return img;
        }

        /// <summary>
        /// 绘制热评
        /// </summary>
        private IImageProcessingContext DrawInteractive(DynamicModel.Module_Interaction item,
            IImageProcessingContext img, ref PointF point)
        {
            if (item == null || item.items?.Length == 0)
            {
                return img;
            }

            PointF initalPoint = new(point.X, point.Y);
            point = new(78 + 8, point.Y + 5);

            using var comment = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "comment.png"));
            comment.Mutate(x => x.Resize(16, 16));
            img.DrawImage(comment, (Point)point, 1);
            point = new(point.X + 14 + 8, point.Y);

            string text = "";
            foreach (var i in item.items[0].desc.rich_text_nodes)
            {
                text += i.orig_text;
            }

            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 0, maxWidth = 610 - 12;
            float maxCharWidth = 0, charHeight = 0, totalHeight = 0;
            foreach (var node in item.items[0].desc.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_AT":
                        foreach (var c in node.text)
                        {
                            totalHeight = DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding,
                                chargap, ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }

                        break;

                    case "RICH_TEXT_NODE_TYPE_TEXT":
                        foreach (var c in node.text)
                        {
                            totalHeight = DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding,
                                chargap, ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }

                        break;

                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        var emoji = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                            node.emoji.icon_url.GetFileNameFromURL()));
                        emoji.Mutate(x => x.Resize(new Size(20, 20)));
                        img.DrawImage(emoji, (Point)point, 1); // ? point
                        break;

                    default:
                        break;
                }
            }

            IPath dash = new RectangularPolygon(initalPoint.X, initalPoint.Y + 5, 2, totalHeight);
            img.Fill(Rgba32.ParseHex("#e7e7e7"), dash);

            point = new(initalPoint.X, point.Y + 10);
            return img;
        }

        /// <summary>
        /// 绘制图片元素
        /// </summary>
        private IImageProcessingContext DrawMajorImage(DynamicModel.Draw item, IImageProcessingContext img,
            ref PointF point, int startX = 78)
        {
            PointF initalPoint = new(point.X, point.Y);

            int picCount = (int)(item?.items.Length);
            if (picCount == 1)
            {
                var i = item.items[0];
                if (i.src.Contains(".gif"))
                {
                    return img;
                }

                using Image image = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                    i.src.GetFileNameFromURL()));
                if (image.Width > 500)
                {
                    image.Mutate(x =>
                    {
                        x.Resize(500, (int)(image.Height * (500 / (float)image.Width)));
                    });
                }
                img.DrawImage(image, (Point)point, 1);
                point = new(startX, point.Y + image.Height);
            }
            else
            {
                if (picCount == 4)
                {
                    for (int index = 1; index <= picCount; index++)
                    {
                        if (item.items[index - 1].src.Contains(".gif"))
                        {
                            continue;
                        }

                        using Image tmp = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                            item.items[index - 1].src.GetFileNameFromURL()));
                        img.DrawImage(tmp, (Point)point, 1);
                        point = index % 2 == 0 ? new(startX, point.Y + 108) : new(point.X + 108, point.Y);
                    }
                }
                else
                {
                    for (int index = 1; index <= picCount; index++)
                    {
                        if (item.items[index - 1].src.Contains(".gif"))
                        {
                            continue;
                        }

                        using Image tmp = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                            item.items[index - 1].src.GetFileNameFromURL()));
                        img.DrawImage(tmp, (Point)point, 1);
                        point = index % 3 == 0 && index != picCount ? new(startX, point.Y + 108) : new(point.X + 108, point.Y);
                    }

                    point = new(initalPoint.X, point.Y + 108);
                }
            }

            return img;
        }

        /// <summary>
        /// 绘制动态数据
        /// </summary>
        private IImageProcessingContext DrawStat(DynamicModel.Module_Stat item, IImageProcessingContext img,
            ref PointF point)
        {
            if (item == null)
            {
                return img;
            }

            PointF initalPoint = new(point.X, point.Y);
            point = new(point.X, point.Y + 20);
            using var forward = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "forward.png"));
            forward.Mutate(x => x.Resize(16, 16));
            using var comment = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "comment.png"));
            comment.Mutate(x => x.Resize(16, 16));
            using var like = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "like.png"));
            like.Mutate(x => x.Resize(16, 16));

            img.DrawImage(forward, (Point)point, 1);
            point = new(point.X + 16 + 4, point.Y);
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 0, maxWidth = 610 - 12;
            float maxCharWidth = 0, charHeight = 0;
            foreach (var c in item.forward.count.ParseNum2Chinese())
            {
                DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth,
                    maxWidth, ref charHeight);
            }

            point = new(point.X + 20, point.Y);

            img.DrawImage(comment, (Point)point, 1);
            point = new(point.X + 16 + 4, point.Y);
            foreach (var c in item.comment.count.ParseNum2Chinese())
            {
                DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth,
                    maxWidth, ref charHeight);
            }

            point = new(point.X + 20, point.Y);

            img.DrawImage(like, (Point)point, 1);
            point = new(point.X + 16 + 4, point.Y);
            foreach (var c in item.like.count.ParseNum2Chinese())
            {
                DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth,
                    maxWidth, ref charHeight);
            }

            point = new(initalPoint.X, point.Y + 16);
            return img;
        }

        /// <summary>
        /// 绘制视频元素
        /// </summary>
        private IImageProcessingContext DrawVideoElement(DynamicModel.Archive item, IImageProcessingContext img,
            ref PointF point, int startX = 78, int elementWidth = 532)
        {
            Point initialPoint = (Point)point;
            IPath container = new RectangularPolygon(startX, point.Y, elementWidth, 127);
            img.Fill(Color.White, container);
            img.Draw(Pens.Solid(new Rgba32(229, 233, 239), 1), container);
            using var cover = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                item.cover.GetFileNameFromURL()));
            img.DrawImage(cover, (Point)point, 1);
            container = new RectangularPolygon(startX + 137, point.Y + 8, 58, 18);
            img.Fill(Rgba32.ParseHex(item.badge.bg_color), container);
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Bold);
            img.DrawText(item.badge.text, font, Rgba32.ParseHex(item.badge.color),
                (Point)new PointF(startX + 137 + 5, point.Y + 8));
            point = new(startX + 203 + 16, point.Y + 9);

            font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 0, maxWidth = elementWidth - 28 + startX;
            float maxCharWidth = 0, charHeight = 0;
            if (item.title.Length > 42)
            {
                item.title = item.title.Substring(0, 40) + "..";
            }

            foreach (var c in item.title)
            {
                DrawString(img, c, Color.Black, ref point, options, padding, chargap, ref maxCharWidth, maxWidth,
                    ref charHeight);
            }

            point = new(padding, point.Y + charHeight + 5);
            if (item.desc.Length > 44)
            {
                item.desc = item.desc.Substring(0, 44) + "..";
            }

            if (item.desc.Count(x => x == '\n') >= 2)
            {
                int first = item.desc.IndexOf('\n');
                item.desc = item.desc.Substring(0, item.desc.IndexOf('\n', first + 1)) + "..";
            }

            font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            options = new(font);
            foreach (var c in item.desc)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap,
                    ref maxCharWidth, maxWidth, ref charHeight);
            }

            // stat
            point = new(padding, initialPoint.Y + 109);
            using var play = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "play.png"));
            play.Mutate(x => x.Resize(14, 14));
            img.DrawImage(play, (Point)point, 1);
            point = new(point.X + 16, initialPoint.Y + 107);
            foreach (var c in item.stat.play)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap,
                    ref maxCharWidth, maxWidth, ref charHeight);
            }

            point = new(point.X + 16, point.Y + 2);
            using var danmaku = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "danmaku.png"));
            danmaku.Mutate(x => x.Resize(14, 14));
            img.DrawImage(danmaku, (Point)point, 1);
            point = new(point.X + 16, initialPoint.Y + 107);
            foreach (var c in item.stat.danmaku)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap,
                    ref maxCharWidth, maxWidth, ref charHeight);
            }

            point = new Point(startX, initialPoint.Y + 127);
            return img;
        }

        /// <summary>
        /// 绘制文本
        /// </summary>
        private IImageProcessingContext RenderRichText(DynamicModel.Item item, IImageProcessingContext img,
            ref PointF point, int padding = 78)
        {
            PointF initalPoint = new(point.X, point.Y);

            if (item == null)
            {
                return img;
            }

            Font font;
            TextOptions options;
            int chargap = 0, maxWidth = 459 + padding;
            float maxCharWidth = 0, charHeight = 0;
            if (item.modules.module_dynamic.topic != null)
            {
                using var topic = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "topic.png"));
                topic.Mutate(x => x.Resize(18, 18));
                img.DrawImage(topic, (Point)point, 1);
                point = new(point.X + 18, point.Y);
                font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                img.DrawText(item.modules.module_dynamic.topic.name, font, new Rgba32(0, 138, 197), point);
                point = new(initalPoint.X, point.Y + 20);
            }

            if (item.modules.module_dynamic.desc == null)
            {
                return img;
            }

            foreach (var node in item.modules.module_dynamic.desc?.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_TEXT":
                        font = SystemFonts.CreateFont("Microsoft YaHei Light", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in node.text)
                        {
                            DrawString(img, c, Color.Black, ref point, options, padding, chargap, ref maxCharWidth,
                                maxWidth, ref charHeight);
                        }

                        break;

                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        var emoji = Image.Load(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"),
                            node.emoji.icon_url.GetFileNameFromURL()));
                        emoji.Mutate(x => x.Resize(new Size(20, 20)));
                        img.DrawImage(emoji, (Point)point, 1);
                        point = new(point.X + 20, point.Y);
                        break;

                    case "RICH_TEXT_NODE_TYPE_LOTTERY":
                        using (Image gift = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "gift.png")))
                        {
                            gift.Mutate(x => x.Resize(18, 18));
                            img.DrawImage(gift, (Point)point, 1);
                            point = new(point.X + 22, point.Y);
                        }

                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in "互动抽奖")
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding,
                                chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }

                        break;

                    case "RICH_TEXT_NODE_TYPE_WEB":
                        using (Image url = Image.Load(Path.Combine(Config.BaseDirectory, "Assets", "url.png")))
                        {
                            url.Mutate(x => x.Resize(18, 18));
                            img.DrawImage(url, (Point)point, 1);
                            point = new(point.X + 22, point.Y);
                        }

                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in "跳转网址")
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding,
                                chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }

                        break;

                    case "RICH_TEXT_NODE_TYPE_TOPIC":
                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in node.text)
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding,
                                chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }

                        break;

                    case "RICH_TEXT_NODE_TYPE_AT":
                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in node.text)
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding,
                                chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }

                        break;

                    default:
                        break;
                }
            }

            point = new(initalPoint.X, point.Y + 20);
            return img;
        }
    }
}