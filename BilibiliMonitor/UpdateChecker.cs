using BilibiliMonitor.BilibiliAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace BilibiliMonitor
{
    public class UpdateChecker
    {
        public static string BasePath { get; set; } = "";
        public static string PicPath { get; set; } = "";
        public int DynamicCheckCD { get; set; } = 1;
        public List<Dynamics> Dynamics { get; set; } = new();
        public static UpdateChecker Instance { get; private set; }
        public bool Enabled { get; set; } = false;
        public UpdateChecker(string basePath, string picPath)
        {
            BasePath = basePath;
            PicPath = picPath;
            Instance = this;
            new Thread(() =>
            {
                while (true)
                {
                    if (Enabled)
                    {
                        foreach (var dy in Dynamics)
                        {
                            if (dy.FetchDynamicList())
                            {
                                dy.DownloadPics();
                                string pic = dy.DrawImage();
                                if(string.IsNullOrEmpty(pic) == false)
                                {
                                    LogHelper.Info("动态更新", $"{dy.UserName}的动态有更新，id={dy.LastDynamicID}，路径={pic}");
                                }
                            }
                        }
                    }
                    Thread.Sleep(DynamicCheckCD * 60 * 1000);
                }
            }).Start();
        }
        public void AddDynamic(int uid)
        {
            if (Dynamics.Any(x => x.UID == uid))
            {
                return;
            }
            var dy = new Dynamics(uid);
            dy.FetchDynamicList();
            Dynamics.Add(dy);
        }
        public void Start()
        {
            Enabled = true;
        }
        public void Stop()
        {
            Enabled = false;
        }
    }
}
