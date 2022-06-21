using System;
using System.Collections.Generic;
using System.Text;

namespace BilibiliMonitor.Models
{
    public class UserInfoModel
    {

        public class Main
        {
            public int code { get; set; }
            public string message { get; set; }
            public int ttl { get; set; }
            public Data data { get; set; }
        }

        public class Data
        {
            public Card card { get; set; }
            public bool following { get; set; }
            public int archive_count { get; set; }
            public int article_count { get; set; }
            public int follower { get; set; }
            public int like_num { get; set; }
        }

        public class Card
        {
            public string mid { get; set; }
            public string name { get; set; }
            public bool approve { get; set; }
            public string sex { get; set; }
            public string rank { get; set; }
            public string face { get; set; }
            public int face_nft { get; set; }
            public int face_nft_type { get; set; }
            public string DisplayRank { get; set; }
            public int regtime { get; set; }
            public int spacesta { get; set; }
            public string birthday { get; set; }
            public string place { get; set; }
            public string description { get; set; }
            public int article { get; set; }
            public object[] attentions { get; set; }
            public int fans { get; set; }
            public int friend { get; set; }
            public int attention { get; set; }
            public string sign { get; set; }
            public Level_Info level_info { get; set; }
            public Pendant pendant { get; set; }
            public Nameplate nameplate { get; set; }
            public Official Official { get; set; }
            public Official_Verify official_verify { get; set; }
            public Vip vip { get; set; }
            public int is_senior_member { get; set; }
        }

        public class Level_Info
        {
            public int current_level { get; set; }
            public int current_min { get; set; }
            public int current_exp { get; set; }
            public int next_exp { get; set; }
        }

        public class Pendant
        {
            public int pid { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public int expire { get; set; }
            public string image_enhance { get; set; }
            public string image_enhance_frame { get; set; }
        }

        public class Nameplate
        {
            public int nid { get; set; }
            public string name { get; set; }
            public string image { get; set; }
            public string image_small { get; set; }
            public string level { get; set; }
            public string condition { get; set; }
        }

        public class Official
        {
            public int role { get; set; }
            public string title { get; set; }
            public string desc { get; set; }
            public int type { get; set; }
        }

        public class Official_Verify
        {
            public int type { get; set; }
            public string desc { get; set; }
        }

        public class Vip
        {
            public int type { get; set; }
            public int status { get; set; }
            public long due_date { get; set; }
            public int vip_pay_type { get; set; }
            public int theme_type { get; set; }
            public Label label { get; set; }
            public int avatar_subscript { get; set; }
            public string nickname_color { get; set; }
            public int role { get; set; }
            public string avatar_subscript_url { get; set; }
            public int tv_vip_status { get; set; }
            public int tv_vip_pay_type { get; set; }
            public int vipType { get; set; }
            public int vipStatus { get; set; }
        }

        public class Label
        {
            public string path { get; set; }
            public string text { get; set; }
            public string label_theme { get; set; }
            public string text_color { get; set; }
            public int bg_style { get; set; }
            public string bg_color { get; set; }
            public string border_color { get; set; }
            public bool use_img_label { get; set; }
            public string img_label_uri_hans { get; set; }
            public string img_label_uri_hant { get; set; }
            public string img_label_uri_hans_static { get; set; }
            public string img_label_uri_hant_static { get; set; }
        }

    }
}
