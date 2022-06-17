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
            public Actor[] actor { get; set; }
            public string alias { get; set; }
            public string allow_bp { get; set; }
            public string allow_download { get; set; }
            public string area { get; set; }
            public int arealimit { get; set; }
            public string bangumi_id { get; set; }
            public string bangumi_title { get; set; }
            public string brief { get; set; }
            public int business_type { get; set; }
            public string coins { get; set; }
            public string copyright { get; set; }
            public string cover { get; set; }
            public string danmaku_count { get; set; }
            public int dm_seg { get; set; }
            public int ed_jump { get; set; }
            public Detail_Episode[] episodes { get; set; }
            public string evaluate { get; set; }
            public string favorites { get; set; }
            public int has_unfollow { get; set; }
            public string is_finish { get; set; }
            public int is_guide_follow { get; set; }
            public string jp_title { get; set; }
            public Limit_Info limit_info { get; set; }
            public Media media { get; set; }
            public string newest_ep_id { get; set; }
            public string newest_ep_index { get; set; }
            public string origin_name { get; set; }
            public string play_count { get; set; }
            public string pub_string { get; set; }
            public string pub_time { get; set; }
            public string pub_time_show { get; set; }
            public object[] related_seasons { get; set; }
            public Rights rights { get; set; }
            public string season_id { get; set; }
            public int season_status { get; set; }
            public string season_title { get; set; }
            public Season[] seasons { get; set; }
            public string share_url { get; set; }
            public string spid { get; set; }
            public string squareCover { get; set; }
            public string staff { get; set; }
            public object[] tag2s { get; set; }
            public object[] tags { get; set; }
            public string title { get; set; }
            public string update_pattern { get; set; }
            public User_Season user_season { get; set; }
            public int viewRank { get; set; }
            public int vip_quality { get; set; }
            public string watchingCount { get; set; }
            public string weekday { get; set; }
        }

        public class Limit_Info
        {
            public int code { get; set; }
            public Data data { get; set; }
            public string message { get; set; }
        }

        public class Data
        {
            public int down { get; set; }
            public int play { get; set; }
        }

        public class Media
        {
            public Area[] area { get; set; }
            public string cover { get; set; }
            public Episode_Index episode_index { get; set; }
            public int media_id { get; set; }
            public Publish publish { get; set; }
            public string title { get; set; }
            public int type_id { get; set; }
            public string type_name { get; set; }
        }

        public class Episode_Index
        {
            public string index_show { get; set; }
        }

        public class Publish
        {
            public int is_finish { get; set; }
            public int is_started { get; set; }
        }

        public class Area
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class Rights
        {
            public int arealimit { get; set; }
            public int is_started { get; set; }
        }

        public class User_Season
        {
            public string attention { get; set; }
            public int bp { get; set; }
            public string last_ep_index { get; set; }
            public string last_time { get; set; }
            public int report_ts { get; set; }
        }

        public class Actor
        {
            public string actor { get; set; }
            public int actor_id { get; set; }
            public string role { get; set; }
        }

        public class Detail_Episode
        {
            public string av_id { get; set; }
            public string coins { get; set; }
            public string cover { get; set; }
            public string danmaku { get; set; }
            public string episode_id { get; set; }
            public int episode_status { get; set; }
            public int episode_type { get; set; }
            public string from { get; set; }
            public string index { get; set; }
            public string index_title { get; set; }
            public string is_new { get; set; }
            public string is_webplay { get; set; }
            public string mid { get; set; }
            public string page { get; set; }
            public bool premiere { get; set; }
            public Up up { get; set; }
            public string update_time { get; set; }
        }

        public class Up
        {
        }

        public class Season
        {
            public string bangumi_id { get; set; }
            public string cover { get; set; }
            public string is_finish { get; set; }
            public string newest_ep_id { get; set; }
            public string newest_ep_index { get; set; }
            public string pub_real_time { get; set; }
            public string season_id { get; set; }
            public int season_status { get; set; }
            public string title { get; set; }
        }


    }
}
