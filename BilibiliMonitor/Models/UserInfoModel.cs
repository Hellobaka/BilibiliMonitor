namespace BilibiliMonitor.Models
{
    public class UserInfoModel
    {
        public class Main
        {
            public int code { get; set; }

            public string message { get; set; }

            public Data data { get; set; }
        }

        public class Data
        {
            public Card card { get; set; }

            public int archive_count { get; set; }
        }

        public class Card
        {
            public int fans { get; set; }

            public Vip vip { get; set; }
        }

        public class Vip
        {
            public string nickname_color { get; set; }
        }
    }
}