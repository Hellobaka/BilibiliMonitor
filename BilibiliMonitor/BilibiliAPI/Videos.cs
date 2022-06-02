using BilibiliMonitor.Models;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Videos
    {
        private static string BaseURL = "http://api.bilibili.com/x/web-interface/view?bvid={0}";
        public static VideoModel.Data GetVideoInfo(string bvId)
        {
            string url = string.Format(BaseURL, bvId);
            var json = JsonConvert.DeserializeObject<VideoModel.Main>(Helper.Get(url).Result);
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
