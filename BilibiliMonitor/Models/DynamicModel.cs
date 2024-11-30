namespace BilibiliMonitor.Models
{
    public class DynamicModel
    {
        public class Main
        {
            public int code { get; set; }

            public string message { get; set; }

            public Data data { get; set; }
        }

        public class Data
        {

            public Item[] items { get; set; }
        }

        public class Item
        {

            public string id_str { get; set; }

            public Modules modules { get; set; }

            public string type { get; set; }

            public Item orig { get; set; }
        }

        public class Modules
        {
            public Module_Author module_author { get; set; }

            public Module_Dynamic module_dynamic { get; set; }

            public Module_Interaction module_interaction { get; set; }

            public Module_Stat module_stat { get; set; }
        }

        public class Module_Author
        {
            public Decorate decorate { get; set; }

            public string face { get; set; }

            public string name { get; set; }

            public Pendant pendant { get; set; }

            public string pub_action { get; set; }

            public int pub_ts { get; set; }

            public Vip vip { get; set; }
        }

        public class Decorate
        {
            public string card_url { get; set; }

            public Fan fan { get; set; }

            public int type { get; set; }
        }

        public class Fan
        {
            public string color { get; set; }

            public string num_str { get; set; }
        }

        public class Pendant
        {
            public string image { get; set; }
        }

        public class Vip
        {
            public string avatar_subscript_url { get; set; }

            public string nickname_color { get; set; }

            public int status { get; set; }
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
            public Desc1 desc1 { get; set; }

            public string title { get; set; }
        }

        public class Desc1
        {
            public string text { get; set; }
        }

        public class Desc
        {
            public Rich_Text_Nodes[] rich_text_nodes { get; set; }
        }

        public class Rich_Text_Nodes
        {
            public string orig_text { get; set; }

            public string text { get; set; }

            public string type { get; set; }

            public Emoji emoji { get; set; }
        }

        public class Emoji
        {
            public string icon_url { get; set; }
        }

        public class Major
        {
            public Draw draw { get; set; }

            public Archive archive { get; set; }

            public Article article { get; set; }
        }

        public class Draw
        {
            public Item1[] items { get; set; }
        }

        public class Item1
        {
            public int height { get; set; }

            public string src { get; set; }

            public object[] tags { get; set; }

            public int width { get; set; }
        }

        public class Archive
        {
            public Badge badge { get; set; }

            public string cover { get; set; }

            public string desc { get; set; }

            public Stat stat { get; set; }

            public string title { get; set; }
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
        }

        public class Desc3
        {
            public Rich_Text_Nodes[] rich_text_nodes { get; set; }
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
        }

        public class Forward
        {
            public int count { get; set; }
        }

        public class Like
        {
            public int count { get; set; }
        }

        public class Article
        {
            public string[] covers { get; set; }

            public string desc { get; set; }

            public string title { get; set; }
        }
    }

    public class Vote
    {
        public string desc { get; set; }

        public int end_time { get; set; }
    }
}