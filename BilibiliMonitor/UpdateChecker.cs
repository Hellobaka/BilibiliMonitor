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
        public List<LiveStreams> LiveStreams { get; set; } = new();

        public delegate void DynamicUpdateHandler(DynamicModel.Item item, int id, string picPath);
        public event DynamicUpdateHandler OnDynamic;
        public delegate void StreamOpenHandler(LiveStreamsModel.RoomInfo roomInfo, LiveStreamsModel.UserInfo userInfo, string picPath);
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
                        try
                        {
                            foreach (var dy in Dynamics)
                            {
                                try
                                {
                                    if (dy.FetchDynamicList())
                                    {
                                        dy.DownloadPics();
                                        string pic = dy.DrawImage();
                                        if (string.IsNullOrEmpty(pic) == false)
                                        {
                                            OnDynamic?.Invoke(dy.LatestDynamic, dy.UID, pic);
                                            LogHelper.Info("动态更新", $"{dy.UserName}的动态有更新，id={dy.LastDynamicID}，路径={pic}");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Info("异常捕获", e.Message + e.StackTrace, false);
                                }
                            }

                            foreach (var live in LiveStreams)
                            {
                                try
                                {
                                    if(live.FetchRoomInfo())
                                    {
                                        live.DownloadPics();
                                        string pic = live.DrawLiveStreamPic();
                                        if (string.IsNullOrEmpty(pic) == false)
                                        {
                                            OnStream?.Invoke(live.RoomInfo, live.UserInfo, pic);
                                            LogHelper.Info("开播", $"{live.UserInfo.info.uname}开播了，路径={pic}");
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Info("异常捕获", e.Message + e.StackTrace, false);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.Info("异常捕获", e.Message + e.StackTrace, false);
                        }
                        Thread.Sleep(DynamicCheckCD * 60 * 1000);
                    }
                }
            }).Start();
        }
        public Dynamics AddDynamic(int uid)
        {
            if (Dynamics.Any(x => x.UID == uid))
            {
                return null;
            }
            var dy = new Dynamics(uid);
            dy.FetchDynamicList();
            Dynamics.Add(dy);
            return dy;
        }

        public void RemoveDynamic(int uid)
        {
            if (!Dynamics.Any(x => x.UID == uid))
            {
                return;
            }

            Dynamics.Remove(Dynamics.First(x => x.UID == uid));
        }
        public LiveStreams AddStream(int uid)
        {
            if (LiveStreams.Any(x => x.UID == uid))
            {
                return null;
            }
            var live = new LiveStreams(uid);
            live.FetchRoomInfo();
            LiveStreams.Add(live);
            return live;
        }

        public void RemoveStream(int uid)
        {
            if (!LiveStreams.Any(x => x.UID == uid))
            {
                return;
            }

            LiveStreams.Remove(LiveStreams.First(x => x.UID == uid));
        }
        public List<(int, string, bool)> GetStreamList()
        {
            List<(int, string, bool)> ls = new();
            foreach (var item in LiveStreams)
            {
                ls.Add((item.UID, item.Name, item.Streaming));
            }
            return ls;
        }
        public List<(int, string)> GetDynamicList()
        {
            List<(int, string)> ls = new();
            foreach (var item in Dynamics)
            {
                ls.Add((item.UID, item.UserName));
            }
            return ls;
        }
        public Dynamics GetDynamic(int uid)
        {
            foreach (var item in Dynamics)
            {
                if(item.UID == uid)
                    return item;
            }
            return null;
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
