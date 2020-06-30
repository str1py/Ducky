using Ducky.Helpers;
using Ducky.Helpers.Socials.Telegram;
using Ducky.Helpers.Socials.Vk;
using Ducky.Model;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Ducky.ViewModel
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        public static MainWindowViewModel instance;
        public ObservableCollection<AlertModel> alerts = new ObservableCollection<AlertModel>();
        #region fields
        private static LogsPageViewModel Log;
        private static MainPageViewModel Main;
        private static PlayerPageViewModel Player;
        private static TwitchPageViewModel TwitchPage;
        private static SettingsPageViewModel Settings;
        private static RadioPageViewModel Radio;

        private static BassNetHelper BassHelper;
        private static TelegramAuth Telegram;
        private static TwitchHelper TwitchHelp;
        private static TelegramBot Telebot;
        private static ProxyHelper Proxy;
        private static InternetConnection Internet;
        private static VkAuth Vk;
        public static WindowState winstate;
        #endregion

        public MainWindowViewModel()
        {
            // InstalledApps ia = new InstalledApps();
            #region Init
            instance = this;
            BassHelper = BassHelper ?? new BassNetHelper();
            Log = Log ?? new LogsPageViewModel();
            Player = Player ?? new PlayerPageViewModel();
            Radio = Radio ?? new RadioPageViewModel();
            Main = Main ?? new MainPageViewModel();
            Settings = Settings ?? new SettingsPageViewModel();
            Telegram = Telegram ?? new TelegramAuth();
            TwitchPage = TwitchPage ?? new TwitchPageViewModel();
            Telebot = Telebot ?? new TelegramBot();
            Proxy = Proxy ?? new ProxyHelper();
            Internet = Internet ?? new InternetConnection();
            Vk = Vk ?? new VkAuth();
            #endregion

            SelectedViewModel = Main;
            IsAlertOn = "null";
        }

        #region Instances return
        public static PlayerPageViewModel GetPlayerVM()
        {
            if (Player == null)
                Player = new PlayerPageViewModel();
            return Player;
        }
        public static RadioPageViewModel GetRadio()
        {
            if (Radio == null)
                Radio = new RadioPageViewModel();
            return Radio;
        }
        public static LogsPageViewModel GetLogVM()
        {
            if (Log == null)
                Log = new LogsPageViewModel();
            return Log;
        }
        public static MainPageViewModel GetMainVM()
        {
            if (Main == null)
                Main = new MainPageViewModel();
            return Main;
        }
        public static SettingsPageViewModel GetSettVM()
        {
            if (Settings == null)
                Settings = new SettingsPageViewModel();
            return Settings;
        }
        public static TwitchPageViewModel GetTwitchVM()
        {
            if (TwitchPage == null)
                TwitchPage = new TwitchPageViewModel();
            return TwitchPage;
        }
        public static BassNetHelper GetBassNetHelper()
        {
            if (BassHelper == null)
                BassHelper = new BassNetHelper();
            return BassHelper;
        }

        public static TelegramAuth GetTelegram()
        {
            if (Telegram == null)
                Telegram = new TelegramAuth();
            return Telegram;
        }
        public static TelegramBot GetTelebot()
        {
            if (Telebot == null)
                Telebot = new TelegramBot();
            return Telebot;
        }
        public static TwitchHelper GetTwitchHelper()
        {
            if (TwitchHelp == null)
                TwitchHelp = new TwitchHelper();
            return TwitchHelp;
        }
        public static ProxyHelper GetProxyHelper()
        {
            if (Proxy == null)
                Proxy = new ProxyHelper();
            return Proxy;
        }
        public static InternetConnection GetInternet()
        {
            if (Internet == null)
                Internet = new InternetConnection();
            return Internet;
        }
        public static VkAuth GetVk()
        {
            if (Vk == null)
                Vk = new VkAuth();
            return Vk;
        }
        
        #endregion

        private object contextVM;
        public object ContextVM
        {
            get { return contextVM; }
            set
            {
               contextVM = value; OnPropertyChanged(); 
            }
        }
        private object selectedViewModel;
        public object SelectedViewModel
        {
            get { return selectedViewModel; }
            set { selectedViewModel = value; OnPropertyChanged(); }
        }
        private ICommand _resizeCommand;
        public ICommand ResizeCommand
        {
            get { return _resizeCommand ?? (_resizeCommand = new RelayCommand(p => Resize())); }
        }
        
        private ICommand _collapseCommand;
        public ICommand CollapseCommand
        {
            get { return _collapseCommand ?? (_collapseCommand = new RelayCommand(p => Collapse())); }
        }
        public ICommand CloseAppCommand;

        private int _item = 1;
        public  int Item
        {
            get { return _item; }
            set
            {
                _item = value;

                SelectedViewModel = GetView(Item);
                OnPropertyChanged();           
            }
        }

        private WindowState _curWindowState;
        public WindowState CurWindowState
        {
            get { return _curWindowState; }
            set { _curWindowState = value;
                OnPropertyChanged(); }
        }
        private Visibility _curWindowVisibility;
        public Visibility CurWindowVisibility
        {
            get { return _curWindowVisibility; }
            set { _curWindowVisibility = value; OnPropertyChanged(); }
        }

        private double _contentPresenterOpacity;
        public double ContentPresenterOpacity
        {
            get { return _contentPresenterOpacity; }
            set { _contentPresenterOpacity = value; OnPropertyChanged(); }
        }

        private object GetView(int index)
        {
            switch (index)
            {
                case 1:
                    return Main;
                case 2:
                    return Player;
                case 3:
                    return Radio;
                case 4:
                    return TwitchPage;
                case 5:
                    return Settings;
                case 6:
                    return Log;
            }
            return null;
        }

        private void Resize()
        {
            if (CurWindowState == WindowState.Maximized)
                CurWindowState = WindowState.Normal;
            else
                CurWindowState = WindowState.Maximized;

            winstate = CurWindowState;
        }
        private void Collapse()
        {
            CurWindowState = WindowState.Minimized;
            winstate = CurWindowState;
        }

        #region ALERTS
        private string _isAlertOn;
        public string IsAlertOn
        {
            get { return _isAlertOn; }
            set { _isAlertOn = value;OnPropertyChanged(); }
        }

        private AlertModel _newAlert;
        public AlertModel NewAlert
        {
            get {return _newAlert; }
            set { _newAlert = value;OnPropertyChanged(); }
        }

        public async void Alert(BitmapImage image, string from, string message)
        {
            NewAlert = (new AlertModel
            {
                AlertImage = image,
                AlertFrom = from,
                AlertMessage = message,
                Time = DateTime.Now.ToString("HH:mm:ss")
            });

            alerts.Add(NewAlert);
            //SOUNDALERT
            if (Properties.Settings.Default.IsSoundAlerts == true)
            {
                BassNetHelper bh = new BassNetHelper();
                bh.Play(Properties.Settings.Default.AlertSound, Properties.Settings.Default.UserVolume);
            }
            
            //TEXTALERT
            if (Properties.Settings.Default.IsTextAlerts == true)
            {
                IsAlertOn = "ON";
                await Task.Delay(4000);
                IsAlertOn = "OFF";
            }
            //VOICEALERT
            if (Properties.Settings.Default.IsVoiceAlerts == true)
            {

            }
        }
        #endregion

    }
}
