using BilibiliMonitor.BilibiliAPI;
using Newtonsoft.Json.Linq;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
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
            var group = Config.GetConfig<JObject>("Monitor_Dynamic");
            foreach (JProperty id in group.Values())
            {
                var o = id.Value.ToObject<int[]>();               
            }
        }
    }
}
