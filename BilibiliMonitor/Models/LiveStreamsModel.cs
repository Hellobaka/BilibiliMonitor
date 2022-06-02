namespace BilibiliMonitor.Models
{
    public class LiveStreamsModel
    {

        public class Main
        {
            public int code { get; set; }
            public string msg { get; set; }
            public string message { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
        }

        public class RoomInfo
        {
            public string title { get; set; }
            public int room_id { get; set; }
            public int uid { get; set; }
            public int online { get; set; }
            public int live_time { get; set; }
            public int live_status { get; set; }
            public int short_id { get; set; }
            public int area { get; set; }
            public string area_name { get; set; }
            public int area_v2_id { get; set; }
            public string area_v2_name { get; set; }
            public string area_v2_parent_name { get; set; }
            public int area_v2_parent_id { get; set; }
            public string uname { get; set; }
            public string face { get; set; }
            public string tag_name { get; set; }
            public string tags { get; set; }
            public string cover_from_user { get; set; }
            public string keyframe { get; set; }
            public string lock_till { get; set; }
            public string hidden_till { get; set; }
            public int broadcast_type { get; set; }
        }

    }
}
