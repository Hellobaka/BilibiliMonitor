namespace BilibiliMonitor.Models
{
    public class VideoModel
    {
        public class Main
        {
            public long code { get; set; }

            public string message { get; set; }

            public Data data { get; set; }
        }

        public class Data
        {
            public string bvid { get; set; }

            public long aid { get; set; }

            public string pic { get; set; }

            public string title { get; set; }

            public long pubdate { get; set; }

            public string desc { get; set; }

            public Owner owner { get; set; }

            public Stat stat { get; set; }
        }

        public class Owner
        {
            public long mid { get; set; }

            public string name { get; set; }

            public string face { get; set; }
        }

        public class Stat
        {
            public long aid { get; set; }

            public long view { get; set; }

            public long danmaku { get; set; }

            public long reply { get; set; }

            public long favorite { get; set; }

            public long coin { get; set; }

            public long share { get; set; }

            public long now_rank { get; set; }

            public long his_rank { get; set; }

            public long like { get; set; }

            public long dislike { get; set; }

            public string evaluation { get; set; }

            public string argue_msg { get; set; }
        }
    }
}