using BilibiliMonitor.BilibiliAPI;
using Newtonsoft.Json.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace BilibiliMonitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Bangumi ban = new(41481);
            ban.FetchEPDetail();
            ban.DownloadPic();
            Console.WriteLine(ban.DrawLastEpPic());
        }
    }
}
