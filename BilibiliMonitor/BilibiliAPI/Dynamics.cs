using BilibiliMonitor.Models;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Dynamics
    {
        private const string BaseUrl = "https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space?offset=&host_mid={0}";

        public Dynamics(long uid)
        {
            UID = uid;
        }

        public static event Action<DynamicModel.Item, long, string> OnDynamicUpdated;

        public List<(string, DateTime)> Cached { get; set; } = [];

        public List<DynamicModel.Item> DynamicList { get; set; } = [];

        public string LastDynamicID { get; set; }

        public long UID { get; set; }

        public string UserName { get; set; }

        private static List<Dynamics> CheckItems { get; set; } = [];

        private static List<Dynamics> DelayAddItems { get; set; } = [];

        private static List<Dynamics> DelayRemoveItems { get; set; } = [];

        private static Timer UpdateCheck { get; set; }

        private static bool Updating { get; set; }

        private int CanvasMinWidth { get; set; } = 720;

        private int CanvasWidth { get; set; } = 720;

        private int ErrorCount { get; set; }

        private DynamicModel.Item LatestDynamic => DynamicList.FirstOrDefault(x => x.id_str == LastDynamicID);

        private bool ReFetchFlag { get; set; } = false;

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
                    _ = Helper.DownloadFile(item.modules.module_dynamic.major.archive.cover,
                        Path.Combine(Config.BaseDirectory, "tmp")).Result;
                }
                if (item.modules.module_dynamic.major?.article != null)
                {
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
                        foreach (var j in i?.desc?.rich_text_nodes)
                        {
                            _ = Helper.DownloadFile(j.emoji?.icon_url, Path.Combine(Config.BaseDirectory, "tmp")).Result;
                        }
                    }
                }

                int picCount = item.modules.module_dynamic.major?.draw?.items.Length ?? 0;
                int minWidth = int.MaxValue;
                int minHeight = int.MaxValue;
                foreach (var i in item.modules.module_dynamic.major?.draw?.items ?? [])
                {
                    // 2图时，每图至少480px
                    // 3图时，每图至少360px
                    // 最小图片依照最大图片的尺寸, 缩放至原本大小, 后放置在容器中心
                    // 1图时, 图片最大720px, 容器随图片尺寸变化, 有最小宽度限制
                    _ = Helper.DownloadFile(i.src, Path.Combine(Config.BaseDirectory, "tmp")).Result;
                    minWidth = Math.Min(i.width, minWidth);
                    minHeight = Math.Min(i.height, minHeight);
                }
                if (picCount <= 0)
                {
                    CanvasWidth = CanvasMinWidth;
                }
                else if (picCount == 1)
                {
                    CanvasWidth = Math.Max(720 + 10 * 4, CanvasMinWidth);
                }
                else if (picCount == 2)
                {
                    CanvasWidth = Math.Max(480 * 2 + 10 + 10 * 4, CanvasMinWidth);
                }
                else
                {
                    // 图片尺寸 + gap + padding
                    CanvasWidth = Math.Max(360 * 3 + 10 * 2 + 10 * 4, CanvasMinWidth);
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
            int avatarSize = 48;
            int pendantSize = 72;
            Painting main = new Painting(CanvasWidth, 6000);

            // 头像
            string avatarPath = Path.Combine(Config.BaseDirectory, "tmp", item.modules.module_author.face.GetFileNameFromURL());
            using var avatar = main.LoadImage(avatarPath);
            main.DrawImage(main.CreateCircularImage(avatar, avatarSize), new SKRect { Left = 14, Top = 14, Size = new() { Width = avatarSize, Height = avatarSize } });

            // 头像框
            if (!string.IsNullOrWhiteSpace(item.modules.module_author.pendant.image))
            {
                using var pendant = main.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), item.modules.module_author.pendant.image.GetFileNameFromURL()));
                main.DrawImage(pendant, new SKRect { Left = 2, Top = 2, Size = new() { Width = pendantSize, Height = pendantSize } });
            }

            // 用户名
            SKColor nameColor = SKColors.Black;
            if (item.modules.module_author.vip?.status == 1)
            {
                try
                {
                    nameColor = SKColor.Parse(item.modules.module_author.vip.nickname_color);
                }
                catch
                {
                    LogHelper.Info("颜色异常", $"color: {item.modules.module_author.vip.nickname_color}", false);
                }
            }
            var textP = main.DrawText(item.modules.module_author.name, Painting.Anywhere, new SKPoint() { X = 14 + avatarSize + 10, Y = 20 }, nameColor, 16);
            string text = $"{Helper.TimeStamp2DateTime(item.modules.module_author.pub_ts):G}{(string.IsNullOrWhiteSpace(item.modules.module_author.pub_action) ? "" : " · ")}{item.modules.module_author.pub_action}";
            textP = main.DrawText(text, Painting.Anywhere, new SKPoint() { X = 14 + avatarSize + 10, Y = textP.Y + 7 }, new SKColor(153, 162, 170), 14);

            //装扮
            if (item.modules.module_author.decorate != null)
            {
                using var decorate = main.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), item.modules.module_author.decorate?.card_url.GetFileNameFromURL()));
                switch (item.modules.module_author.decorate.type)
                {
                    case 3:
                        main.DrawImage(decorate, new() { Left = main.Width - 10 - 146, Top = 18, Size = new() { Width = 146, Height = 44 } });
                        if (item.modules.module_author.decorate?.fan != null)
                        {
                            main.DrawText(item.modules.module_author.decorate.fan.num_str, Painting.Anywhere, new SKPoint(main.Width - 10 - 100, 32), SKColor.Parse(item.modules.module_author.decorate.fan.color), 12, SKTypeface.FromFile(Path.Combine(Config.BaseDirectory, "Assets", "fannum.ttf")));
                        }
                        break;

                    case 1:
                        main.DrawImage(decorate, new() { Left = main.Width - 10 - 60, Top = 18, Size = new() { Width = 60, Height = 34 } });
                        break;

                    default:
                        break;
                }
            }

            // 正文
            SKPoint point = new(padding, 73);
            RenderRichText(item, ref main, ref point);
            point = new(padding, point.Y + 5);
            switch (item.type)
            {
                case "DYNAMIC_TYPE_DRAW":
                    point = new(padding, point.Y + 10);
                    DrawMajorImage(item.modules.module_dynamic.major.draw, ref main, ref point, padding);
                    break;

                case "DYNAMIC_TYPE_AV":
                    point = new(padding, point.Y + 10);
                    DrawVideoElement(item.modules.module_dynamic.major.archive, ref main, ref point, (int)(main.Width - padding * 2));
                    break;

                case "DYNAMIC_TYPE_FORWARD":
                    point = new(padding, point.Y + 10);
                    DrawForward(item.orig, ref main, ref point);
                    break;

                case "DYNAMIC_TYPE_ARTICLE":
                    point = new(padding, point.Y + 10);
                    DrawArticle(item.modules.module_dynamic.major.article, ref main, ref point, (int)(main.Width - padding * 2));
                    break;
            }

            DrawInteractive(item.modules.module_interaction, ref main, ref point);
            DrawStat(item.modules.module_stat, ref main, ref point);

            main.Resize((int)main.Width, (int)point.Y + padding);
            main.Padding(padding, padding, padding, padding, new SKColor(244, 245, 247));
            string path = Path.Combine(Config.PicSaveBasePath, "BiliBiliMonitor", "Dynamic");
            Directory.CreateDirectory(path);
            string filename = $"{item.id_str}.png";
            main.Save(Path.Combine(path, filename));
            main.Dispose();
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
                    return !Config.DynamicFilters.Any(text.Contains);
                }
            }
            else
            {
                Debug.WriteLine(json.message);
            }

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

        private void DrawArticle(DynamicModel.Article item, ref Painting img, ref SKPoint point, int elementWidth = 100)
        {
            var initPoint = new SKPoint(point.X, point.Y);
            using var cover = img.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), item.covers[0].GetFileNameFromURL()));
            float imgHeight = cover.Width / (cover.Width / elementWidth) + 68;
            img.DrawRectangle(new SKRect { Left = point.X, Top = point.Y, Size = new SKSize { Width = elementWidth, Height = imgHeight } }, SKColors.White, new SKColor(229, 233, 239), 1);
            img.DrawImage(cover, new SKRect { Location = point, Size = new SKSize { Width = elementWidth, Height = imgHeight } });

            point = new(point.X + 16, point.Y + imgHeight + 5);
            point = img.DrawRelativeText(item.title, new SKRect { Left = point.X, Right = point.X + elementWidth - 16, Bottom = initPoint.Y + imgHeight - 10 }, point, SKColors.Black, 16, isBold: true);
            point.Y += 16 + 5;     

            point = img.DrawRelativeText(item.desc, new SKRect { Left = point.X, Right = point.X + elementWidth - 16, Bottom = initPoint.Y + imgHeight - 10 }, point, new SKColor(102, 102, 102), 14);

            point = new SKPoint(initPoint.X, initPoint.Y + imgHeight + 5);
        }

        /// <summary>
        /// 绘制转发
        /// </summary>
        private void DrawForward(DynamicModel.Item item, ref Painting img, ref SKPoint point)
        {
            SKPoint initalPoint = new(point.X, point.Y);
            if (item == null)
            {
                return;
            }
            // 转发模块双倍padding
            int padding = 10;
            int width = CanvasWidth - padding * 4;
            Painting main = new(width, 10000);
            main.Clear(new(244, 245, 247));
            SKPoint p = new(0, 0);
            using var avatar = main.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), item.modules.module_author.face.GetFileNameFromURL()));
            main.DrawImage(main.CreateCircularImage(avatar, 24), new SKRect { Location = p, Size = new(24, 24) });

            p = new(p.X + 24 + 8, p.Y + 3);
            p = main.DrawText(item.modules.module_author.name, Painting.Anywhere, p, new(0, 161, 214), 12);
            p = main.DrawText(item.modules.module_author.pub_action, Painting.Anywhere, new(p.X + 8, p.Y - 12), SKColors.Black, 12);
            p = new(0, p.Y + 12 + 5);

            RenderRichText(item, ref main, ref p, 0);

            p = new(0, p.Y + 5);
            switch (item.type)
            {
                case "DYNAMIC_TYPE_DRAW":
                    DrawMajorImage(item.modules.module_dynamic.major.draw, ref main, ref p, 0);
                    break;

                case "DYNAMIC_TYPE_AV":
                    DrawVideoElement(item.modules.module_dynamic.major.archive, ref main, ref p, width);
                    break;
            }

            img.DrawRectangle(new(initalPoint.X, initalPoint.Y, width + padding * 2 + initalPoint.X, p.Y + 20 + initalPoint.Y), new(244, 245, 247), SKColors.Black, 0);
            main.Resize((int)main.Width, (int)p.Y + 10);
            img.DrawImage(main.SnapShot(), new SKRect { Location = new(initalPoint.X + 10, initalPoint.Y + 10), Size = new(main.Width, main.Height) });
            point = new(initalPoint.X, initalPoint.Y + p.Y + 20 + 10);
            main.Dispose();
        }

        /// <summary>
        /// 绘制热评
        /// </summary>
        private void DrawInteractive(DynamicModel.Module_Interaction item, ref Painting img, ref SKPoint point)
        {
            if (item == null || item.items?.Length == 0)
            {
                return;
            }
            int padding = 10;
            int fontSize = 14;
            SKPoint initalPoint = new(point.X, point.Y);
            point = new(point.X + padding, point.Y + 10);

            using var comment = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "comment.png"));
            img.DrawImage(comment, new SKRect { Location = point, Size = new SKSize(18, 18) });
            point = new(point.X + 16 + 8, point.Y - 2);
            foreach (var node in item.items[0].desc.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_TEXT":
                    case "RICH_TEXT_NODE_TYPE_AT":
                        point = img.DrawText(node.text.Replace("\r", " ").Replace("\n", " "), new SKRect { Left = initalPoint.X + padding + 16 + 8, Right = CanvasWidth - padding }, point, SKColor.Parse("#6d757a"), fontSize);
                        point = new SKPoint(point.X, point.Y - fontSize);

                        break;

                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        using (var emoji = img.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), node.emoji.icon_url.GetFileNameFromURL())))
                        {
                            img.DrawImage(emoji, new SKRect { Location = point, Size = new(20, 20) });
                            point = new SKPoint(point.X + 20, point.Y);
                        }
                        break;

                    default:
                        break;
                }
            }
            img.DrawRectangle(new SKRect { Location = new(initalPoint.X , initalPoint.Y + 8), Size = new(2, fontSize + 6) }, SKColor.Parse("#e7e7e7"), SKColors.Black, 0);

            point = new(initalPoint.X, point.Y + 10);
        }

        /// <summary>
        /// 绘制图片元素
        /// </summary>
        private void DrawMajorImage(DynamicModel.Draw item, ref Painting img, ref SKPoint point, int startX = 10, int padding = 0)
        {
            SKPoint initalPoint = new(point.X, point.Y);

            int picCount = (int)(item?.items.Length);
            int imgMaxWidth = 0;
            if (picCount == 1)
            {
                var i = item.items[0];
                using var image = img.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), i.src.GetFileNameFromURL()));
                imgMaxWidth = 720 - padding * 2;
                int width = image.Width;
                int height = image.Height;
                if (image.Width > imgMaxWidth)
                {
                    width = imgMaxWidth;
                    height = (int)(image.Height * (imgMaxWidth / (float)image.Width));
                }
                img.DrawImage(image, new SKRect { Location = point, Size = new(width, height) });
                if (i.src.EndsWith(".gif"))
                {
                    int paddingLeft = 4;
                    int paddingTop = 2;
                    var textSize = img.MeasureString("动图", 14);
                    var textPoint = new SKPoint(point.X + width - 10 - textSize.Width - paddingLeft, point.Y + height - 10 - textSize.Height);
                    img.DrawRectangle(new SKRect { Location = new(textPoint.X - paddingLeft, textPoint.Y - paddingTop), Size = new(textSize.Width + paddingLeft * 2, textSize.Height + paddingTop * 2) }, new SKColor(0, 0, 0, 0xBB), SKColors.Black, 0);
                    img.DrawText("动图", Painting.Anywhere, textPoint, SKColors.White, 14);
                }
                point = new(startX, point.Y + height + 10);
            }
            else
            {
                imgMaxWidth = picCount == 4 ? 480 : 360;
                imgMaxWidth -= padding * 2;
                bool newLine = false;
                for (int index = 1; index <= picCount; index++)
                {
                    var image = img.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), item.items[index - 1].src.GetFileNameFromURL()));
                    if (image.Width >= imgMaxWidth && image.Height >= imgMaxWidth)
                    {
                        // 缩小后，剪裁顶部
                        if (image.Width > image.Height)
                        {
                            int width = (int)(image.Width / (image.Height / (imgMaxWidth * 1.0f)));
                            image = img.ResizeImage(image, width, imgMaxWidth);
                        }
                        else
                        {
                            int height = (int)(image.Height / (image.Width / (imgMaxWidth * 1.0f)));
                            image = img.ResizeImage(image, imgMaxWidth, height);
                        }
                        image = image.Subset(new SKRectI { Location = new(), Size = new(imgMaxWidth, imgMaxWidth) });
                    }
                    else if (image.Width >= imgMaxWidth && image.Height < imgMaxWidth)
                    {
                        // 剪裁左侧
                        image = image.Subset(new SKRectI { Location = new(), Size = new(image.Height, image.Height) });
                    }
                    else if (image.Width < imgMaxWidth && image.Height >= imgMaxWidth)
                    {
                        // 剪裁顶部
                        image = image.Subset(new SKRectI { Location = new(), Size = new(image.Width, image.Width) });
                    }
                    else
                    {
                        // 在中心绘制
                        using Painting frame = new(imgMaxWidth, imgMaxWidth);
                        frame.DrawRectangle(new SKRect { Size = new(imgMaxWidth, imgMaxWidth) }, SKColors.White, SKColors.Gray, 1);
                        frame.DrawImage(image, new SKRect { Location = new(imgMaxWidth / 2 - image.Width / 2, imgMaxWidth / 2 - image.Height / 2), Size = new(image.Width, image.Height) });
                        image.Dispose();
                        image = frame.SnapShot();
                    }
                    img.DrawImage(image, new SKRect { Location = point, Size = new(imgMaxWidth, imgMaxWidth) });
                    if (item.items[index - 1].src.EndsWith(".gif"))
                    {
                        int paddingLeft = 4;
                        int paddingTop = 2;
                        var textSize = img.MeasureString("动图", 14);
                        var textPoint = new SKPoint(point.X + imgMaxWidth - 10 - textSize.Width - paddingLeft, point.Y + imgMaxWidth - 10 - textSize.Height);
                        img.DrawRectangle(new SKRect { Location = new(textPoint.X - paddingLeft, textPoint.Y - paddingTop), Size = new(textSize.Width + paddingLeft * 2, textSize.Height + paddingTop * 2) }, new SKColor(0, 0, 0, 0xBB), SKColors.Black, 0);
                        img.DrawText("动图", Painting.Anywhere, textPoint, SKColors.White, 14);
                    }
                    image.Dispose();
                    if (picCount == 4)
                    {
                        newLine = index % 2 == 0;
                        point = index % 2 == 0 ? new(startX, point.Y + imgMaxWidth + 10) : new(point.X + imgMaxWidth + 10, point.Y);
                    }
                    else
                    {
                        newLine = index % 3 == 0;
                        point = index % 3 == 0 ? new(startX, point.Y + imgMaxWidth + 10) : new(point.X + imgMaxWidth + 10, point.Y);
                    }
                }
                if (!newLine)
                {
                    point = new SKPoint(initalPoint.X, point.Y + imgMaxWidth + 10);
                }
            }
        }

        /// <summary>
        /// 绘制动态数据
        /// </summary>
        private void DrawStat(DynamicModel.Module_Stat item, ref Painting img, ref SKPoint point)
        {
            if (item == null)
            {
                return;
            }
            int iconSize = 12;
            int fontSize = 12;
            SKPoint initalPoint = new(point.X, point.Y);
            point = new(point.X, point.Y + 20);
            using var forward = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "forward.png"));
            using var comment = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "comment.png"));
            using var like = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "like.png"));

            img.DrawImage(forward, new SKRect { Location = point, Size = new(iconSize, iconSize) });
            point = new(point.X + iconSize + 4, point.Y - 2);
            point = img.DrawText(Helper.ParseLongNumber(item.forward.count), Painting.Anywhere, point, SKColor.Parse("#6d757a"), fontSize);
            point = new(point.X + 20, point.Y - fontSize + 2);

            img.DrawImage(comment, new SKRect { Location = point, Size = new(iconSize, iconSize) });
            point = new(point.X + iconSize + 4, point.Y - 2);
            point = img.DrawText(Helper.ParseLongNumber(item.comment.count), Painting.Anywhere, point, SKColor.Parse("#6d757a"), fontSize);
            point = new(point.X + 20, point.Y - fontSize + 2);

            img.DrawImage(like, new SKRect { Location = point, Size = new(iconSize, iconSize) });
            point = new(point.X + iconSize + 4, point.Y - 2);
            point = img.DrawText(Helper.ParseLongNumber(item.like.count), Painting.Anywhere, point, SKColor.Parse("#6d757a"), fontSize);
        }

        /// <summary>
        /// 绘制视频元素
        /// </summary>
        private void DrawVideoElement(DynamicModel.Archive item, ref Painting img, ref SKPoint point, int elementWidth = 100)
        {
            SKPoint initialPoint = new(point.X, point.Y);
            using var cover = img.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), item.cover.GetFileNameFromURL()));
            float imgWidth = elementWidth * 0.33f;
            float imgHeight = cover.Height / (cover.Width / imgWidth);
            img.DrawRectangle(new(point.X, point.Y, elementWidth, point.Y + imgHeight), SKColors.White, new(229, 233, 239), 1);
            img.DrawImage(cover, new() { Location = point, Size = new(imgWidth, imgHeight) });

            int titleFontSize = 14;
            int bodyFontSize = 12;

            if (item.badge != null && !string.IsNullOrEmpty(item.badge.text))
            {
                var size = img.MeasureString(item.badge.text, 12);
                int paddingLeft = 5, paddingTop = 2;
                img.DrawRectangle(new SKRect() { Left = initialPoint.X + imgWidth - size.Width - 10 - paddingLeft * 2, Top = point.Y + 8, Size = new(size.Width + paddingLeft * 2, size.Height + paddingTop * 2) }, SKColor.Parse(item.badge.bg_color), SKColors.Black, 0);

                img.DrawText(item.badge.text, Painting.Anywhere, new(initialPoint.X + imgWidth - size.Width - 10 - paddingLeft, point.Y + 8 + paddingTop), SKColors.White, 12, isBold: true);
            }

            point = new(0, initialPoint.Y + 10);
            point = img.DrawRelativeText(item.title, new SKRect { Left = initialPoint.X + imgWidth + 10, Right = elementWidth - 10, Bottom = initialPoint.Y + imgHeight - 25 }, point, SKColors.Black, titleFontSize);
            point = new(0, point.Y + 10);

            point = img.DrawRelativeText(item.desc.Replace("\n", " ").Replace("\r", ""), new SKRect { Left = initialPoint.X + imgWidth + 10, Right = elementWidth - 10, Bottom = initialPoint.Y + imgHeight - 25 }, point, new SKColor(102, 102, 102), bodyFontSize);
            point = new(initialPoint.X + imgWidth + 10, point.Y - bodyFontSize);

            // stat
            point = new(initialPoint.X + imgWidth + 10, initialPoint.Y + imgHeight - 20);
            using var play = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "play.png"));
            img.DrawImage(play, new() { Location = point, Size = new(14, 14) });
            point = new(point.X + 16, point.Y);
            point = img.DrawText(item.stat.play, Painting.Anywhere, point, new(102, 102, 102), 12);
            point = new(point.X + 16, point.Y - 12);

            using var danmaku = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "danmaku.png"));
            img.DrawImage(danmaku, new() { Location = point, Size = new(14, 14) });
            point = new(point.X + 16, point.Y);
            point = img.DrawText(item.stat.danmaku, Painting.Anywhere, point, new(102, 102, 102), 12);

            point = new(initialPoint.X, initialPoint.Y + imgHeight + 5);
        }

        /// <summary>
        /// 绘制文本
        /// </summary>
        private void RenderRichText(DynamicModel.Item item, ref Painting img, ref SKPoint point, int padding = 14)
        {
            SKPoint initalPoint = new(point.X, point.Y);

            if (item == null)
            {
                return;
            }
            int bodyFontSize = 16;
            if (item.modules.module_dynamic.topic != null)
            {
                using var topic = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "topic.png"));
                img.DrawImage(topic, new SKRect { Location = initalPoint, Size = new() { Width = bodyFontSize, Height = bodyFontSize } });
                point = new(point.X + bodyFontSize + 4, point.Y - 3);
                img.DrawText(item.modules.module_dynamic.topic.name, Painting.Anywhere, point, new SKColor(0, 138, 197), bodyFontSize);
                point = new(initalPoint.X, point.Y + bodyFontSize + 3 + 3);
            }

            if (item.modules.module_dynamic.desc == null)
            {
                return;
            }

            foreach (var node in item.modules.module_dynamic.desc?.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_TEXT":
                        point = img.DrawText(node.text, new SKRect { Left = initalPoint.X, Right = CanvasWidth - padding }, point, SKColors.Black, bodyFontSize);
                        point.Y -= bodyFontSize;
                        break;

                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        using (var emoji = img.LoadImage(Path.Combine(Path.Combine(Config.BaseDirectory, "tmp"), node.emoji.icon_url.GetFileNameFromURL())))
                        {
                            img.DrawImage(emoji, new SKRect { Left = point.X, Top = point.Y, Size = new SKSize { Width = bodyFontSize + 2, Height = bodyFontSize + 2 } });
                        }
                        point = new(point.X + bodyFontSize + 2, point.Y);
                        break;

                    case "RICH_TEXT_NODE_TYPE_LOTTERY":
                        using (var gift = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "gift.png")))
                        {
                            img.DrawImage(gift, new SKRect { Left = point.X, Top = point.Y, Size = new SKSize { Width = bodyFontSize + 2, Height = bodyFontSize + 2 } });
                        }
                        point = new(point.X + bodyFontSize + 4, point.Y);
                        point = img.DrawText("互动抽奖", Painting.Anywhere, point, new SKColor(23, 139, 207), bodyFontSize);
                        point.Y -= bodyFontSize;

                        break;

                    case "RICH_TEXT_NODE_TYPE_WEB":
                        using (var url = img.LoadImage(Path.Combine(Config.BaseDirectory, "Assets", "url.png")))
                        {
                            img.DrawImage(url, new SKRect { Left = point.X, Top = point.Y, Size = new SKSize { Width = bodyFontSize + 2, Height = bodyFontSize + 2 } });
                        }
                        point = new(point.X + bodyFontSize + 4, point.Y);
                        point = img.DrawText("跳转网址", Painting.Anywhere, point, new SKColor(23, 139, 207), bodyFontSize);
                        point.Y -= bodyFontSize;

                        break;

                    case "RICH_TEXT_NODE_TYPE_AT":
                    case "RICH_TEXT_NODE_TYPE_TOPIC":
                        point = img.DrawText(node.text, Painting.Anywhere, point, new SKColor(23, 139, 207), bodyFontSize);
                        point.Y -= bodyFontSize;

                        break;

                    default:
                        break;
                }
            }

            point = new(initalPoint.X, point.Y + bodyFontSize + 3);
        }
    }
}