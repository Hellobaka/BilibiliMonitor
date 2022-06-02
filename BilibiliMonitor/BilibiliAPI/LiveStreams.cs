using BilibiliMonitor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor.BilibiliAPI
{
    public static class LiveStreams
    {
        private static string BaseURL = "http://api.live.bilibili.com/room/v1/Room/get_status_info_by_uids";
        public static Dictionary<int, LiveStreamsModel.RoomInfo> LiveStreamData { get; set; } = new();
        public static void AddUID(int uid)
        {
            FetchLiveStream(new List<int> { uid });
        }
        public static void RemoveUID(int uid)
        {
            LiveStreamData.Remove(uid);
        }
        /// <summary>
        /// 根据传入的UID来更新直播列表
        /// </summary>
        /// <param name="uids">需要拉取的UID</param>
        /// <returns>有状态更新的用户</returns>
        public static List<int> FetchLiveStream(List<int> uids)
        {
            string text = Helper.Post(BaseURL, new { uids }).Result;
            var json = JsonConvert.DeserializeObject<LiveStreamsModel.Main>(text);
            List<int> update = new();
            if(json.code == 0)
            {
                foreach (var item in uids)
                {
                    var t = json.data[item.ToString()].ToObject<LiveStreamsModel.RoomInfo>();
                    if (LiveStreamData.ContainsKey(item))
                    {
                        if(t.live_status != LiveStreamData[item].live_status)
                        {
                            update.Add(item);
                        }
                        LiveStreamData[item] = t;
                    }
                    else
                    {
                        LiveStreamData.Add(item, t);
                    }
                }
            }
            else
            {
                Debug.WriteLine(json.message);
            }

            return update;
        }
    }
}
