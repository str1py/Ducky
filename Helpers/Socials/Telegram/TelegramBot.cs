using Ducky.ViewModel;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Ducky.Helpers.Socials.Telegram
{
    public class TelegramBot
    {
        private TelegramBotClient Bot;

        private readonly MainPageViewModel mp;
        private readonly PlayerPageViewModel player;
        private readonly LogsPageViewModel log;
        private readonly ProxyHelper ph;

        string callbackData;

        public TelegramBot()
        {
            log = MainWindowViewModel.GetLogVM();
            mp = MainWindowViewModel.GetMainVM();
            player = MainWindowViewModel.GetPlayerVM();
            ph = MainWindowViewModel.GetProxyHelper();

            //CreateConnection();
            ////NULL EXCEPTION
            //if (Bot != null)
            //{
            //    Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
            //    Bot.OnMessage += BotOnMessageReceived;
            //    Bot.OnMessageEdited += BotOnMessageReceived;
            //}
        }

        public async void CreateConnection()
        {
            if (Properties.Socials.Default.TelegramUserID != 0)
            {
                Bot = null;
                if (Properties.Settings.Default.IsProxy == true)
                {
                    var proxysplit = Properties.Settings.Default.ProxyInUse.Split(';');
                    try
                    {
                        if (proxysplit[0] == "Socks5")
                            Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken, ph.Socks5Proxy());
                        else
                            Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken, ph.HttpProxy());

                        var me = await Bot.GetMeAsync();
                        Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
                        Bot.OnMessage += BotOnMessageReceived;
                        Bot.OnMessageEdited += BotOnMessageReceived;
                        Bot.StartReceiving();
                    }
                    catch (Exception e)
                    {
                        MethodBase m = MethodBase.GetCurrentMethod();
                        log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                        Bot = null;
                        MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Rasources/Icons/telegram.png")), "Telegram Bot", "Не подключен. Провертьте настройки прокси");
                    }
                }
                else
                {
                    try { Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken); }
                    catch { MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram Bot", "Не подключен. Провертьте настройки прокси"); }
                }
            }
        }
        public async Task<bool> TryToConnect(string proxy)
        {
            Bot = null;
            if (Properties.Settings.Default.IsProxy == true)
            {
                var proxysplit = proxy.Split(';');
                try
                {
                    if (proxysplit[0] == "Socks5")
                        Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken, ph.TestSocks5Proxy(proxy));
                    else
                        Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken, ph.TestHttpProxy(proxy));

                    var me = await Bot.GetMeAsync();
                    Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
                    Bot.OnMessage += BotOnMessageReceived;
                    Bot.OnMessageEdited += BotOnMessageReceived;
                    return true;
                }
                catch(Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    Bot = null;
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram Bot", "Не подключен. Провертьте настройки прокси");
                    return false;
                }
            }
            else
            {
                try
                {
                    Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken);
                    var me = await Bot.GetMeAsync();
                    return true;
                }
                catch(Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram Bot", "Не подключен. Провертьте настройки прокси");
                    return false;
                }
            }
        }
        public async Task<bool> TryToConnect()
        {
            Bot = null;
            if (Properties.Settings.Default.IsProxy == true)
            {
                var proxysplit = Properties.Settings.Default.ProxyInUse.Split(';');
                try
                {
                    if (proxysplit[0] == "Socks5")
                        Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken, ph.Socks5Proxy());
                    else
                        Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken, ph.HttpProxy());

                    var me = await Bot.GetMeAsync();
                    Bot.OnCallbackQuery += BotOnCallbackQueryReceived;
                    Bot.OnMessage += BotOnMessageReceived;
                    Bot.OnMessageEdited += BotOnMessageReceived;
                    Bot.StartReceiving();
                    return true;
                }
                catch (Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    Bot = null;
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram Bot", "Не подключен. Провертьте настройки прокси");
                    return false;
                }
            }
            else
            {
                try
                {
                    Bot = new TelegramBotClient(Properties.Socials.Default.TwitchBotToken);
                    var me = await Bot.GetMeAsync();
                    return true;
                }
                catch (Exception e)
                {
                    MethodBase m = MethodBase.GetCurrentMethod();
                    log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Icons/telegram.png")), "Telegram Bot", "Не подключен. Провертьте настройки прокси");
                    return false;
                }
            }
        }
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            try
            {
                var message = messageEventArgs.Message;
                if (message == null || message.Type != MessageType.Text) return;
                await Bot.SendChatActionAsync(Properties.Socials.Default.TelegramUserID.ToString(), ChatAction.Typing);

                var usermessage = message.Text; //Cообщение от пользователя 
                string answer = "";

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    answer = mp.SendFromTelegram(usermessage);
                });


                await Task.Delay(500);
                await Bot.SendTextMessageAsync(Properties.Socials.Default.TelegramUserID.ToString(), answer);//получение ответа и отправка ответа в TELEGRAM
            }
            catch(Exception e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
            } 
        }
        private async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            callbackData = callbackQueryEventArgs.CallbackQuery.Data;
            var from = callbackQueryEventArgs.CallbackQuery.Message.Text;
            if (from == "Управление плеером")
                PlayerAction(callbackData);

            if (callbackData.Contains("twitch.tv"))
                Process.Start("chrome.exe", callbackData);
            
            await Task.Delay(500);
            await Bot.AnswerCallbackQueryAsync(callbackQueryEventArgs.CallbackQuery.Id,
              $"");
        }

        public async void SendAuthMessage()
        {
            if(Bot != null)
                await Bot.SendTextMessageAsync(Properties.Socials.Default.TelegramUserID.ToString(), "Опачки, Ducky уже тут, кстати");//получение ответа и отправка ответа в TELEGRAM
        }

        #region Player
        public async void ShowPlayerButtons(object sender, MessageEventArgs messageargs)
        {
            await Bot.SendChatActionAsync(Properties.Socials.Default.TelegramUserID.ToString(), ChatAction.Typing);
            var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new[] // first row
                    {
                        InlineKeyboardButton.WithCallbackData("⏮"),
                        InlineKeyboardButton.WithCallbackData("⏯"),
                        InlineKeyboardButton.WithCallbackData("⏭"),
                    },
                    new [] // second row
                    {
                        InlineKeyboardButton.WithCallbackData("-"),
                        InlineKeyboardButton.WithCallbackData("+"),
                    }
                });
            await Bot.SendTextMessageAsync(Properties.Socials.Default.TelegramUserID.ToString(), "Управление плеером",
            replyMarkup: keyboard);
        }
        private void PlayerAction(string action)
        {
            switch (action)
            {
                case "⏮":
                    App.Current.Dispatcher.Invoke((Action)delegate { player.Previous(); });
                    break;
                case "⏯":
                    App.Current.Dispatcher.Invoke((Action)delegate { player.PlayPause(); });
                    break;
                case "⏭":
                    App.Current.Dispatcher.Invoke((Action)delegate { player.Next(); });
                    break;
                case "+":
                    App.Current.Dispatcher.Invoke((Action)delegate { player.IncreaseVolume(); });
                    break;
                case "-":
                    App.Current.Dispatcher.Invoke((Action)delegate { player.DicreaseVolume(); });
                    break;
            }
        }
        #endregion

        #region Twitch
        public async void StreamStatusChangedAlert(object sender, MessageEventArgs messageargs,string streamer, string status, string link)
        {
            try
            {
                if (Bot != null)
                {
                    await Bot.SendChatActionAsync(Properties.Socials.Default.TelegramUserID.ToString(), ChatAction.Typing);
                    var keyboard = new InlineKeyboardMarkup(new[]
                       {
                 new[] // first row
                    {
                        InlineKeyboardButton.WithUrl("Начать просмотр на устройстве",link)
                    },
                    new [] // second row
                    {
                        InlineKeyboardButton.WithCallbackData("Начат просмотр на компьютере", link)
                    }
             });
                    await Bot.SendTextMessageAsync(Properties.Socials.Default.TelegramUserID.ToString(), $"{streamer} {status}. {link}",
                    replyMarkup: keyboard);
                }
            }catch(Exception e)
            {
                log.AddLog("ERROR:", e.Message);
            }
        }
        #endregion

    }
}
