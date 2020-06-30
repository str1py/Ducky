using Ducky.Model;
using Ducky.ViewModel;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace Ducky.Helpers.Socials.Vk
{
    public class VkAuth
    {
        //92f60ab281fa2e9dc6e73316dc0b432a215395c5806a5f144e7358ad7d9e110d4543209c4a10039dda138 - Api KEy Vk Ducku_bot
        //define('VK_API_VERSION', '5.67'); //Используемая версия API
        //define('VK_API_ENDPOINT', "https://api.vk.com/method/");

        //BOT
        private readonly MainPageViewModel mp = MainWindowViewModel.GetMainVM();
        private readonly LogsPageViewModel log = MainWindowViewModel.GetLogVM();
        private bool AuthUsing2FA { get; set; } = false;
        [JsonIgnore]
        private Func<string> Auth2Fa { get; set; }
        private VkApi user = new VkApi();
        public static VkApi bot = new VkApi();

        public VkAuth()
        {
            CreateBot();
        }

        public async Task<VkUserModel> Auth(string login, string password)
        {
            try
            {
                await user.AuthorizeAsync(new ApiAuthParams
                {
                    ApplicationId = Properties.Socials.Default.VkAppId,
                    Login = login,
                    Password = password,
                    Settings = Settings.All,
                    TwoFactorAuthorization = (this.AuthUsing2FA) ? Auth2Fa : default(Func<string>)
                });

                if (user.IsAuthorized)
                {
                    var info = await user.Account.GetProfileInfoAsync();
                    var photo = user.Photo.Get(new PhotoGetParams
                    {
                        AlbumId = PhotoAlbumType.Profile,
                        Reversed = true,
                        Count = 1,
                    }) ;

                    var bitmapPhoto = new BitmapImage();
                    bitmapPhoto.BeginInit();
                    bitmapPhoto.UriSource = photo[0].Sizes[0].Url; 
                    bitmapPhoto.EndInit();

                    var vkuser = (new VkUserModel
                    {
                        userId = (int)user.UserId,
                        photo = bitmapPhoto,
                        userToken = user.Token,
                        userName = info.FirstName,
                        userSurname = info.LastName,
                        screenName = info.ScreenName,
                        FullName = $"{info.FirstName} {info.LastName}"
                    });

                    user.OnTokenExpires += user => user.RefreshToken();

                    Properties.Socials.Default.VkUserId = (ulong)user.UserId;
                    Properties.Settings.Default.Save();
                    SendAnswerMessage("Смотрю вы подключили Ducky к VK! Буду рад помогать, кря!", vkuser.userId);
                    return vkuser;
                }
                else
                    return null;

            }
            catch(Exception e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                return null; }  
        }

        private void CreateBot()
        {
            if (Properties.Socials.Default.VkUserId != 0)
            {
                try { bot.Authorize(new ApiAuthParams() { AccessToken = Properties.Socials.Default.VkAccessKey }); }
                catch { log.AddLog("Create bot", "FAILED"); }
            }
        }
    
        public async Task BotStartMessageRecive()
        {
            try
            {
                if (bot == null)
                {
                    CreateBot();
                    log.AddLog("VK message recieve", "START");
                    await Task.Run(() =>
                    {
                        while (Properties.Socials.Default.VkUserId != 0 && InternetConnection.IsConnectionExist)
                        {
                            try
                            {
                                var s = bot.Groups.GetLongPollServer(Properties.Socials.Default.VkGroupId);
                                var poll = bot.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                                {
                                    Server = s.Server,
                                    Ts = s.Ts,
                                    Key = s.Key,
                                    Wait = 25
                                });
                                if (poll?.Updates == null) continue;
                                foreach (var a in poll.Updates)
                                {
                                    if (a.Type == GroupUpdateType.MessageNew && a.Message.UserId == (long?)Properties.Socials.Default.VkUserId)
                                    {
                                        SendAnswerMessage(a.Message.Body.ToLower(), a.Message.UserId);
                                    }
                                }
                            }
                            catch
                            {
                                log.AddLog("Vk message recieve", "STOP");
                            }
                        }
                    });
                }
                else
                {
                    log.AddLog("VK message recieve", "START");
                    await Task.Run(() =>
                    {
                        while (Properties.Socials.Default.VkUserId != 0 && InternetConnection.IsConnectionExist)
                        {
                            try
                            {
                                var s = bot.Groups.GetLongPollServer(Properties.Socials.Default.VkGroupId);
                                var poll = bot.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                                {
                                    Server = s.Server,
                                    Ts = s.Ts,
                                    Key = s.Key,
                                    Wait = 25
                                });
                                if (poll?.Updates == null) continue;
                                foreach (var a in poll.Updates)
                                {
                                    if (a.Type == GroupUpdateType.MessageNew && a.Message.UserId == (long?)Properties.Socials.Default.VkUserId)
                                    {
                                        SendAnswerMessage(a.Message.Body.ToLower(), a.Message.UserId);
                                    }
                                }
                            }
                            catch
                            {
                                log.AddLog("Vk message recieve", "STOP");
                            }
                        }
                    });
                }
            }catch(Exception e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
            }
           
        }

        public void SendAnswerMessage(string message, long? userID)
        {
            string answer = "";
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                answer = mp.SendFromVk(message);
            });
            Random rnd = new Random();
            bot.Messages.Send(new MessagesSendParams
            {
                RandomId = rnd.Next(),
                UserId = userID,
                Message = answer
            });
        }
    } 
}
