namespace BilibiliMonitor.Models
{
    public class DynamicModel
    {
        public class Main
        {
            public long code { get; set; }

            public string message { get; set; }

            public long ttl { get; set; }

            public Data data { get; set; }
        }

        public class Data
        {
            public bool has_more { get; set; }

            public Item[] items { get; set; }

            public string offset { get; set; }

            public string update_baseline { get; set; }

            public long update_num { get; set; }
        }

        public class Item
        {
            public string id_str { get; set; }

            public Modules modules { get; set; }

            public string type { get; set; }

            public bool? visible { get; set; }

            public Item? orig { get; set; }
        }

        public class Modules
        {
            public Module_Author? module_author { get; set; }

            public Module_Dynamic? module_dynamic { get; set; }

            public Module_More? module_more { get; set; }

            public Module_Stat? module_stat { get; set; }

            public Module_Tag? module_tag { get; set; }

            public object? module_longeraction { get; set; }

            public Module_Interaction? module_interaction { get; set; }
        }

        public class Module_Author
        {
            public string face { get; set; }

            public string name { get; set; }

            public string pub_action { get; set; }

            public long pub_ts { get; set; }

            public Vip vip { get; set; }
        }

        public class Vip
        {
            public string nickname_color { get; set; }

            public long status { get; set; }
        }

        public class Module_Dynamic
        {
            public Desc? desc { get; set; }

            public Major? major { get; set; }

            public Topic? topic { get; set; }
        }

        public class Desc
        {
            public Rich_Text_Nodes[] rich_text_nodes { get; set; }

            public string text { get; set; }
        }

        public class Emoji
        {
            public string icon_url { get; set; }

            public long size { get; set; }

            public string text { get; set; }

            public long type { get; set; }
        }

        public class Major
        {
            public string type { get; set; }

            public Archive archive { get; set; }

            public Opus opus { get; set; }
        }

        public class Archive
        {
            public string aid { get; set; }

            public Badge badge { get; set; }

            public string bvid { get; set; }

            public string cover { get; set; }

            public string desc { get; set; }

            public Stat stat { get; set; }

            public string title { get; set; }

            public long type { get; set; }
        }

        public class Badge
        {
            public string bg_color { get; set; }

            public string color { get; set; }

            public object icon_url { get; set; }

            public string text { get; set; }
        }

        public class Stat
        {
            public string danmaku { get; set; }

            public string play { get; set; }
        }

        public class Opus
        {
            public string[] fold_action { get; set; }

            public string jump_url { get; set; }

            public Pic[] pics { get; set; }

            public Summary summary { get; set; }

            public string title { get; set; }
        }

        public class Summary
        {
            public Rich_Text_Nodes[] rich_text_nodes { get; set; }

            public string text { get; set; }
        }
        
        public class Rich_Text_Nodes
        {
            public string orig_text { get; set; }

            public string text { get; set; }

            public string type { get; set; }

            public Emoji emoji { get; set; }
        }

        public class Pic
        {
            public object aigc { get; set; }

            public long height { get; set; }

            public float size { get; set; }

            public string url { get; set; }

            public long width { get; set; }
        }

        public class Topic
        {
            public long id { get; set; }

            public string jump_url { get; set; }

            public string name { get; set; }
        }

        public class Module_More
        {
            public Three_Polong_Items[] three_polong_items { get; set; }
        }

        public class Three_Polong_Items
        {
            public string label { get; set; }

            public string type { get; set; }
        }

        public class Module_Stat
        {
            public Comment comment { get; set; }

            public Forward forward { get; set; }

            public Like like { get; set; }
        }

        public class Comment
        {
            public long count { get; set; }

            public bool forbidden { get; set; }
        }

        public class Forward
        {
            public long count { get; set; }

            public bool forbidden { get; set; }
        }

        public class Like
        {
            public long count { get; set; }

            public bool forbidden { get; set; }

            public bool status { get; set; }
        }

        public class Module_Tag
        {
            public string text { get; set; }
        }

        public class Module_Interaction
        {
            public Item_Module_Interaction[] items { get; set; }
        }

        public class Item_Module_Interaction
        {
            public Desc_Module_Interaction desc { get; set; }
            public int type { get; set; }
        }

        public class Desc_Module_Interaction
        {
            public Rich_Text_Nodes[] rich_text_nodes { get; set; }
            public string text { get; set; }
        }
    }
}