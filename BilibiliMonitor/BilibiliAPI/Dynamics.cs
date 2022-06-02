using BilibiliMonitor.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor.BilibiliAPI
{
    public class Dynamics
    {
        private static string BaseUrl = "https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space?offset=&host_mid={0}";
        public int UID { get; set; }
        public string LastDynamicID { get; set; }
        public List<DynamicModel.Item> DynamicList { get; set; }
        public bool FetchDynamicList()
        {
            string url = string.Format(BaseUrl, UID);
            var json = JsonConvert.DeserializeObject<DynamicModel.Main>(Helper.Get(url).Result);
            if (json.code == 0)
            {
                DynamicList = json.data.items.ToList();
                if (DynamicList.Count > 0) LastDynamicID = DynamicList[0].id_str;
                for (int i = 1; i < DynamicList.Count; i++)
                {                    
                    if(!Helper.CompareNumString(LastDynamicID, DynamicList[i].id_str))
                    {
                        LastDynamicID = DynamicList[i].id_str;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
