using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BilibiliMonitor
{
    // https://socialsisteryi.github.io/bilibili-API-collect/docs/login/cookie_refresh.html
    public static class CookieManager
    {
        static CookieManager()
        {
            SetCookie(Config.Cookies, Config.RefreshToken);
        }

        private const string PublicKey = "-----BEGIN PUBLIC KEY----- MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71 nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40 JNrRuoEUXpabUzGB8QIDAQAB -----END PUBLIC KEY-----";

        private static Dictionary<string, string> CurrentCookieDict { get; set; } = [];

        private static string CurrentRefresh_csrf { get; set; } = "";

        private static string CurrentRefreshToken { get; set; } = "";

        private static string[] CookieFilter { get; set; } = ["Path", "Domain", "Expires", "HttpOnly", "Secure"];

        private static DateTime LastGetCookieTime { get; set; } = new();

        public static string? GetCurrentCookie()
        {
            if (LastGetCookieTime.Date != DateTime.Now.Date)
            {
                if (!UpdateCookie(false))
                {
                    LogHelper.Info("GetCurrentCookie", "更新Cookie失败，查看日志排查问题", false);
                    LastGetCookieTime = DateTime.Now;
                    return null;
                }
            }
            LastGetCookieTime = DateTime.Now;
            return BuildCookieFromDict(CurrentCookieDict);
        }

        public static void SetCookie(string cookie, string refreshToken)
        {
            if (string.IsNullOrEmpty(cookie) || string.IsNullOrEmpty(refreshToken) || !cookie.Contains("bili_jct="))
            {
                throw new ArgumentException("无效Token或RefreshToken，需要确保cookie中包含bili_jct。RefreshToken取自localStorage中的ac_time_value");
            }
            CurrentRefreshToken = refreshToken;
            CurrentCookieDict = [];
            foreach (var item in cookie.Split([';'], StringSplitOptions.RemoveEmptyEntries))
            {
                var c = item.Split('=');
                string key = c.First().Trim();
                if (CurrentCookieDict.ContainsKey(key))
                {
                    CurrentCookieDict[key] = c.Last().Trim();
                }
                else
                {
                    CurrentCookieDict.Add(c.First().Trim(), c.Last().Trim());
                }
            }
            CurrentRefresh_csrf = CurrentCookieDict["bili_jct"];

            Config.Cookies = cookie;
            Config.RefreshToken = refreshToken;
            Config.Instance.SetConfig("Cookies", cookie);
            Config.Instance.SetConfig("RefreshToken", refreshToken);
        }

        public static bool UpdateCookie(bool forced)
        {
            if ((CheckRefreshRequired(out bool success, out string timestamp) || forced) && success)
            {
                string correspondPath = GetCorrespondPath(timestamp);
                if (correspondPath != null
                    && GetRefreshCSRF(correspondPath, out var token)
                    && RefreshCookie(token, out var newToken))
                {
                    CurrentRefreshToken = newToken;
                    Config.Cookies = BuildCookieFromDict(CurrentCookieDict);
                    Config.RefreshToken = CurrentRefreshToken;
                    Config.Instance.SetConfig("Cookies", Config.Cookies);
                    Config.Instance.SetConfig("RefreshToken", Config.RefreshToken);
                    return true;
                }
                else
                {
                    LogHelper.Info("UpdateCookie", "UpdateCookie失败，详情见日志", false);
                }
            }
            else if (!success)
            {
                LogHelper.Info("UpdateCookie", "UpdateCookie失败，获取CheckRefreshRequired状态失败", false);
            }
            else if (success)
            {
                LogHelper.Info("UpdateCookie", "无需刷新Token");
                return true;
            }

            return false;
        }

        private static string BuildCookieFromDict(Dictionary<string, string> dict)
        {
            StringBuilder stringBuilder = new();
            foreach (var item in dict)
            {
                stringBuilder.Append($"{item.Key}={item.Value}; ");
            }
            return stringBuilder.ToString();
        }

        private static bool CheckRefreshRequired(out bool success, out string timestamp)
        {
            LogHelper.Info("CheckRefreshRequired", $"start");

            timestamp = "";
            string url = "https://passport.bilibili.com/x/passport-login/web/cookie/info";
            using WebClient webClient = new();
            try
            {
                webClient.Headers.Add("Cookie", BuildCookieFromDict(CurrentCookieDict));
                string response = webClient.DownloadString($"{url}?csrf={CurrentRefresh_csrf}");
                var json = JObject.Parse(response);
                Console.WriteLine(json);
                success = ((int)json["code"]) == 0;
                if (success)
                {
                    timestamp = json["data"]["timestamp"].ToString();
                    return ((bool)json["data"]["refresh"]);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Info("CheckRefreshRequired", $"{ex.Message}\n{ex.StackTrace}", false);
                success = false;
                return false;
            }
        }

        private static string GetCorrespondPath(string timestamp)
        {
            LogHelper.Info("GetCorrespondPath", $"timestamp={timestamp}");

            using RSA rsa = new RSACng();
            try
            {
                using var stringReader = new StringReader(PublicKey);
                var pemReader = new PemReader(stringReader);
                rsa.ImportParameters(DotNetUtilities.ToRSAParameters((RsaKeyParameters)pemReader.ReadObject()));
                byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes($"refresh_{timestamp}"), RSAEncryptionPadding.OaepSHA256);

                return BitConverter.ToString(encryptedData).Replace("-", "").ToLower();
            }
            catch (Exception ex)
            {
                LogHelper.Info("GetCorrespondPath", $"{ex.Message}\n{ex.StackTrace}", false);
                return null;
            }
        }

        private static bool GetRefreshCSRF(string correspondPath, out string? token)
        {
            LogHelper.Info("RefreshCSRF", $"correspondPath={correspondPath}");

            token = null;
            string url = $"https://www.bilibili.com/correspond/1/{correspondPath}";
            try
            {
                using WebClient webClient = new();
                webClient.Headers.Add("Cookie", BuildCookieFromDict(CurrentCookieDict));
                webClient.Headers.Add("Accept-Encoding", "gzip");
                var download = webClient.DownloadData(url);

                using MemoryStream stream = new(download);
                using GZipStream gzipStream = new(stream, CompressionMode.Decompress);
                using StreamReader reader = new(gzipStream, Encoding.UTF8);
                string html = reader.ReadToEnd();

                Regex regex = new("<div\\s+id=\"1-name\">\\s*(.*?)\\s*</div>");
                if (regex.Match(html).Success)
                {
                    token = regex.Match(html).Groups[1].Value;
                    LogHelper.Info("RefreshCSRF", $"Token={token}", true);
                    return true;
                }
                else
                {
                    LogHelper.Info("RefreshCSRF", $"未能从html中捕获到Token", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Info("RefreshCSRF", $"{ex.Message}\n{ex.StackTrace}", false);
                return false;
            }
        }

        private static bool RefreshCookie(string token, out string refreshToken)
        {
            LogHelper.Info("RefreshCookie", $"token={token}");
            string url = "https://passport.bilibili.com/x/passport-login/web/cookie/refresh";
            using WebClient webClient = new();
            webClient.Headers.Add("Cookie", BuildCookieFromDict(CurrentCookieDict));
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            NameValueCollection postData = new()
            {
                ["csrf"] = CurrentRefresh_csrf,
                ["refresh_csrf"] = token,
                ["source"] = "main_web",
                ["refresh_token"] = CurrentRefreshToken
            };
            refreshToken = "";
            try
            {
                string json = Encoding.UTF8.GetString(webClient.UploadValues(url, postData));
                var j = JObject.Parse(json);
                int code = ((int)j["code"]);
                if (code != 0)
                {
                    throw new WebException($"Request Error, code = {code}, msg = {j["message"]}");
                }
                refreshToken = j["data"]["refresh_token"].ToString();
                LogHelper.Info("RefreshCookie", $"Update RefreshToken={refreshToken}", false);

                foreach (var item in webClient.ResponseHeaders.AllKeys.Where(x => x == "Set-Cookie"))
                {
                    var cookieStrings = webClient.ResponseHeaders[item].Split(',');
                    foreach (var cookieString in cookieStrings)
                    {
                        var cookie = cookieString.Split(';');
                        foreach (var v in cookie.Where(x => x.Contains("=")))
                        {
                            string c = v.Trim();
                            string key = c.Split('=')[0];
                            string value = c.Split('=')[1];
                            if (CookieFilter.Any(x => x.ToLower() == key.ToLower()))
                            {
                                continue;
                            }
                            else if (CurrentCookieDict.ContainsKey(key))
                            {
                                CurrentCookieDict[key] = value;
                                LogHelper.Info("RefreshCookie", $"UpdateCookie, {key}={value}");
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Info("RefreshCookie", $"{ex.Message}\n{ex.StackTrace}", false);
                return false;
            }
        }
    }
}