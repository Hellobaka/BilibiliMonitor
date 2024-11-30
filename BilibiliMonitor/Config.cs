using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace BilibiliMonitor
{
    /// <summary>
    /// 配置读取帮助类
    /// </summary>
    public class Config : ConfigBase
    {
        public Config(string configPath) : base(configPath)
        {
            Instance = this;
        }

        public static ConfigBase Instance { get; set; }

        public static string Cookies { get; set; } = "";

        public static string RefreshToken { get; set; } = "";

        public static int RefreshInterval { get; set; } = 120000;

        public static bool DebugMode { get; set; } = false;

        public static int BangumiRetryCount { get; set; } = 3;

        public static int DynamicRetryCount { get; set; } = 3;

        public static int LiveStreamRetryCount { get; set; } = 3;

        public static string BaseDirectory { get; set; } = "";

        public static string PicSaveBasePath { get; set; } = "";

        public override void LoadConfig()
        {
            Cookies = GetConfig("Cookies", "");
            RefreshToken = GetConfig("RefreshToken", "");
            RefreshInterval = GetConfig("RefreshInterval", 120 * 1000);
            BangumiRetryCount = GetConfig("BangumiRetryCount", 3);
            LiveStreamRetryCount = GetConfig("LiveStreamRetryCount", 3);
            DebugMode = GetConfig("DebugMode", false);
        }
    }
}