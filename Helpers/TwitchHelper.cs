using Ducky.Helpers.Socials.Telegram;
using Ducky.Model;
using Ducky.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ducky.Helpers
{
    public class TwitchHelper
    {
        private string logoutReqLink = "https://id.twitch.tv/oauth2/revoke";
        private string refresgLink = "https://id.twitch.tv/oauth2/token?grant_type=refresh_token";//--data-urlencode
        private string updateStreamLink = "https://api.twitch.tv/kraken/streams/";
        private string tokenLink = "https://id.twitch.tv/oauth2/token";
        private string userLink = "https://api.twitch.tv/kraken/user/";
        private string followLink = "https://api.twitch.tv/kraken/users/";

        //using System.Runtime.InteropServices;
        [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        private TwitchUserModel twuser;
        private ImagesHelper imghlp = new ImagesHelper();
        private ObservableCollection<TwitchChannelModel> followed;
        private TelegramBot tb;
        private LogsPageViewModel log;
        private string clientId = $"?client_id={Properties.Socials.Default.TwitchAppClientID}";
        private string clientSecret = $"&client_secret={Properties.Socials.Default.TwitchAppClientSecret}";
        private string grant = "&grant_type=authorization_code";
        private string reditect = "&redirect_uri=http://localhost";

        public TwitchHelper()
        {
            followed = new ObservableCollection<TwitchChannelModel>();
            tb = MainWindowViewModel.GetTelebot();
            log = MainWindowViewModel.GetLogVM();
        }

        public async Task<string> GetCodeFromCallBack()
        {
            WebBrowser webBrowser = WebBrowserExtension.GetWebBrowser();
            string str;
            do
            {
                await Task.Delay(1000);
                str = webBrowser.Source?.ToString() ?? "";
                if (str.Contains("http://localhost/auth_cancle"))
                    break;

            } while (!str.Contains("http://localhost/?code"));

            if (str.Contains("http://localhost/?code"))
            {
                string[] strsplit = str.Split('=');
                string[] strcode = strsplit[1].Split('&');
                return strcode[0];
            }
            else
                return "";
        }
        public async Task<TwitchUserModel> GetUserInfoAsync(string code)
        {
            var json = await GetTokenAsync(code);
            return await GetUser(json);
        }
        public async Task<ObservableCollection<TwitchChannelModel>> GetFollows()
        {
            await GetFollowChannels();
            GetSubsChanels(followed);
            return await GetChannelStatus(followed);
        }

        public async Task<TwitchChannelModel> UpdateStreamInfo(TwitchChannelModel stream)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var str = await SendGetRequest($"{updateStreamLink}{stream._id}", false);
                    var o = JObject.Parse(str);
                    string streamStatusString = "";
                    string prevStatus = stream.channel_status;

                    try { streamStatusString = (string)o["stream"]["stream_type"]; }
                    catch { streamStatusString = (string)o["stream"]; }

                    if (streamStatusString == null)
                    {
                        stream.channel_status = "offline";
                        stream.viewers = "";
                        stream.dotcolor = "Transpatent";
                    }
                    else if (streamStatusString == "rerun") //Если повтор
                    {
                        stream.channel_status = "rerun";
                        stream.viewers = (string)o["stream"]["viewers"];
                        stream.game = (string)o["stream"]["channel"]["game"];
                        stream.dotcolor = "#504b57";
                    }
                    else if (streamStatusString == "live") //ЕСЛИ ОНЛАЙН
                    {
                        stream.channel_status = "live";
                        stream.livetime = GetStreamLength((DateTime)o["stream"]["created_at"]);
                        stream.viewers = (string)o["stream"]["viewers"];
                        stream.game = (string)o["stream"]["channel"]["game"];
                        stream.dotcolor = "Red";
                    }

                    //ALERTS
                    if (prevStatus != streamStatusString)
                    {
                        if (streamStatusString == "live")
                        {
                            MainWindowViewModel.instance.Alert(stream.logo_image, "Twitch", $"{stream.display_name} начал прямой эфир /n {stream.broadcast_discr}");
                            tb.StreamStatusChangedAlert(this, null, stream.display_name, "начал прямой эфир", stream.url);
                        }
                        else if (streamStatusString == "rerun")
                        {
                            MainWindowViewModel.instance.Alert(stream.logo_image, "Twitch", $"{stream.display_name} запустил повтор /n {stream.broadcast_discr}");
                            tb.StreamStatusChangedAlert(this, null, stream.display_name, "начал повтор", stream.url);
                        }
                    }

                    return stream;
                }
            }
            catch (Exception e)
            {
                stream.channel_status = "unavalible";
                stream.viewers = "";
                stream.game = "";
                stream.dotcolor = "";
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                return stream;
            }
        }
        public async void Logout()
        {
            twuser = null;
       
            string post = $"{logoutReqLink}{clientId}&token={Properties.Socials.Default.TwitchUserAccess_token}"; 
            var request = WebRequest.Create(post);
            request.ContentType = "application/json";
            request.Method = "POST";
            var response = await GetResponseAsync(request);
            Properties.Socials.Default.TwitchUserAccess_token = "";
            Properties.Socials.Default.TwitchUserRefresh_token = "";
            Properties.Socials.Default.Save();
            followed.Clear();
            WebBrowserClear();
        }

        private void WebBrowserClear()
        {
            WebBrowser webBrowser = WebBrowserExtension.GetWebBrowser();
            
            var ptr = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(ptr, 3);
            InternetSetOption(IntPtr.Zero, 81, ptr, 4);
            Marshal.Release(ptr);
        }

        public async Task<TwitchUserModel> IsAuth()
        {
            if (Properties.Socials.Default.TwitchUserAccess_token != "")
            {
                await RefreshTokenAsync();
                if (Properties.Socials.Default.TwitchUserAccess_token != "")
                {
                    return await GetUser();
                }
                else
                    return null;
            }
            else return null;
        }

        private async Task<JObject> GetTokenAsync(string codefromrequest)
        {
            var code = $"&code={codefromrequest}";
            string post = tokenLink + clientId + clientSecret + code + grant + reditect;

            var request = WebRequest.Create(post);
            request.ContentType = "application/json";
            request.Method = "POST";

            var response = await GetResponseAsync(request);
            if (response == null)
            {
                WebBrowserClear();
                return null;
            }
            else
                return JObject.Parse(response);
        }
        private async Task<TwitchUserModel> GetUser(JObject json)
        {
            if (json != null)
            {
                Properties.Socials.Default.TwitchUserAccess_token = (string)json["access_token"];
                Properties.Socials.Default.TwitchUserRefresh_token = (string)json["refresh_token"];
                Properties.Socials.Default.Save();

                var user = await SendGetRequest(userLink, true);
                var userparsed = JObject.Parse(user);

                twuser = (new TwitchUserModel
                {
                    UserId = (string)userparsed["_id"],
                    UserName = (string)userparsed["display_name"],
                    UserImage = imghlp.GetImage((string)userparsed["logo"]),
                    // UserImage = (string)userparsed["logo"],
                    email = (string)userparsed["email"],
                    Partnered = (bool)userparsed["partnered"],
                    Type = (string)userparsed["type"]
                });
                return twuser;
            }
            else
                return null;
        }
        private async Task<TwitchUserModel> GetUser()
        {
            if (Properties.Socials.Default.TwitchUserAccess_token != "")
            {
                var user = await SendGetRequest(userLink, true);

                if (user != null)
                {
                    var userparsed = JObject.Parse(user);
                    twuser = (new TwitchUserModel
                    {
                        UserId = (string)userparsed["_id"],
                        UserName = (string)userparsed["display_name"],
                        UserImage = imghlp.GetImage((string)userparsed["logo"]),
                        // UserImage = (string)userparsed["logo"],
                        email = (string)userparsed["email"],
                        Partnered = (bool)userparsed["partnered"],
                        Type = (string)userparsed["type"]
                    });
                    return twuser;
                }
                else return null;
            }
            else
                return null;
        }
        private async Task GetFollowChannels()
        {
            try
            {
                string uri = $"{followLink}{twuser.UserId}/follows/channels?limit=100";
                var followchannels = await SendGetRequest(uri, false);
                var foll = JObject.Parse(followchannels);
                int count = (int)foll["_total"];

                for (int i = 0; i < count; i++)
                {
                    Visibility isPartner;

                    if ((bool)foll["follows"][i]["channel"]["partner"] == true)
                        isPartner = Visibility.Visible;
                    else
                        isPartner = Visibility.Hidden;

                    var channel = (new TwitchChannelModel
                    {
                        _id = (string)foll["follows"][i]["channel"]["_id"],
                        display_name = (string)foll["follows"][i]["channel"]["display_name"],
                        url = (string)foll["follows"][i]["channel"]["url"],
                        logo = (string)foll["follows"][i]["channel"]["logo"],
                        partner = isPartner,
                        channel_status = "Offline",
                        broadcast_discr = (string)foll["follows"][i]["channel"]["status"],
                        game = (string)foll["follows"][i]["channel"]["game"],
                        viewers = "",
                        issub = Visibility.Hidden,
                        dotcolor = ""
                    });
                    followed.Add(channel);

                }
            }
            catch { }
        }      

        private async void GetSubsChanels(ObservableCollection<TwitchChannelModel> followlist) 
        {
            try
            {
                foreach (TwitchChannelModel channel in followlist) //EXCEPTION
                {
                    try
                    {
                        // GET https://api.twitch.tv/kraken/users/<user ID>/subscriptions/<channel ID>
                        string uri = $"{followLink}{twuser.UserId}/subscriptions/{channel._id}";
                        var resp = await SendGetRequest(uri, true);
                        var sub = JObject.Parse(resp);
                        channel.issub = Visibility.Visible;
                        string subb = (string)sub["sub_plan"];
                        channel.subTier = "Подписка уровня " + subb.Substring(0, 1);
                        log.AddLog("Подписка уровня ", subb.Substring(0, 1), "на канал ", channel.display_name);
                    }
                    catch (Exception e)
                    {
                        MethodBase m = MethodBase.GetCurrentMethod();
                        log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                        channel.issub = Visibility.Hidden;
                        channel.subTier = "0";
                    }
                }
            }
            catch { }
        }
        private async Task<ObservableCollection<TwitchChannelModel>> GetChannelStatus(ObservableCollection<TwitchChannelModel> list)
        {
            foreach (TwitchChannelModel twitch in list)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var str = await SendGetRequest($"https://api.twitch.tv/kraken/streams/{twitch._id}", false);
                        var o = JObject.Parse(str);
                        string streamStatusString = "";

                        try { streamStatusString = (string)o["stream"]["stream_type"]; }
                        catch { streamStatusString = (string)o["stream"]; }

                        if (streamStatusString == "" || String.IsNullOrEmpty(streamStatusString))
                        {
                            twitch.channel_status = "offline";
                            twitch.viewers = "";
                            twitch.dotcolor = "Transpatent";
                        }
                        else if (streamStatusString == "rerun") //Если повтор
                        {
                            twitch.channel_status = "rerun";
                            twitch.viewers = (string)o["stream"]["viewers"];
                            twitch.game = (string)o["stream"]["channel"]["game"];
                            twitch.dotcolor = "#504b57";
                            
                        }
                        else if (streamStatusString == "live") //ЕСЛИ ОНЛАЙН
                        {
                            DateTime s = (DateTime)o["stream"]["created_at"];
                            twitch.channel_status = "live";
                            twitch.livetime = GetStreamLength(s);
                            twitch.viewers = (string)o["stream"]["viewers"];
                            twitch.game = (string)o["stream"]["channel"]["game"];
                            twitch.dotcolor = "Red";
                        }
                    }
                }
                catch(Exception e)
                {
                    twitch.channel_status = "unavalible";
                    twitch.viewers = "";
                    twitch.game = "";
                    twitch.dotcolor = "";
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                }
            }
            return list;
        }
 
        private async Task<string> SendGetRequest(string link, bool isAuthNeed)
        {
            try
            {
                HttpWebRequest webRequest = CreateGetRequest(link, isAuthNeed);
                return await GetResponseAsync(webRequest);
            }
            catch(Exception e)
            {
                await RefreshTokenAsync();
                HttpWebRequest webRequest = CreateGetRequest(link, isAuthNeed);
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                return await GetResponseAsync(webRequest);
            }

        }     
        private HttpWebRequest CreateGetRequest(string link, bool isAuthNeed)
        {
            HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(link);
            webRequest.Method = "GET";
            webRequest.Accept = "application/vnd.twitchtv.v5+json";
            webRequest.Headers.Add("Client-ID", Properties.Socials.Default.TwitchAppClientID);
            if (isAuthNeed)
                webRequest.Headers.Add("Authorization", $"OAuth {Properties.Socials.Default.TwitchUserAccess_token}");
            return webRequest;
        }
        private async Task<string> GetResponseAsync(WebRequest request)
        {
            try
            {
                HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                return null;
            }
        }

        private async Task RefreshTokenAsync()
        {
            string post = $"{refresgLink}&refresh_token={Properties.Socials.Default.TwitchUserRefresh_token}&client_id={Properties.Socials.Default.TwitchAppClientID}&client_secret={Properties.Socials.Default.TwitchAppClientSecret}";

            var request = WebRequest.Create(post);
            request.ContentType = "application/json";
            request.Method = "POST";

            var response = await GetResponseAsync(request);
            if (response != null)
            {
                var result = JObject.Parse(response);
                Properties.Socials.Default.TwitchUserAccess_token = (string)result["access_token"];
                Properties.Socials.Default.TwitchUserRefresh_token = (string)result["refresh_token"];
                Properties.Socials.Default.Save();
            }
            else
            {
                Properties.Socials.Default.TwitchUserAccess_token = "";
                Properties.Socials.Default.TwitchUserRefresh_token = "";
                Properties.Socials.Default.Save();
            }
        }

        private string GetStreamLength(DateTime date)
        {
            // "created_at": "2019-11-09T16:00:11Z",

            var s = DateTime.UtcNow - date;
           
            string hours;
            string legth;

            if (s.Hours < 10)
                hours = "0" + s.Hours;

            if (s.Days != 0)
                legth = $"{s.Days}d {s.Hours}h {s.Minutes}m";
            else if(s.Hours != 0)
                legth = $"{s.Hours}h {s.Minutes}m";
            else
                legth = $"{s.Minutes}m";

            return legth;
        }
    }
}
