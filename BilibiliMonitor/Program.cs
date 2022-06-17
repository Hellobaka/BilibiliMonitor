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
            Bangumi ban = new(41470);
            ban.DownloadPic();
            ban.FetchEPDetail();
            Console.WriteLine(ban.DrawLastEpPic());
        }
    }
}
