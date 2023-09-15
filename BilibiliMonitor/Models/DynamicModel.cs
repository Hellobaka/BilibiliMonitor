namespace BilibiliMonitor.Models
{
    public class DynamicModel
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
            public bool has_more { get; set; }
            public Item[] items { get; set; }
            public string offset { get; set; }
            public string update_baseline { get; set; }
            public int update_num { get; set; }
        }

        public class Item
        {
            public Basic basic { get; set; }
            public string id_str { get; set; }
            public Modules modules { get; set; }
            public string type { get; set; }
            public bool visible { get; set; }
            public Item orig { get; set; }
        }

        public class Basic
        {
            public string comment_id_str { get; set; }
            public int comment_type { get; set; }
            public Like_Icon like_icon { get; set; }
            public string rid_str { get; set; }
        }

        public class Like_Icon
        {
            public string action_url { get; set; }
            public string end_url { get; set; }
            public int id { get; set; }
            public string start_url { get; set; }
        }

        public class Modules
        {
            public Module_Author module_author { get; set; }
            public Module_Dynamic module_dynamic { get; set; }
            public Module_Interaction module_interaction { get; set; }
            public Module_More module_more { get; set; }
            public Module_Stat module_stat { get; set; }
            public Module_Tag module_tag { get; set; }
        }

        public class Module_Author
        {
            public Decorate decorate { get; set; }
            public string face { get; set; }
            public bool face_nft { get; set; }
            public object following { get; set; }
            public string jump_url { get; set; }
            public string label { get; set; }
            public long mid { get; set; }
            public string name { get; set; }
            public Official_Verify official_verify { get; set; }
            public Pendant pendant { get; set; }
            public string pub_action { get; set; }
            public string pub_time { get; set; }
            public int pub_ts { get; set; }
            public string type { get; set; }
            public Vip vip { get; set; }
        }

        public class Decorate
        {
            public string card_url { get; set; }
            public Fan fan { get; set; }
            public long id { get; set; }
            public string jump_url { get; set; }
            public string name { get; set; }
            public int type { get; set; }
        }

        public class Fan
        {
            public string color { get; set; }
            public bool is_fan { get; set; }
            public string num_str { get; set; }
            public int number { get; set; }
        }

        public class Official_Verify
        {
            public string desc { get; set; }
            public int type { get; set; }
        }

        public class Pendant
        {
            public int expire { get; set; }
            public string image { get; set; }
            public string image_enhance { get; set; }
            public string image_enhance_frame { get; set; }
            public string name { get; set; }
            public int pid { get; set; }
        }

        public class Vip
        {
            public int avatar_subscript { get; set; }
            public string avatar_subscript_url { get; set; }
            public long due_date { get; set; }
            public Label label { get; set; }
            public string nickname_color { get; set; }
            public int status { get; set; }
            public int theme_type { get; set; }
            public int type { get; set; }
        }

        public class Label
        {
            public string bg_color { get; set; }
            public int bg_style { get; set; }
            public string border_color { get; set; }
            public string label_theme { get; set; }
            public string path { get; set; }
            public string text { get; set; }
            public string text_color { get; set; }
        }

        public class Module_Dynamic
        {
            public Additional additional { get; set; }
            public Desc desc { get; set; }
            public Major major { get; set; }
            public Topic topic { get; set; }
        }
        public class Topic
        {
            public long id { get; set; }
            public string jump_url { get; set; }
            public string name { get; set; }
        }
        public class Additional
        {
            public Reserve reserve { get; set; }
            public Vote vote { get; set; }
            public string type { get; set; }
        }

        public class Reserve
        {
            public Button button { get; set; }
            public Desc1 desc1 { get; set; }
            public Desc2 desc2 { get; set; }
            public string jump_url { get; set; }
            public int reserve_total { get; set; }
            public int rid { get; set; }
            public int state { get; set; }
            public int stype { get; set; }
            public string title { get; set; }
            public int up_mid { get; set; }
        }

        public class Button
        {
            public Check check { get; set; }
            public int status { get; set; }
            public int type { get; set; }
            public Uncheck uncheck { get; set; }
            public Jump_Style jump_style { get; set; }
            public string jump_url { get; set; }
        }

        public class Check
        {
            public string icon_url { get; set; }
            public string text { get; set; }
        }

        public class Uncheck
        {
            public string icon_url { get; set; }
            public string text { get; set; }
        }

        public class Jump_Style
        {
            public string icon_url { get; set; }
            public string text { get; set; }
        }

        public class Desc1
        {
            public int style { get; set; }
            public string text { get; set; }
        }

        public class Desc2
        {
            public int style { get; set; }
            public string text { get; set; }
            public bool visible { get; set; }
        }

        public class Desc
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
            public string jump_url { get; set; }
        }
        public class Emoji
        {
            public string icon_url { get; set; }
            public int size { get; set; }
            public string text { get; set; }
            public int type { get; set; }
        }

        public class Major
        {
            public Draw draw { get; set; }
            public string type { get; set; }
            public Archive archive { get; set; }
            public Article article { get; set; }
        }

        public class Draw
        {
            public long id { get; set; }
            public Item1[] items { get; set; }
        }

        public class Item1
        {
            public int height { get; set; }
            public float size { get; set; }
            public string src { get; set; }
            public object[] tags { get; set; }
            public int width { get; set; }
        }

        public class Archive
        {
            public string aid { get; set; }
            public Badge badge { get; set; }
            public string bvid { get; set; }
            public string cover { get; set; }
            public string desc { get; set; }
            public bool disable_preview { get; set; }
            public string duration_text { get; set; }
            public string jump_url { get; set; }
            public Stat stat { get; set; }
            public string title { get; set; }
            public int type { get; set; }
        }

        public class Badge
        {
            public string bg_color { get; set; }
            public string color { get; set; }
            public string text { get; set; }
        }

        public class Stat
        {
            public string danmaku { get; set; }
            public string play { get; set; }
        }

        public class Module_Interaction
        {
            public Item2[] items { get; set; }
        }

        public class Item2
        {
            public Desc3 desc { get; set; }
            public int type { get; set; }
        }

        public class Desc3
        {
            public Rich_Text_Nodes[] rich_text_nodes { get; set; }
            public string text { get; set; }
        }

        public class Rich_Text_Nodes1
        {
            public string orig_text { get; set; }
            public string rid { get; set; }
            public string text { get; set; }
            public string type { get; set; }
        }

        public class Module_More
        {
            public Three_Point_Items[] three_point_items { get; set; }
        }

        public class Three_Point_Items
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
            public int count { get; set; }
            public bool forbidden { get; set; }
        }

        public class Forward
        {
            public int count { get; set; }
            public bool forbidden { get; set; }
        }

        public class Like
        {
            public int count { get; set; }
            public bool forbidden { get; set; }
            public bool status { get; set; }
        }

        public class Module_Tag
        {
            public string text { get; set; }
        }

        public class Orig
        {
            public Basic1 basic { get; set; }
            public object id_str { get; set; }
            public Modules1 modules { get; set; }
            public string type { get; set; }
            public bool visible { get; set; }
        }

        public class Basic1
        {
            public string comment_id_str { get; set; }
            public int comment_type { get; set; }
            public Like_Icon1 like_icon { get; set; }
            public string rid_str { get; set; }
        }

        public class Like_Icon1
        {
            public string action_url { get; set; }
            public string end_url { get; set; }
            public long id { get; set; }
            public string start_url { get; set; }
        }

        public class Modules1
        {
            public Module_Author1 module_author { get; set; }
            public Module_Dynamic1 module_dynamic { get; set; }
        }

        public class Module_Author1
        {
            public string face { get; set; }
            public bool face_nft { get; set; }
            public bool following { get; set; }
            public string jump_url { get; set; }
            public string label { get; set; }
            public int mid { get; set; }
            public string name { get; set; }
            public string pub_action { get; set; }
            public string pub_time { get; set; }
            public int pub_ts { get; set; }
            public string type { get; set; }
        }

        public class Module_Dynamic1
        {
            public object additional { get; set; }
            public object desc { get; set; }
            public Major1 major { get; set; }
            public object topic { get; set; }
        }

        public class Major1
        {
            public None none { get; set; }
            public string type { get; set; }
        }

        public class None
        {
            public string tips { get; set; }
        }
        public class Article
        {
            public string[] covers { get; set; }
            public string desc { get; set; }
            public string title { get; set; }
            public string jump_url { get; set; }
            public string label { get; set; }
        }
    }


    public class Vote
    {
        public string desc { get; set; }
        public int end_time { get; set; }
    }
}
