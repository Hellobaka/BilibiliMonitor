using BilibiliMonitor.BilibiliAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BilibiliMonitor.Models;
using System.Text;

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
        public List<Bangumi> Bangumis { get; set; } = new();

        public delegate void DynamicUpdateHandler(DynamicModel.Item item, int id, string picPath);
        public event DynamicUpdateHandler OnDynamic;
        public delegate void StreamOpenHandler(LiveStreamsModel.RoomInfo roomInfo, LiveStreamsModel.UserInfo userInfo, string picPath);
        public event StreamOpenHandler OnStream;
        public delegate void BangumiUpdateHandler(BangumiModel.DetailInfo bangumiInfo, BangumiModel.Episode epInfo, string picPath);
        public event BangumiUpdateHandler OnBangumi;
        public delegate void BangumiEndHandler(Bangumi bangumi);
        public event BangumiEndHandler OnBangumiEnd;
        public UpdateChecker(string basePath, string picPath)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
            BasePath = basePath;
            PicPath = picPath;
            Instance = this;
            new Thread(() =>
            {
                while (true)
                {
                    if (Enabled)
                    {
                        Thread.Sleep(DynamicCheckCD * 60 * 1000);// 减缓速率

                        try
                        {
                            foreach (var dy in Dynamics)// 动态检查
                            {
                                string pic = null;
                                try
                                {
                                    // TODO: 移动至独立方法
                                    // 动态列表缓存
                                    for (int i = 0; i < dy.Used.Count; i++) 
                                    {
                                        var item = dy.Used[i];
                                        if (item.Item2.AddDays(1) < DateTime.Now)
                                        {
                                            dy.Used.Remove(item);
                                            i--;
                                        }
                                    }
                                    if (dy.FetchDynamicList())
                                    {
                                        dy.DownloadPics();
                                        pic = dy.DrawImage();
                                        GC.Collect();
                                        dy.ReFetchFlag = false;
                                        dynamicErrCount = 0;
                                    }
                                }
                                catch (Exception e)
                                {
                                    pic = null;
                                    dy.ReFetchFlag = true;
                                    LogHelper.Info("动态更新", $"错误次数={dynamicErrCount},exc={e.Message + e.StackTrace}", false);
                                    dynamicErrCount++;
                                    if (dynamicErrCount >= 3)
                                    {
                                        LogHelper.Info("动态更新", "错误次数超过上限", false);
                                        LogHelper.Info("异常捕获", e.Message + e.StackTrace, false);
                                        dy.ReFetchFlag = false;
                                        dynamicErrCount = 1;
                                        pic = "";
                                    }
                                }
                                finally
                                {
                                    if (pic != null)
                                    {
                                        OnDynamic?.Invoke(dy.LatestDynamic, dy.UID, pic);
                                        LogHelper.Info("动态更新", $"{dy.UserName}的动态有更新，id={dy.LastDynamicID}，路径={pic}");
                                    }
                                }
                            }

                            foreach (var live in LiveStreams)// 直播检查
                            {
                                string pic = null;
                                try
                                {
                                    if (live.FetchRoomInfo())
                                    {
                                        live.DownloadPics();
                                        pic = live.DrawLiveStreamPic();
                                        GC.Collect();
                                        live.ReFetchFlag = false;
                                        livestreamErrCount = 0;
                                    }
                                }
                                catch (Exception e)
                                {
                                    pic = null;
                                    live.ReFetchFlag = true;
                                    LogHelper.Info("直播更新", $"错误次数={livestreamErrCount},exc={e.Message + e.StackTrace}", false);
                                    livestreamErrCount++;
                                    if (livestreamErrCount >= 3)
                                    {
                                        LogHelper.Info("直播更新", "错误次数超过上限", false);
                                        live.ReFetchFlag = false;
                                        livestreamErrCount = 1;
                                        pic = "";
                                    }
                                }
                                finally
                                {
                                    if(pic != null)
                                    {
                                        OnStream?.Invoke(live.RoomInfo, live.UserInfo, pic);
                                        LogHelper.Info("开播", $"{live.UserInfo.info.uname}开播了，路径={pic}");
                                    }
                                }
                            }

                            List<int> removeBangumiList = new();
                            foreach (var bangumi in Bangumis)// 番剧检查
                            {
                                string pic = null;
                                try
                                {
                                    if (bangumi.FetchEPDetail())
                                    {
                                        bangumi.DownloadPic();
                                        pic = bangumi.DrawLastEpPic();
                                        GC.Collect();
                                        bangumi.ReFetchFlag = false;
                                        bangumiErrCount = 0;
                                    }
                                    if (bangumi.BangumiInfo.result.is_finish == "1")
                                    {
                                        LogHelper.Info("番剧完结", $"{bangumi.Name} 已完结，清除监测");
                                        removeBangumiList.Add(bangumi.SeasonID);
                                        OnBangumiEnd?.Invoke(bangumi);
                                    }
                                }
                                catch (Exception e)
                                {
                                    pic = null;
                                    bangumi.ReFetchFlag = true;
                                    LogHelper.Info("番剧更新", $"错误次数={bangumiErrCount},exc={e.Message + e.StackTrace}", false);
                                    bangumiErrCount++;
                                    if (bangumiErrCount >= 3)
                                    {
                                        LogHelper.Info("番剧更新", "错误次数超过上限", false);
                                        bangumi.ReFetchFlag = false;
                                        bangumiErrCount = 1;
                                        pic = "";
                                    }
                                }
                                finally
                                {
                                    if(pic != null)
                                    {
                                        OnBangumi?.Invoke(bangumi.BangumiInfo, bangumi.LastEp, pic);
                                        LogHelper.Info("番剧更新", $"{bangumi.Name} 更新了，路径={pic}");
                                    }
                                }
                            }

                            if (removeBangumiList.Count != 0)
                            {
                                foreach (int item in removeBangumiList)
                                {
                                    RemoveBangumi(item);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            LogHelper.Info("异常捕获", e.Message + e.StackTrace, false);
                        }
                    }
                }
            }).Start();
        }
        int bangumiErrCount = 0;
        int dynamicErrCount = 0;
        int livestreamErrCount = 0;
        public Dynamics AddDynamic(int uid)
        {
            if (Dynamics.Any(x => x.UID == uid))
            {
                return Dynamics.First(x => x.UID == uid);
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
                return LiveStreams.First(x => x.UID == uid);
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

        public Bangumi AddBangumi(int seasonId)
        {
            if (Bangumis.Any(x => x.SeasonID == seasonId)) return Bangumis.First(x => x.SeasonID == seasonId);
            Bangumi ban = new(seasonId);
            if (string.IsNullOrWhiteSpace(ban.Name)) return null;
            Bangumis.Add(ban);
            return ban;
        }

        public void RemoveBangumi(int seasonId)
        {
            if (Bangumis.Any(x => x.SeasonID == seasonId) is false) return;
            Bangumis.Remove(Bangumis.First(x => x.SeasonID == seasonId));
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
        public List<(int, string)> GetBangumiList()
        {
            List<(int, string)> ls = new();
            foreach (var item in Bangumis)
            {
                ls.Add((item.SeasonID, item.Name));
            }
            return ls;
        }
        public Dynamics GetDynamic(int uid)
        {
            foreach (var item in Dynamics)
            {
                if (item.UID == uid)
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
