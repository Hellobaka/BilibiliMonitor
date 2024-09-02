namespace BilibiliMonitor.Models
{
    public class VideoModel
    {
        public class Main
        {
            public int code { get; set; }

            public string message { get; set; }

            public Data data { get; set; }
        }

        public class Data
        {
            public string bvid { get; set; }

            public int aid { get; set; }

            public string pic { get; set; }

            public string title { get; set; }

            public int pubdate { get; set; }

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
            public int aid { get; set; }

            public int view { get; set; }

            public int danmaku { get; set; }

            public int reply { get; set; }

            public int favorite { get; set; }

            public int coin { get; set; }

            public int share { get; set; }

            public int now_rank { get; set; }

            public int his_rank { get; set; }

            public int like { get; set; }

            public int dislike { get; set; }

            public string evaluation { get; set; }

            public string argue_msg { get; set; }
        }
    }
}