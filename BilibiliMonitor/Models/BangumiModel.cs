namespace BilibiliMonitor.Models
{
    public class BangumiModel
    {
        public class Main
        {
            public int code { get; set; }

            public Result result { get; set; }
        }

        public class Result
        {
            public Main_Section main_section { get; set; }
        }

        public class Main_Section
        {
            public Episode[] episodes { get; set; }
        }

        public class Episode
        {
            public string cover { get; set; }

            public int id { get; set; }

            public string long_title { get; set; }

            public string share_url { get; set; }

            public string title { get; set; }
        }

        public class DetailInfo
        {
            public int code { get; set; }

            public Detail_Result result { get; set; }
        }

        public class Detail_Result
        {
            public Detail_Episode[] episodes { get; set; }

            public string is_finish { get; set; }

            public string season_id { get; set; }

            public string squareCover { get; set; }

            public string title { get; set; }
        }

        public class Detail_Episode
        {
            public string av_id { get; set; }

            public string mid { get; set; }
        }
    }
}