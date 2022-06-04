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
using Path = System.IO.Path;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Dynamics
    {
        private static string BaseUrl = "https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space?offset=&host_mid={0}";
        public int UID { get; set; }
        public string LastDynamicID { get; set; }
        private static string FanNumFontPath { get; set; } = @"E:\编程\程序c#\BilibiliMonitor\BilibiliMonitor\bin\Debug\net5.0\a.ttf";
        private static FontFamily EmojiFont { get; set; }
        public Dynamics(int uid)
        {
            UID = uid;
            EmojiFont = new FontCollection().Add(@"C:\Windows\Fonts\seguiemj.ttf");
        }
        public List<DynamicModel.Item> DynamicList { get; set; } = new();
        /// <summary>
        /// 拉取动态列表并比对动态ID来获取最新动态
        /// </summary>
        /// <returns>是否有变化</returns>
        public bool FetchDynamicList()
        {
            string url = string.Format(BaseUrl, UID);
            // string text = Helper.Get(url).Result;
            string text = File.ReadAllText(@"E:\DO\dynamic.json");
            var json = JsonConvert.DeserializeObject<DynamicModel.Main>(text);
            if (json.code == 0)
            {
                DynamicList = json.data.items.ToList();
                if (DynamicList.Count > 0) LastDynamicID = DynamicList[0].id_str;
                for (int i = 1; i < DynamicList.Count; i++)
                {
                    if (!Helper.CompareNumString(LastDynamicID, DynamicList[i].id_str))
                    {
                        LastDynamicID = DynamicList[i].id_str;
                        return true;
                    }
                }
            }
            else
            {
                Debug.WriteLine(json.message);
            }
            return false;
        }
        public bool DownloadPics()
        {
            return DownloadPics(LastDynamicID);
        }
        public bool DownloadPics(string id)
        {
            return DownloadPics(DynamicList.First(x => x.id_str == id));
        }
        public bool DownloadPics(DynamicModel.Item item)
        {
            if (item == null) return false;
            try
            {
                _ = Helper.DownloadFile(item.modules.module_author.face, "tmp").Result;
                _ = Helper.DownloadFile(item.modules.module_author.vip?.avatar_subscript_url, "tmp").Result;
                _ = Helper.DownloadFile(item.modules.module_author.decorate?.card_url, "tmp").Result;
                _ = Helper.DownloadFile(item.modules.module_author.pendant?.image, "tmp").Result;
                if(item.modules.module_dynamic.major?.archive != null)
                {
                    item.modules.module_dynamic.major.archive.cover += "@203w_127h_1c.webp";
                    _ = Helper.DownloadFile(item.modules.module_dynamic.major.archive.cover, "tmp").Result;
                }
                int picCount = (int)(item.modules.module_dynamic.major.draw?.items.Length);
                foreach (var i in item.modules.module_dynamic.major.draw?.items)
                {
                    string webp = ".webp";
                    if (i.height / (double)i.width > 2)
                    {
                        if (picCount == 1)
                            webp = "240w_320h_!header" + webp;
                        else
                            webp = "104w_104h_!header" + webp;
                    }
                    else
                    {
                        if (picCount == 1)
                            webp = "320w_180h_1e_1c" + webp;
                        else
                            webp = "104w_104h_1e_1c" + webp;
                    }
                    i.src += "@" + webp;
                    _ = Helper.DownloadFile(i.src, "tmp").Result;
                }
                foreach (var i in item.modules.module_dynamic.desc?.rich_text_nodes)
                {
                    _ = Helper.DownloadFile(i.emoji?.icon_url, "tmp").Result;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
        }
        public void DrawImage()
        {
            DrawImage(LastDynamicID);
        }
        public void DrawImage(string id)
        {
            int padding = 10;
            DynamicModel.Item item = DynamicList.First(x => x.id_str == id);
            if (item == null) return;

            using Image<Rgba32> main = new(652, 30000, new Rgba32(244, 245, 247));
            using Image<Rgba32> background = new(632, 30000, Color.White);

            int left = 78;
            //头像
            Size avatarSize = new(48);
            using Image avatar = Image.Load(Path.Combine("tmp", item.modules.module_author.face.GetFileNameFromURL()));
            avatar.Mutate(x => x.Resize(avatarSize));
            using Image<Rgba32> avatarFrame = new(48, 48, new Rgba32(255, 255, 255, 0));
            IPath circle = new EllipsePolygon(avatarFrame.Width / 2, avatarFrame.Height / 2, avatarFrame.Width / 2);
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            background.Mutate(x => x.DrawImage(avatarFrame, new Point(14, 14), 1));

            if (!string.IsNullOrWhiteSpace(item.modules.module_author.pendant.image))
            {
                using Image pendant = Image.Load(Path.Combine("tmp", item.modules.module_author.pendant.image.GetFileNameFromURL()));
                pendant.Mutate(x => x.Resize(new Size(72, 72)));
                background.Mutate(x => x.DrawImage(pendant, new Point(2, 2), 1));
            }
            //认证

            //标题
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 16);
            Color nameColor = Color.Black;
            if (item.modules.module_author.vip?.status == 1)
            {
                nameColor = Color.ParseHex(item.modules.module_author.vip.nickname_color);
            }
            background.Mutate(x => x.DrawText(item.modules.module_author.name, font, nameColor, new PointF(left, 27)));
            font = SystemFonts.CreateFont("Microsoft YaHei", 12);
            string text = $"{item.modules.module_author.pub_time}{(string.IsNullOrWhiteSpace(item.modules.module_author.pub_action) ? "" : " · ")}{item.modules.module_author.pub_action}";
            background.Mutate(x => x.DrawText(text, font, new Rgba32(153, 162, 170), new PointF(left, 27 + 24)));
            //装扮
            using Image decorate = Image.Load(Path.Combine("tmp", item.modules.module_author.decorate?.card_url.GetFileNameFromURL()));
            decorate.Mutate(x => x.Resize(146, 44));
            var fontCollection = new FontCollection();
            var fanNum = fontCollection.Add(FanNumFontPath);

            if (item.modules.module_author.decorate?.fan != null)
            {
                decorate.Mutate(x => x.DrawText(item.modules.module_author.decorate.fan.num_str, fanNum.CreateFont(12), Color.ParseHex(item.modules.module_author.decorate.fan.color), new PointF(48, 17)));
            }
            background.Mutate(x => x.DrawImage(decorate, new Point(background.Width - padding - 24 - decorate.Width, 18), 1));
            //文本
            PointF point = new(78, 73);
            background.Mutate(x => RenderRichText(item, x, ref point));
            switch (item.type)
            {
                case "DYNAMIC_TYPE_DRAW":
                    point = new(78, point.Y + 30);
                    background.Mutate(x => DrawMajorImage(item.modules.module_dynamic.major.draw, x, ref point));
                    break;
                case "DYNAMIC_TYPE_AV":
                    point = new(78, point.Y + 10);
                    background.Mutate(x => DrawVideoElement(item.modules.module_dynamic.major.archive, x, ref point));
                    break;
            }
            background.Mutate(x => DrawInteractive(item.modules.module_interaction, x, ref point));
            background.Mutate(x => DrawStat(item.modules.module_stat, x, ref point));

            background.Mutate(x => x.Crop(background.Width, (int)point.Y + padding));
            main.Mutate(x => x.Crop(main.Width, background.Height + padding * 2));
            main.Mutate(x => x.DrawImage(background, new Point(padding, padding), 1));
            main.Save("1.png");
        }
        private IImageProcessingContext DrawVideoElement(DynamicModel.Archive item, IImageProcessingContext img, ref PointF point)
        {
            Point initialPoint = (Point)point;
            IPath container = new RectangularPolygon(point.X, point.Y, 532, 127);
            img.Draw(Pens.Solid(new Rgba32(229, 233, 239), 1), container);
            using var cover = Image.Load(Path.Combine("tmp", item.cover.GetFileNameFromURL()));
            img.DrawImage(cover, (Point)point, 1);
            container = new RectangularPolygon(point.X + 137, point.Y + 8, 58, 18);
            img.Fill(Rgba32.ParseHex(item.badge.bg_color), container);
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Bold);
            img.DrawText(item.badge.text, font, Rgba32.ParseHex(item.badge.color), (Point)new PointF(point.X + 137 + 5, point.Y + 8));
            point = new(point.X + 203 + 16, point.Y + 9);

            font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 1, maxWidth = 600 - 12;
            float maxCharWidth = 0, charHeight = 0;
            foreach(var c in item.title)
            {
                DrawString(img, c, Color.Black, ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }
            point = new(padding, point.Y + 25);
            if(item.desc.Length > 44)
            {
                item.desc = item.desc[..44] + "..";
            }
            font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            options = new(font);
            foreach (var c in item.desc)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }
            point = new(padding, initialPoint.Y + 109);
            using var play = Image.Load(Path.Combine("component", "play.png"));
            play.Mutate(x => x.Resize(14, 14));
            img.DrawImage(play, (Point)point, 1);
            point = new(point.X + 16, initialPoint.Y + 107);
            foreach (var c in item.stat.play)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }

            point = new(point.X + 16, point.Y + 2);
            using var danmaku = Image.Load(Path.Combine("component", "danmaku.png"));
            danmaku.Mutate(x => x.Resize(14, 14));
            img.DrawImage(danmaku, (Point)point, 1);
            point = new(point.X + 16, initialPoint.Y + 107);
            foreach (var c in item.stat.danmaku)
            {
                DrawString(img, c, new Color(new Rgba32(102, 102, 102)), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }
            point = new Point(78, initialPoint.Y + 127);
            return img;
        }
        private IImageProcessingContext DrawStat(DynamicModel.Module_Stat item, IImageProcessingContext img, ref PointF point)
        {
            if (item == null) return img;
            PointF initalPoint = new(point.X, point.Y);
            point = new(point.X, point.Y + 20);
            using var forward = Image.Load(Path.Combine("component", "forward.png"));
            forward.Mutate(x => x.Resize(16, 16));
            using var comment = Image.Load(Path.Combine("component", "comment.png"));
            comment.Mutate(x => x.Resize(16, 16));
            using var like = Image.Load(Path.Combine("component", "like.png"));
            like.Mutate(x => x.Resize(16, 16));

            img.DrawImage(forward, (Point)point, 1);
            point = new(point.X + 16 + 4, point.Y);
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 1, maxWidth = 610 - 12;
            float maxCharWidth = 0, charHeight = 0;
            foreach(var c in item.forward.count.ParseNum2Chinese())
            {
                DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }
            point = new(point.X + 10, point.Y);

            img.DrawImage(comment, (Point)point, 1);
            point = new(point.X + 16 + 4, point.Y);
            foreach(var c in item.comment.count.ParseNum2Chinese())
            {
                DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }
            point = new(point.X + 10, point.Y);

            img.DrawImage(like, (Point)point, 1);
            point = new(point.X + 16 + 4, point.Y);
            foreach(var c in item.like.count.ParseNum2Chinese())
            {
                DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
            }

            point = new(initalPoint.X, point.Y + 16);
            return img;
        }
        private IImageProcessingContext DrawInteractive(DynamicModel.Module_Interaction item, IImageProcessingContext img, ref PointF point)
        {
            if (item == null || item.items?.Length == 0) return img;
            PointF initalPoint = new(point.X, point.Y);
            point = new(78 + 8, point.Y + 5);

            using var comment = Image.Load(Path.Combine("component", "comment.png"));
            comment.Mutate(x => x.Resize(16, 16));
            img.DrawImage(comment, (Point)point, 1);
            point = new(point.X + 14 + 8, point.Y);

            string text = "";
            foreach(var i in item.items[0].desc.rich_text_nodes)
            {
                text += i.orig_text;
            }
            Font font = SystemFonts.CreateFont("Microsoft YaHei", 12, FontStyle.Regular);
            TextOptions options = new(font);
            int padding = (int)point.X, chargap = 1, maxWidth = 610 - 12;
            float maxCharWidth = 0, charHeight = 0, totalHeight = 0;
            foreach (var node in item.items[0].desc.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_AT":
                        foreach (var c in node.text)
                        {
                            totalHeight = DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }
                        break;
                    case "RICH_TEXT_NODE_TYPE_TEXT":
                        foreach (var c in node.text)
                        {
                            totalHeight = DrawString(img, c, Rgba32.ParseHex("#6d757a"), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
                        }
                        break;
                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        var emoji = Image.Load(Path.Combine("tmp", node.emoji.icon_url.GetFileNameFromURL()));
                        emoji.Mutate(x => x.Resize(new Size(20, 20)));
                        img.DrawImage(emoji, (Point)point, 1);// ? point
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
        private IImageProcessingContext DrawForward(DynamicModel.Module_Interaction item, IImageProcessingContext img, ref PointF point)
        {

            return img;
        }
        private IImageProcessingContext DrawMajorImage(DynamicModel.Draw item, IImageProcessingContext img, ref PointF point)
        {
            PointF initalPoint = new(point.X, point.Y);

            int picCount = (int)(item?.items.Length);
            if (picCount == 1)
            {
                var i = item.items[0];
                using Image image = Image.Load(Path.Combine("tmp", i.src.GetFileNameFromURL()));
                img.DrawImage(image, (Point)point, 1);
                point = new(78, point.Y + image.Height);
            }
            else
            {
                if (picCount == 4)
                {
                    for (int index = 1; index <= picCount; index++)
                    {
                        using Image tmp = Image.Load(Path.Combine("tmp", item.items[index - 1].src.GetFileNameFromURL()));
                        img.DrawImage(tmp, (Point)point, 1);
                        if (index % 2 == 0)
                        {
                            point = new(78, point.Y + 108);
                        }
                        else
                        {
                            point = new(point.X + 108, point.Y);
                        }
                    }
                }
                else
                {
                    for (int index = 1; index <= picCount; index++)
                    {
                        using Image tmp = Image.Load(Path.Combine("tmp", item.items[index - 1].src.GetFileNameFromURL()));
                        img.DrawImage(tmp, (Point)point, 1);
                        if (index % 3 == 0)
                        {
                            point = new(78, point.Y + 108);
                        }
                        else
                        {
                            point = new(point.X + 108, point.Y);
                        }
                    }
                    point = new(initalPoint.X, point.Y + 108);
                }
            }
            return img;
        }
        private IImageProcessingContext RenderRichText(DynamicModel.Item item, IImageProcessingContext img, ref PointF point)
        {
            if (item == null) return img;
            Font font;
            TextOptions options;
            int padding = 78, chargap = 1, maxWidth = 532;
            float maxCharWidth = 0, charHeight = 0;
            if (item.modules.module_dynamic.topic != null)
            {
                using var topic = Image.Load(Path.Combine("component", "topic.png"));
                topic.Mutate(x => x.Resize(18, 18));
                img.DrawImage(topic, (Point)point, 1);
                point = new(point.X + 18, point.Y);
                font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                img.DrawText(item.modules.module_dynamic.topic.name, font, new Rgba32(0, 138, 197), point);
                point = new(point.X, point.Y + 15);
            }
            if (item.modules.module_dynamic.desc == null) return img;
            foreach (var node in item.modules.module_dynamic.desc?.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_TEXT":
                        font = SystemFonts.CreateFont("Microsoft YaHei Light", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in node.text)
                        {
                            DrawString(img, c, Color.Black, ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }
                        break;
                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        var emoji = Image.Load(Path.Combine("tmp", node.emoji.icon_url.GetFileNameFromURL()));
                        emoji.Mutate(x => x.Resize(new Size(20, 20)));
                        img.DrawImage(emoji, (Point)point, 1);
                        point = new(point.X + 20, point.Y);
                        break;
                    case "RICH_TEXT_NODE_TYPE_LOTTERY":
                        using (Image gift = Image.Load(Path.Combine("component", "gift.png")))
                        {
                            gift.Mutate(x => x.Resize(18, 18));
                            img.DrawImage(gift, (Point)point, 1);
                            point = new(point.X + 22, point.Y);
                        }
                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in "互动抽奖")
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }
                        break;
                    case "RICH_TEXT_NODE_TYPE_WEB":
                        using (Image url = Image.Load(Path.Combine("component", "url.png")))
                        {
                            url.Mutate(x => x.Resize(18, 18));
                            img.DrawImage(url, (Point)point, 1);
                            point = new(point.X + 22, point.Y);
                        }
                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in "跳转网址")
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }
                        break;
                    case "RICH_TEXT_NODE_TYPE_TOPIC":
                        font = SystemFonts.CreateFont("Microsoft YaHei", 14, FontStyle.Regular);
                        options = new(font);
                        foreach (var c in node.text)
                        {
                            DrawString(img, c, new Color(new Rgba32(23, 139, 207)), ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }
                        break;
                    default:
                        break;
                }
            }
            return img;
        }
        private static string emojiStore = "";
        public static float DrawString(IImageProcessingContext img, char c, Color color, ref PointF point, TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight, float totalHeight = 0)
        {
            string target;
            if(c.JudgeEmoji())
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
            if (string.IsNullOrWhiteSpace(target)) target = c.ToString();
            return DrawString(img, target, color, ref point, option, padding, charGap, ref maxCharWidth, maxWidth, ref charHeight, totalHeight);
        }
        public static float DrawString(IImageProcessingContext img, string text, Color color, ref PointF point, TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight, float totalHeight = 0)
        {
            var charSize = TextMeasurer.Measure(text, option);
            charHeight = Math.Max(charSize.Height, charHeight);
            if (totalHeight == 0) totalHeight = charHeight;
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
        public static float WrapTest(int maxWidth, int padding, int charGap, FontRectangle charSize, ref PointF point, float totalHeight)
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
    }
}
