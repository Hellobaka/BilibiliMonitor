using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliMonitor.Models
{
    public class VideoTagModel
    {

        public class Main
        {
            public int code { get; set; }
            public string message { get; set; }
            public int ttl { get; set; }
            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public int tag_id { get; set; }
            public string tag_name { get; set; }
            public string cover { get; set; }
            public string head_cover { get; set; }
            public string content { get; set; }
            public string short_content { get; set; }
            public int type { get; set; }
            public int state { get; set; }
            public int ctime { get; set; }
            public Count count { get; set; }
            public int is_atten { get; set; }
            public int likes { get; set; }
            public int hates { get; set; }
            public int attribute { get; set; }
            public int liked { get; set; }
            public int hated { get; set; }
            public int extra_attr { get; set; }
        }

        public class Count
        {
            public int view { get; set; }
            public int use { get; set; }
            public int atten { get; set; }
        }

    }
}
