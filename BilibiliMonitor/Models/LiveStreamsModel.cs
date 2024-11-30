namespace BilibiliMonitor.Models
{
    public class LiveStreamsModel
    {
        public class UserInfo_Main
        {
            public int code { get; set; }

            public UserInfo data { get; set; }
        }

        public class UserInfo
        {
            public Info info { get; set; }

            public int room_id { get; set; }
        }

        public class Info
        {
            public long uid { get; set; }

            public string uname { get; set; }

            public string face { get; set; }
        }

        public class RoomInfo_Main
        {
            public int code { get; set; }

            public string message { get; set; }

            public RoomInfo data { get; set; }
        }

        public class RoomInfo
        {
            public long room_id { get; set; }

            public int live_status { get; set; }

            public string title { get; set; }

            public string user_cover { get; set; }

            public string live_time { get; set; }

            public string area_name { get; set; }
        }
    }
}