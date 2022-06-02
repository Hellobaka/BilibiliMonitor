using BilibiliMonitor.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            string text = Helper.Get(url).Result;
            var json = JsonConvert.DeserializeObject<DynamicModel.Main>(text);
            if (json.code == 0)
            {
                DynamicList = json.data.items.ToList();
                if (DynamicList.Count > 0) LastDynamicID = DynamicList[0].id_str;
                for (int i = 1; i < DynamicList.Count; i++)
                {                    
                    if(!Helper.CompareNumString(LastDynamicID, DynamicList[i].id_str))
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
    }
}
