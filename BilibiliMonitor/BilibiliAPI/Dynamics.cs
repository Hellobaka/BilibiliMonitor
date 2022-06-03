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
        public Dynamics(int uid)
        {
            UID = uid;
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
            string text = File.ReadAllText(@"E:\DO\5050.json");
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
                _ = Helper.DownloadFile(item.modules.module_dynamic.major?.archive?.cover, "tmp").Result;
                foreach (var i in item.modules.module_dynamic.major.draw?.items)
                {
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

            using Image<Rgba32> background = new(652, (int)CalcMaxHeight(item), new Rgba32(244, 245, 247));
            IPath card = new RectangularPolygon(padding, padding, background.Width - padding * 2, background.Height - padding * 2);
            background.Mutate(x => x.Fill(Color.White, card));
            int left = 88;
            //头像
            Size avatarSize = new(48);
            using Image avatar = Image.Load(Path.Combine("tmp", item.modules.module_author.face.GetFileNameFromURL()));
            avatar.Mutate(x => x.Resize(avatarSize));
            using Image<Rgba32> avatarFrame = new(48, 48, new Rgba32(255, 255, 255, 0));
            IPath circle = new EllipsePolygon(avatarFrame.Width / 2, avatarFrame.Height / 2, avatarFrame.Width / 2);
            avatarFrame.Mutate(x => x.Fill(new ImageBrush(avatar), circle));
            background.Mutate(x => x.DrawImage(avatarFrame, new Point(24, 24), 1));

            if (!string.IsNullOrWhiteSpace(item.modules.module_author.pendant.image))
            {
                using Image pendant = Image.Load(Path.Combine("tmp", item.modules.module_author.pendant.image.GetFileNameFromURL()));
                pendant.Mutate(x => x.Resize(new Size(72, 72)));
                background.Mutate(x => x.DrawImage(pendant, new Point(12, 12), 1));
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
            var fanNum = fontCollection.Add(@"E:\编程\程序c#\BilibiliMonitor\BilibiliMonitor\bin\Debug\net5.0\a.ttf");
            
            if(item.modules.module_author.decorate?.fan != null)
            {
                decorate.Mutate(x => x.DrawText(item.modules.module_author.decorate.fan.num_str, fanNum.CreateFont(12), Color.ParseHex(item.modules.module_author.decorate.fan.color), new PointF(48, 17)));
            }
            background.Mutate(x => x.DrawImage(decorate, new Point(background.Width - padding - 24 - decorate.Width, 18), 1));
            //文本
            background.Mutate(x => x.DrawText(text+"\\/", font, Color.Black, new Point(100,100)));
            //background.Mutate(x => RenderRichText(item, x));
            background.Save("1.png");
        }
        private IImageProcessingContext RenderRichText(DynamicModel.Item item, IImageProcessingContext img)
        {
            if (item == null) return img;
            if (item.modules.module_dynamic.desc == null) return img;
            PointF point = new(88, 73);
            Font font = SystemFonts.CreateFont("Microsoft YaHei Light", 14);
            TextOptions options = new(font);
            int padding = 88, chargap = 1, maxWidth = 532;
            float maxCharWidth = 0, charHeight = 0;
            foreach (var node in item.modules.module_dynamic.desc?.rich_text_nodes)
            {
                switch (node.type)
                {
                    case "RICH_TEXT_NODE_TYPE_TEXT":
                        foreach(var c in node.text)
                        {
                            DrawString(img, c, Color.Black, ref point, options, padding, chargap, ref maxCharWidth, maxWidth, ref charHeight);
                        }
                        break;
                    case "RICH_TEXT_NODE_TYPE_EMOJI":
                        var emoji = Image.Load(Path.Combine("tmp", node.emoji.icon_url.GetFileNameFromURL()));
                        emoji.Mutate(x => x.Resize(new Size(20, 20)));
                        img.DrawImage(emoji, (Point)point, 1);
                        break;
                    case "RICH_TEXT_NODE_TYPE_WEB":
                        foreach (var c in node.orig_text)
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
        public static void DrawString(IImageProcessingContext img, char text, Color color, ref PointF point, TextOptions option, int padding, int charGap, ref float maxCharWidth, int maxWidth, ref float charHeight)
        {
            var charSize = TextMeasurer.Measure(text.ToString(), option);
            charHeight = Math.Max(charSize.Height, charHeight);
            if (text == '\n')
            {
                point.X = padding;
                point.Y += charHeight + 2;
                return;
            }
            maxCharWidth = Math.Max(maxCharWidth, charSize.Width);
            var pointClone = new PointF(point.X, point.Y);//在表达式内无法使用ref
            img.DrawText(text.ToString(), option.Font, color, pointClone);
            WrapTest(maxWidth, padding, charGap, charSize, ref point);
        }
        public static void WrapTest(int maxWidth, int padding, int charGap, FontRectangle charSize, ref PointF point)
        {
            if (point.X + charSize.Width >= maxWidth)
            {
                point.X = padding;
                point.Y += charSize.Height + 2;
            }
            else
            {
                point.X += charSize.Width + charGap;
            }
        }
        private float CalcMaxHeight(DynamicModel.Item item)
        {
            // header: 73
            // footer: 48
            // video: 129
            // interaction: 21
            // singal pic: 320
            // mutli pics: 104
            float width = 532;
            float height = 73 + 48;
            if (item.modules.module_dynamic.desc != null)
            {
                Font font = SystemFonts.CreateFont("Microsoft YaHei", 14);
                TextOptions options = new(font)
                {
                    TabWidth = 4,
                    WrappingLength = width,
                };
                var size = TextMeasurer.Measure(item.modules.module_dynamic.desc.text, options);
                height += size.Height;
            }
            if (item.modules.module_dynamic.major != null)
            {
                if (item.modules.module_dynamic.major.type == "MAJOR_TYPE_DRAW")
                {
                    if (item.modules.module_dynamic.major.draw != null)
                    {
                        int picCount = item.modules.module_dynamic.major.draw.items.Length;
                        if (picCount == 1)
                        {
                            height += 320;
                        }
                        else
                        {
                            if (picCount <= 9)
                            {
                                height += 108 * ((float)Math.Floor(picCount / 3.0));
                            }
                            else
                            {
                                height += 108 * 3;
                            }
                        }
                    }
                }
                else if(item.modules.module_dynamic.major.type == "MAJOR_TYPE_ARCHIVE")
                {
                    height += 129;
                }
            }
            return height;
        }
    }
}
