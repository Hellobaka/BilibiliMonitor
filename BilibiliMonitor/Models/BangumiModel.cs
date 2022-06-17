namespace BilibiliMonitor.Models
{
    public class BangumiModel
    {
        public class Main
        {
            public int code { get; set; }
            public string message { get; set; }
            public Result result { get; set; }
        }

        public class Result
        {
            public Main_Section main_section { get; set; }
            public Section[] section { get; set; }
        }

        public class Main_Section
        {
            public Episode[] episodes { get; set; }
            public int id { get; set; }
            public string title { get; set; }
            public int type { get; set; }
        }

        public class Episode
        {
            public int aid { get; set; }
            public string badge { get; set; }
            public Badge_Info badge_info { get; set; }
            public int badge_type { get; set; }
            public int cid { get; set; }
            public string cover { get; set; }
            public string from { get; set; }
            public int id { get; set; }
            public int is_premiere { get; set; }
            public string long_title { get; set; }
            public string share_url { get; set; }
            public int status { get; set; }
            public string title { get; set; }
            public string vid { get; set; }
        }

        public class Badge_Info
        {
            public string bg_color { get; set; }
            public string bg_color_night { get; set; }
            public string text { get; set; }
        }

        public class Section
        {
            public Episode1[] episodes { get; set; }
            public int id { get; set; }
            public string title { get; set; }
            public int type { get; set; }
        }

        public class Episode1
        {
            public int aid { get; set; }
            public string badge { get; set; }
            public Badge_Info1 badge_info { get; set; }
            public int badge_type { get; set; }
            public int cid { get; set; }
            public string cover { get; set; }
            public string from { get; set; }
            public int id { get; set; }
            public int is_premiere { get; set; }
            public string long_title { get; set; }
            public string share_url { get; set; }
            public int status { get; set; }
            public string title { get; set; }
            public string vid { get; set; }
        }

        public class Badge_Info1
        {
            public string bg_color { get; set; }
            public string bg_color_night { get; set; }
            public string text { get; set; }
        }

        public class DetailInfo
        {
            public int code { get; set; }
            public string message { get; set; }
            public Detail_Result result { get; set; }
        }

        public class Detail_Result
        {
            public Activity activity { get; set; }
            public string alias { get; set; }
            public Area[] areas { get; set; }
            public string bkg_cover { get; set; }
            public string cover { get; set; }
            public Detail_Episode[] episodes { get; set; }
            public string evaluate { get; set; }
            public Freya freya { get; set; }
            public string jp_title { get; set; }
            public string link { get; set; }
            public int media_id { get; set; }
            public int mode { get; set; }
            public New_Ep new_ep { get; set; }
            public Payment payment { get; set; }
            public Positive positive { get; set; }
            public Publish publish { get; set; }
            public Rating rating { get; set; }
            public string record { get; set; }
            public Rights rights { get; set; }
            public int season_id { get; set; }
            public string season_title { get; set; }
            public Season[] seasons { get; set; }
            public Series series { get; set; }
            public string share_copy { get; set; }
            public string share_sub_title { get; set; }
            public string share_url { get; set; }
            public Show show { get; set; }
            public int show_season_type { get; set; }
            public string square_cover { get; set; }
            public Stat stat { get; set; }
            public int status { get; set; }
            public string subtitle { get; set; }
            public string title { get; set; }
            public int total { get; set; }
            public int type { get; set; }
            public Up_Info up_info { get; set; }
            public User_Status user_status { get; set; }
        }

        public class Activity
        {
            public string head_bg_url { get; set; }
            public int id { get; set; }
            public string title { get; set; }
        }

        public class Freya
        {
            public string bubble_desc { get; set; }
            public int bubble_show_cnt { get; set; }
            public int icon_show { get; set; }
        }

        public class New_Ep
        {
            public string desc { get; set; }
            public int id { get; set; }
            public int is_new { get; set; }
            public string title { get; set; }
        }

        public class Payment
        {
            public int discount { get; set; }
            public Pay_Type pay_type { get; set; }
            public string price { get; set; }
            public string promotion { get; set; }
            public string tip { get; set; }
            public int view_start_time { get; set; }
            public int vip_discount { get; set; }
            public string vip_first_promotion { get; set; }
            public string vip_promotion { get; set; }
        }

        public class Pay_Type
        {
            public int allow_discount { get; set; }
            public int allow_pack { get; set; }
            public int allow_ticket { get; set; }
            public int allow_time_limit { get; set; }
            public int allow_vip_discount { get; set; }
            public int forbid_bb { get; set; }
        }

        public class Positive
        {
            public int id { get; set; }
            public string title { get; set; }
        }

        public class Publish
        {
            public int is_finish { get; set; }
            public int is_started { get; set; }
            public string pub_time { get; set; }
            public string pub_time_show { get; set; }
            public int unknow_pub_date { get; set; }
            public int weekday { get; set; }
        }

        public class Rating
        {
            public int count { get; set; }
            public float score { get; set; }
        }

        public class Rights
        {
            public int allow_bp { get; set; }
            public int allow_bp_rank { get; set; }
            public int allow_download { get; set; }
            public int allow_review { get; set; }
            public int area_limit { get; set; }
            public int ban_area_show { get; set; }
            public int can_watch { get; set; }
            public string copyright { get; set; }
            public int forbid_pre { get; set; }
            public int freya_white { get; set; }
            public int is_cover_show { get; set; }
            public int is_preview { get; set; }
            public int only_vip_download { get; set; }
            public string resource { get; set; }
            public int watch_platform { get; set; }
        }

        public class Series
        {
            public int series_id { get; set; }
            public string series_title { get; set; }
        }

        public class Show
        {
            public int wide_screen { get; set; }
        }

        public class Stat
        {
            public int coins { get; set; }
            public int danmakus { get; set; }
            public int favorite { get; set; }
            public int favorites { get; set; }
            public int likes { get; set; }
            public int reply { get; set; }
            public int share { get; set; }
            public int views { get; set; }
        }

        public class Up_Info
        {
            public string avatar { get; set; }
            public string avatar_subscript_url { get; set; }
            public int follower { get; set; }
            public int is_follow { get; set; }
            public int mid { get; set; }
            public string nickname_color { get; set; }
            public Pendant pendant { get; set; }
            public int theme_type { get; set; }
            public string uname { get; set; }
            public int verify_type { get; set; }
            public Vip_Label vip_label { get; set; }
            public int vip_status { get; set; }
            public int vip_type { get; set; }
        }

        public class Pendant
        {
            public string image { get; set; }
            public string name { get; set; }
            public int pid { get; set; }
        }

        public class Vip_Label
        {
            public string bg_color { get; set; }
            public int bg_style { get; set; }
            public string border_color { get; set; }
            public string text { get; set; }
            public string text_color { get; set; }
        }

        public class User_Status
        {
            public int area_limit { get; set; }
            public int ban_area_show { get; set; }
            public int follow { get; set; }
            public int follow_status { get; set; }
            public int login { get; set; }
            public int pay { get; set; }
            public int pay_pack_paid { get; set; }
            public int sponsor { get; set; }
        }

        public class Area
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Detail_Episode
        {
            public int aid { get; set; }
            public string badge { get; set; }
            public Detail_Badge_Info badge_info { get; set; }
            public int badge_type { get; set; }
            public string bvid { get; set; }
            public int cid { get; set; }
            public string cover { get; set; }
            public Dimension dimension { get; set; }
            public int duration { get; set; }
            public string from { get; set; }
            public int id { get; set; }
            public bool is_view_hide { get; set; }
            public string link { get; set; }
            public string long_title { get; set; }
            public int pub_time { get; set; }
            public int pv { get; set; }
            public string release_date { get; set; }
            public Rights1 rights { get; set; }
            public string share_copy { get; set; }
            public string share_url { get; set; }
            public string short_link { get; set; }
            public int status { get; set; }
            public string subtitle { get; set; }
            public string title { get; set; }
            public string vid { get; set; }
        }

        public class Detail_Badge_Info
        {
            public string bg_color { get; set; }
            public string bg_color_night { get; set; }
            public string text { get; set; }
        }

        public class Dimension
        {
            public int height { get; set; }
            public int rotate { get; set; }
            public int width { get; set; }
        }

        public class Rights1
        {
            public int allow_demand { get; set; }
            public int allow_dm { get; set; }
            public int allow_download { get; set; }
            public int area_limit { get; set; }
        }

        public class Season
        {
            public string badge { get; set; }
            public Detail_Badge_Info1 badge_info { get; set; }
            public int badge_type { get; set; }
            public string cover { get; set; }
            public string horizontal_cover_1610 { get; set; }
            public string horizontal_cover_169 { get; set; }
            public int media_id { get; set; }
            public New_Ep1 new_ep { get; set; }
            public int season_id { get; set; }
            public string season_title { get; set; }
            public int season_type { get; set; }
            public Stat1 stat { get; set; }
        }

        public class Detail_Badge_Info1
        {
            public string bg_color { get; set; }
            public string bg_color_night { get; set; }
            public string text { get; set; }
        }

        public class New_Ep1
        {
            public string cover { get; set; }
            public int id { get; set; }
            public string index_show { get; set; }
        }

        public class Stat1
        {
            public int favorites { get; set; }
            public int series_follow { get; set; }
            public int views { get; set; }
        }

    }
}
