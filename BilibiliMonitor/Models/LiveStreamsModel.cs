using Newtonsoft.Json.Linq;

namespace BilibiliMonitor.Models
{
    public class LiveStreamsModel
    {
        public class UserInfo_Main
        {
            public int code { get; set; }
            public string msg { get; set; }
            public string message { get; set; }
            public UserInfo data { get; set; }
        }

        public class UserInfo
        {
            public Info info { get; set; }
            public Exp exp { get; set; }
            public int follower_num { get; set; }
            public int room_id { get; set; }
            public string medal_name { get; set; }
            public int glory_count { get; set; }
            public string pendant { get; set; }
            public int link_group_num { get; set; }
            public Room_News room_news { get; set; }
        }

        public class Info
        {
            public int uid { get; set; }
            public string uname { get; set; }
            public string face { get; set; }
            public Official_Verify official_verify { get; set; }
            public int gender { get; set; }
        }

        public class Official_Verify
        {
            public int type { get; set; }
            public string desc { get; set; }
        }

        public class Exp
        {
            public Master_Level master_level { get; set; }
        }

        public class Master_Level
        {
            public int level { get; set; }
            public int color { get; set; }
            public int[] current { get; set; }
            public int[] next { get; set; }
        }

        public class Room_News
        {
            public string content { get; set; }
            public string ctime { get; set; }
            public string ctime_text { get; set; }
        }

        public class RoomInfo_Main
        {
            public int code { get; set; }
            public string msg { get; set; }
            public string message { get; set; }
            public RoomInfo data { get; set; }
        }
        public class RoomInfo
        {
            public int uid { get; set; }
            public int room_id { get; set; }
            public int short_id { get; set; }
            public int attention { get; set; }
            public int online { get; set; }
            public bool is_portrait { get; set; }
            public string description { get; set; }
            public int live_status { get; set; }
            public int area_id { get; set; }
            public int parent_area_id { get; set; }
            public string parent_area_name { get; set; }
            public int old_area_id { get; set; }
            public string background { get; set; }
            public string title { get; set; }
            public string user_cover { get; set; }
            public string keyframe { get; set; }
            public bool is_strict_room { get; set; }
            public string live_time { get; set; }
            public string tags { get; set; }
            public int is_anchor { get; set; }
            public string room_silent_type { get; set; }
            public int room_silent_level { get; set; }
            public int room_silent_second { get; set; }
            public string area_name { get; set; }
            public string pendants { get; set; }
            public string area_pendants { get; set; }
            public string[] hot_words { get; set; }
            public int hot_words_status { get; set; }
            public string verify { get; set; }
            public New_Pendants new_pendants { get; set; }
            public string up_session { get; set; }
            public int pk_status { get; set; }
            public int pk_id { get; set; }
            public int battle_id { get; set; }
            public int allow_change_area_time { get; set; }
            public int allow_upload_cover_time { get; set; }
            public Studio_Info studio_info { get; set; }
        }
        public class New_Pendants
        {
            public Frame frame { get; set; }
            public Badge badge { get; set; }
            public Mobile_Frame mobile_frame { get; set; }
            public object mobile_badge { get; set; }
        }
        public class Frame
        {
            public string name { get; set; }
            public string value { get; set; }
            public int position { get; set; }
            public string desc { get; set; }
            public int area { get; set; }
            public int area_old { get; set; }
            public string bg_color { get; set; }
            public string bg_pic { get; set; }
            public bool use_old_area { get; set; }
        }
        public class Badge
        {
            public string name { get; set; }
            public int position { get; set; }
            public string value { get; set; }
            public string desc { get; set; }
        }
        public class Mobile_Frame
        {
            public string name { get; set; }
            public string value { get; set; }
            public int position { get; set; }
            public string desc { get; set; }
            public int area { get; set; }
            public int area_old { get; set; }
            public string bg_color { get; set; }
            public string bg_pic { get; set; }
            public bool use_old_area { get; set; }
        }
        public class Studio_Info
        {
            public int status { get; set; }
            public object[] master_list { get; set; }
        }
    }
}
