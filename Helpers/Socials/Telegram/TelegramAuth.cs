using Ducky.Model;
using Ducky.ViewModel;
using Starksoft.Aspen.Proxy;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TeleSharp.TL.Auth;
using TLSharp.Core;

namespace Ducky.Helpers.Socials.Telegram
{
    public class TelegramAuth
    {
        private static FileSessionStore store = new FileSessionStore();
        private ImagesHelper imghlp = new ImagesHelper();
        private Session session;
        private TelegramClient client;

        private LogsPageViewModel log = MainWindowViewModel.GetLogVM();
        private ProxyHelper ph = MainWindowViewModel.GetProxyHelper();
        private TelegramBot telebot = MainWindowViewModel.GetTelebot();

        private string hash { get; set; }
        private string CurrectPhone { get; set; }
        private string testproxy { get; set; }

        private TcpClient TestTcpHandler(string address, int port)
        {
            var proxy = testproxy.Split(';');
            if (proxy[0] == "Socks5")
            {
                Socks5ProxyClient SocksProxyClient;
                if (proxy[3] == "")
                    SocksProxyClient = new Socks5ProxyClient(proxy[1], Int32.Parse(proxy[2]));
                else
                    SocksProxyClient = new Socks5ProxyClient(proxy[1], Int32.Parse(proxy[2]), proxy[3], proxy[4]);

                try { TcpClient client = SocksProxyClient.CreateConnection(address, port); return client; }
                catch { return null; }
            }
            else
            {
                HttpProxyClient webProxyClient;
                if (proxy[3] == "")
                    webProxyClient = new HttpProxyClient(proxy[1], Int32.Parse(proxy[2]));
                else
                    webProxyClient = new HttpProxyClient(proxy[1], Int32.Parse(proxy[2]), proxy[3], proxy[4]);

                try
                {
                    TcpClient client = webProxyClient.CreateConnection(address, port);
                    return client;
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<bool> PhoneCheck(string phone)
        {
            if (InternetConnection.IsConnectionExist == false)
            {
                MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram", "Возможно отсутствует подключение к интернету. Статус интернет подключение : " + InternetConnection.IsConnectionExist);
                return false;
            }
            else
            {
                if (String.IsNullOrEmpty(phone) == true)
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram", "Номер телефона не введен. Пожалуйста введите номер.");
                else
                {
                    try
                    {
                        client = await CreateClientAsync();
                        if (client != null)
                        {
                            await client.ConnectAsync();
                            if (client.IsConnected)
                            {
                                hash = await client.SendCodeRequestAsync(phone);
                                CurrectPhone = phone;
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram", "Ошибка входа. " + ex.Message);
                        return false;
                    }
                }
                return false;
            }
        }
        public async Task<TelegramUserModel> CodeCheck(string code)
        {
            try
            {
                var user = await client.MakeAuthAsync(CurrectPhone, hash, code);

                if (user != null)
                {
                    Properties.Socials.Default.TelegramUserID = (ulong)user.Id;
                    Properties.Settings.Default.Save();
                    var telegramuser = (new TelegramUserModel
                    {
                        UserID = user.Id.ToString(),
                        UserNameAndSurname = user.FirstName + " " + user.LastName,
                        UserNickName = user.Username,
                        UserPhoto = imghlp.byteArrayToImage(user.Photo.Serialize())
                    });
                    telebot.SendAuthMessage();
                    return telegramuser;
                }
                else
                {
                    MainWindowViewModel.instance.Alert(null, "Telegram", "Не удалось загрузить данные профиля...");
                    return null;
                }
            }
            catch (Exception e)
            {
                MainWindowViewModel.instance.Alert(null, "Telegram", "Ошибка: " + e.Message.ToString());
                return null;
            }
        }
        public TelegramUserModel IsUserAuth()
        {
            if (Properties.Socials.Default.TelegramUserID != 0)
            {
                try
                {
                    var loadedSession = store.Load("session");
                    session = Session.TryLoadOrCreateNew(store, Properties.Socials.Default.TelegramUserID.ToString());

                    var ph = imghlp.byteArrayToImage(loadedSession.TLUser.Photo.Serialize());
                    TelegramUserModel telegramuser = (new TelegramUserModel
                    {
                        UserID = loadedSession.TLUser.Id.ToString(),
                        UserNameAndSurname = loadedSession.TLUser.FirstName + " " + loadedSession.TLUser.LastName,
                        UserNickName = loadedSession.TLUser.Username,
                        UserPhoto = ph
                    }) ;
                    return telegramuser;
                }
                catch { return null; }
            }
            else return null;
        }
    
        private async Task<TelegramClient> CreateClientAsync()
        {
            client = null;
            return await Task.Run(() =>
            {
                try
                {
                    if (Properties.Settings.Default.IsProxy)
                        return new TelegramClient(Properties.Socials.Default.TelegramApiID, Properties.Socials.Default.TelegramApiHash, store, "session", handler: ph.TcpHandler);
                    else
                        return new TelegramClient(Properties.Socials.Default.TelegramApiID, Properties.Socials.Default.TelegramApiHash, store, "session");
                }
                catch(Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    MainWindowViewModel.instance.Alert(null, "Соединение", "Не удалось подключится к серверу Telegram.");
                    return null;
                }
            });
        }
        public async Task<bool> CreateConnection()
        {
            if (InternetConnection.IsConnectionExist == false)
            {
                MainWindowViewModel.instance.Alert(null, "Telegram", "Возможно отсутствует подключение к интернету. Статус интернет подключение : " + InternetConnection.IsConnectionExist);
                return false;
            }
            else
            {
                client = await CreateClientAsync();
                if (client != null)
                {
                    await client.ConnectAsync();
                    var connect = client.IsConnected;
                    return connect;
                }else return false;
            }
        }
        public async Task LogOut()
        {
            try
            {
                Properties.Socials.Default.TelegramUserID = 0;
                Properties.Socials.Default.Save();
                var LogOut = new TLRequestLogOut();
                bool lo = await client.SendRequestAsync<Boolean>(LogOut);
                client.Dispose();
                string dic = Directory.GetCurrentDirectory();
                File.Delete(dic + "session.dat");

            }catch(Exception e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
            }

        }

        public async Task<bool> TryToConnect(string proxy)
        {
            testproxy = proxy;
            if (InternetConnection.IsConnectionExist == false)
            {
                MainWindowViewModel.instance.Alert(null, "Telegram", "Возможно отсутствует подключение к интернету. Статус интернет подключение : " + InternetConnection.IsConnectionExist);
                return false;
            }
            else
            {
                try
                {
                    TelegramClient testclient = await CreateTestClientAsync();
                    await testclient.ConnectAsync(false);
                    await testclient.ConnectAsync();
                    var connect = testclient.IsConnected;
                    testclient.Dispose();
                    return connect;
                }
                catch(Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    return false;
                }
            }
        }
        private async Task<TelegramClient> CreateTestClientAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return new TelegramClient(Properties.Socials.Default.TelegramApiID, Properties.Socials.Default.TelegramApiHash, handler: TestTcpHandler);
                }
                catch(Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    return null;
                }
            });
        }

    }
}
