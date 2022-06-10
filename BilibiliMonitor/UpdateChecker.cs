using BilibiliMonitor.BilibiliAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BilibiliMonitor.Models;

namespace BilibiliMonitor
{
    public class UpdateChecker
    {
        public static string BasePath { get; set; } = "";
        public static string PicPath { get; set; } = "";
        public int DynamicCheckCD { get; set; } = 5;
        public static UpdateChecker Instance { get; private set; }
        public bool Enabled { get; set; } = false;
        public List<Dynamics> Dynamics { get; set; } = new();

        public delegate void DynamicUpdateHandler(DynamicModel.Item item, string picPath);
        public event DynamicUpdateHandler OnDynamic;
        public delegate void StreamOpenHandler(LiveStreamsModel.RoomInfo item, string picPath);
        public event StreamOpenHandler OnStream;
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
                                    OnDynamic?.Invoke(dy.LatestDynamic, pic);
                                    LogHelper.Info("动态更新", $"{dy.UserName}的动态有更新，id={dy.LastDynamicID}，路径={pic}");
                                }
                            }
                        }

                        foreach (var uid in LiveStreams.FetchLiveStream())
                        {
                            LiveStreams.DownloadPics(uid);
                            string pic = LiveStreams.DrawLiveStreamPic(uid);
                            if (string.IsNullOrEmpty(pic) == false)
                            {
                                var info = LiveStreams.LiveStreamData[uid];
                                OnStream?.Invoke(info, pic);
                                LogHelper.Info("开播", $"{info.uname}开播了，路径={pic}");
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

        public void RemoveDynamic(int uid)
        {
            if (Dynamics.Any(x => x.UID == uid))
            {
                return;
            }

            Dynamics.Remove(Dynamics.First(x => x.UID == uid));
        }
        public void AddStream(int uid)
        {
            LiveStreams.AddUID(uid);
        }

        public void RemoveStream(int uid)
        {
            LiveStreams.RemoveUID(uid);
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
