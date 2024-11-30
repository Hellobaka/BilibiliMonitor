namespace BilibiliMonitor.Models
{
    public class VideoTagModel
    {
        public class Main
        {
            public int code { get; set; }

            public string message { get; set; }

            public Datum[] data { get; set; }
        }

        public class Datum
        {
            public string tag_name { get; set; }
        }
    }
}