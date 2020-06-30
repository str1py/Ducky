using Ducky.Model;
using System;
using System.Collections.Generic;
using System.Windows;
using wf = System.Windows.Forms;
using System.Windows.Input;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Net.NetworkInformation;
using Ducky.Helpers.Socials.Telegram;
using Ducky.Helpers;
using Ducky.Helpers.Socials.Vk;
using Ducky.Model.Socials;
using Ducky.Model.Messages;
using System.Collections.Specialized;

namespace Ducky.ViewModel
{
    public class SettingsPageViewModel : ViewModelBase
    {
        #region Fields
        private readonly List<string> styles;
        private string relUri = "/Themes/Colors/";
        private ResourceDictionary themeDict;
        public ObservableCollection<string> Folders { get; set; }
        public ObservableCollection<ProxyModel> ProxyList { get; set; }
        private PlayerPageViewModel player;
        private TelegramAuth tg;
        private TwitchHelper th;
        private TwitchPageViewModel twitchpage;
        private LogsPageViewModel log;
        private TelegramBot telebot;
        private VkAuth vk;
        private ProxyModel defaultProxy;
        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SettingsPageViewModel()
        {
            #region FieldsInit
            player = MainWindowViewModel.GetPlayerVM();
            twitchpage = MainWindowViewModel.GetTwitchVM();
            th = MainWindowViewModel.GetTwitchHelper();
            telebot = MainWindowViewModel.GetTelebot();
            log = MainWindowViewModel.GetLogVM();
            vk = MainWindowViewModel.GetVk();
            Folders = new ObservableCollection<string>();
            ProxyList = new ObservableCollection<ProxyModel>();
            tg = MainWindowViewModel.GetTelegram();
            styles = new List<string> { "BlackWhite", "VioletOrange", "BlackViolet" };
            themeDict = Application.LoadComponent(new Uri("/Themes/Theme.xaml", UriKind.Relative)) as ResourceDictionary;

            #endregion

            #region ThemeInit
            if (Properties.Settings.Default.Theme.Contains(styles[0]))
                BlackWhiteIsChecked = true;
            else if (Properties.Settings.Default.Theme.Contains(styles[1]))
                VioletOrangeIsChecked = true;
            else if (Properties.Settings.Default.Theme.Contains(styles[2]))
                BlackVioletIsChecked = true;

            var uri = new Uri(Properties.Settings.Default.Theme + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            Application.Current.Resources.MergedDictionaries.Add(themeDict);
            #endregion

            #region AlertsInit
            if (Properties.Settings.Default.IsSoundAlerts == true)
                SoundAlertsIsChecked = true;
            else
                SoundAlertsIsChecked = false;

            if (Properties.Settings.Default.IsTextAlerts == true)
                TextAlertsIsChecked = true;
            else
                TextAlertsIsChecked = false;

            if (Properties.Settings.Default.IsVoiceAlerts == true)
                VoiceAlertsIsChecked = true;
            else
                VoiceAlertsIsChecked = false;

            if (String.IsNullOrEmpty(Properties.Settings.Default.AlertSound))
                SoundName = "Not selected";
            else
                SoundName = Path.GetFileName(Properties.Settings.Default.AlertSound);

            SoundSliderVolumePos = Properties.Settings.Default.SoundAlertVolume;
            VoiceVolumeSliderPos = Properties.Settings.Default.VoiceAlertVolume;

            #endregion

 
            if (Properties.Settings.Default.YoutubeSearch)
                IsYoutube = true;

            #region Proxy
            switch (Properties.Settings.Default.ProxySettings)
            {
                case 0:
                    EnableProxyCheck = true;
                    break;
                case 1:
                    DefaultProxyCheck = true;
                    break;
                case 2:
                    CustomProxyCheck = true;
                    break;
            }
            #endregion

            IsTelegramLogin();
            IsTwitchLogin();

            try
            {
                var strcoll = Properties.Settings.Default.MusicFolders;
                if (strcoll != null)
                {
                    foreach (string str in strcoll)
                        Folders.Add(str.Replace("\\", "/"));
                }
            }
            catch(Exception e)
            {
                log.AddLog(e.Message, DateTime.Now.ToString());
            }

            var prlist = Properties.Settings.Default.ProxyList;
            if(prlist != null)
            {
                foreach (string str in prlist)
                {
                    string[] proxy = str.Split(';');
                    //0-type;1-ip;2-port;1+2-ipandport;3-login;4-password;5-selectVis;6-connectVIs;7-state;
                    ProxyList.Add(new ProxyModel
                    {
                        ProxyType = proxy[0],
                        IpAdress = proxy[1],
                        Port = Int32.Parse(proxy[2]),
                        IpAndPort = proxy[1] + ":" + proxy[2],
                        Login = proxy[3],
                        Password = proxy[4],
                        IsSelectedVisibility = proxy[5],
                        IsConnectingVisibility = proxy[6],//
                        State = proxy[7]
                    });
                }
            }

            string[] defaultproxy = Properties.Settings.Default.DefaultProxy.Split(';');
            defaultProxy = (new ProxyModel
            {
                ProxyType = defaultproxy[0],
                IpAdress = defaultproxy[1],
                Port = Int32.Parse(defaultproxy[2]),
                IpAndPort = defaultproxy[1] + ":" + defaultproxy[2],
                Login = defaultproxy[3],
                Password = defaultproxy[4],
                IsSelectedVisibility = defaultproxy[5],
                IsConnectingVisibility = defaultproxy[6],//
                State = defaultproxy[7]
            });

            GridBlurRadius = 0;
        }

        private double _gridBlurRadius = 0;
        public double GridBlurRadius
        {
            get { return _gridBlurRadius; }
            set { _gridBlurRadius = value; OnPropertyChanged(); }
        }

        #region Music
        //FOLDERS
        private ICommand _addFolderCommand;
        public ICommand AddFolderCommand
        {
            get { return _addFolderCommand ?? (_addFolderCommand = new RelayCommand(p => AddFolder())); }
        }
        private ICommand _delFolderCommand;
        public ICommand DelFolderCommand
        {
            get { return _delFolderCommand ?? (_delFolderCommand = new RelayCommand(p => DeleteFolder())); }
        }
        private bool _delEnable= false;
        public bool DelEnable
        {
            get { return _delEnable; }
            set { _delEnable = value; OnPropertyChanged(); }
        }
        private int _folderSelectedIndex = 1;
        public int FolderSelectedIndex
        {
            get { return _folderSelectedIndex; }
            set
            {
                _folderSelectedIndex = value; OnPropertyChanged();
                if (FolderSelectedIndex > -1)
                    DelEnable = true;
            }
        }

        //MUSIC SEARCH
        private Visibility _musicSearchVisibility;
        public Visibility MusicSearchVisibility
        {
            get { return _musicSearchVisibility; }
            set { _musicSearchVisibility = value; OnPropertyChanged(); }
        }
        private bool _isYoutube;
        public bool IsYoutube
        {
            get { return _isYoutube; }
            set { _isYoutube = value;
                OnPropertyChanged();
                ///////////////////////////////////////////// player.IsYoutubeEnable = IsYoutube;
                Properties.Settings.Default.YoutubeSearch = IsYoutube;
                Properties.Settings.Default.Save();
            }
        }

        private void AddFolder()
        {
            using (var folderDialog = new wf.FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == wf.DialogResult.OK && !string.IsNullOrWhiteSpace(folderDialog.SelectedPath))
                {
                    if (Properties.Settings.Default.MusicFolders != null)
                    {
                        Folders.Add(folderDialog.SelectedPath.Replace("\\", "/"));
                        Properties.Settings.Default.MusicFolders.Add(folderDialog.SelectedPath);
                        Properties.Settings.Default.Save();
                        player.OnFolderChange();
                    }
                    else
                    {
                        StringCollection str = new StringCollection();
                        str.Add(folderDialog.SelectedPath);
                        Folders.Add(folderDialog.SelectedPath.Replace("\\", "/"));
                        Properties.Settings.Default.MusicFolders = str;
                        Properties.Settings.Default.Save();
                        player.OnFolderChange();
                    }
                }
            }
        }
        private void DeleteFolder()
        {
            Properties.Settings.Default.MusicFolders.RemoveAt(FolderSelectedIndex);
            Folders.RemoveAt(FolderSelectedIndex);
            Properties.Settings.Default.Save();
            player.OnFolderChange();
        }
        #endregion

        #region Themes
        private bool _blackwhiteIsChecked;
        public bool BlackWhiteIsChecked
        {
            get { return _blackwhiteIsChecked; }
            set
            {
                _blackwhiteIsChecked = value;
                OnPropertyChanged();

                if (BlackWhiteIsChecked)
                {
                    VioletOrangeIsChecked = false;
                    BlackVioletIsChecked = false;
                    ChangeTheme(styles[0]);
                }
            }
        }

        private bool _voiletOrangeIsChecked;
        public bool VioletOrangeIsChecked
        {
            get { return _voiletOrangeIsChecked; }
            set
            {
                _voiletOrangeIsChecked = value;
                OnPropertyChanged();
                if (VioletOrangeIsChecked)
                {
                    BlackWhiteIsChecked = false;
                    BlackVioletIsChecked = false;
                    ChangeTheme(styles[1]);
                }
            }
        }

        private bool _blackVioletIsChecked;
        public bool BlackVioletIsChecked
        {
            get { return _blackVioletIsChecked; }
            set
            {
                _blackVioletIsChecked = value;
                OnPropertyChanged();

                if (BlackVioletIsChecked)
                {
                    BlackWhiteIsChecked = false;
                    VioletOrangeIsChecked = false;
                    ChangeTheme(styles[2]);
                }
            }
        }

        private void ChangeTheme(string theme)
        {
            var uri = new Uri(relUri + theme + ".xaml", UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            Application.Current.Resources.MergedDictionaries.Add(themeDict);
            Properties.Settings.Default.Theme = relUri + theme;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region Alerts
        private Visibility _soundAlertsVisibility;
        public Visibility SoundAlertsVisibility {
            get { return _soundAlertsVisibility; }
            set { _soundAlertsVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _textAlertsVisibility;
        public Visibility TextAlertsVisibility
        {
            get { return _textAlertsVisibility; }
            set { _textAlertsVisibility = value; OnPropertyChanged(); }

        }
        private Visibility _voiceAlertsVisibility;
        public Visibility VoiceAlertsVisibility
        {
            get { return _voiceAlertsVisibility; }
            set { _voiceAlertsVisibility = value; OnPropertyChanged(); }
        }

        private bool _soundAlertsIsChecked;
        public bool SoundAlertsIsChecked
        {
            get { return _soundAlertsIsChecked; }
            set
            {
                _soundAlertsIsChecked = value;
                OnPropertyChanged();
                if (SoundAlertsIsChecked == true) {
                    SoundAlertsVisibility = Visibility.Visible;
                    Properties.Settings.Default.IsSoundAlerts = true;
                }
                else {
                    SoundAlertsVisibility = Visibility.Collapsed;
                    Properties.Settings.Default.IsSoundAlerts = false;
                }
                Properties.Settings.Default.Save();
            }
        }
        private bool _textAlertsIsChecked;
        public bool TextAlertsIsChecked
        {
            get { return _textAlertsIsChecked; }
            set
            {
                _textAlertsIsChecked = value;
                OnPropertyChanged();
                if (TextAlertsIsChecked == true) {
                    TextAlertsVisibility = Visibility.Visible;
                    Properties.Settings.Default.IsTextAlerts = true;
                }
                else {
                    TextAlertsVisibility = Visibility.Collapsed;
                    Properties.Settings.Default.IsTextAlerts = false;
                }
                Properties.Settings.Default.Save();
            }
        }
        private bool _voiceAlertsIsChecked;
        public bool VoiceAlertsIsChecked
        {
            get { return _voiceAlertsIsChecked; }
            set
            {
                _voiceAlertsIsChecked = value;
                OnPropertyChanged();
                if (VoiceAlertsIsChecked == true)
                {
                    VoiceAlertsVisibility = Visibility.Visible;
                    Properties.Settings.Default.IsVoiceAlerts = true;
                }
                else
                {
                    VoiceAlertsVisibility = Visibility.Collapsed;
                    Properties.Settings.Default.IsVoiceAlerts = false;
                }
                Properties.Settings.Default.Save();
            }
        }
        private string _soundName;
        public string SoundName
        {
            get { return _soundName; }
            set { _soundName = value; OnPropertyChanged(); }
        }

        private ICommand _selectSoundCommand;
        public ICommand SelectSoundCommand
        {
            get { return _selectSoundCommand ?? (_selectSoundCommand = new RelayCommand(p => SelectSound())); }
        }

        private int _soundSliderVolumePos;
        public int SoundSliderVolumePos
        {
            get { return _soundSliderVolumePos; }
            set {
                _soundSliderVolumePos = value;
                OnPropertyChanged();
                Properties.Settings.Default.SoundAlertVolume = SoundSliderVolumePos;
                Properties.Settings.Default.Save();
            }
        }
        private int _voiceVolumeSliderPos;
        public int VoiceVolumeSliderPos
        {
            get { return _voiceVolumeSliderPos; }
            set { _voiceVolumeSliderPos = value;
                OnPropertyChanged();
                Properties.Settings.Default.VoiceAlertVolume = VoiceVolumeSliderPos;
                Properties.Settings.Default.Save();
            }
        }

        private void SelectSound()
        {
            using (var fileDialog = new wf.OpenFileDialog())
            {
                if (fileDialog.ShowDialog() == wf.DialogResult.OK && !string.IsNullOrWhiteSpace(fileDialog.FileName))
                {
                    using (var sound = TagLib.File.Create(fileDialog.FileName))
                    {
                        if ((int)sound.Properties.Duration.TotalSeconds > 2)
                            MessageBox.Show("Длительность более 2 секунды, выберить другой файл");
                        else
                        {
                            SoundName = Path.GetFileName(fileDialog.FileName);
                            Properties.Settings.Default.AlertSound = fileDialog.FileName;
                            Properties.Settings.Default.Save();
                        }
                    }
                }
            }
        }
        #endregion

        #region Socials

        #region TelegramPanel
        private ICommand _proxySettingsCommand;
        public ICommand ProxySettingsCommand
        {
            get { return _proxySettingsCommand ?? (_proxySettingsCommand = new RelayCommand(p => ShowProxySettings())); }
        }
        #endregion

        #region Proxy List Refresh, DoubleClick, Selected
        private ICommand _refreshProxyCommand;
        public ICommand RefreshProxyCommand
        {
            get { return _refreshProxyCommand ?? (_refreshProxyCommand = new RelayCommand(p => Ping())); }
        }
        private ICommand _listViewDoubleClick;
        public ICommand ListViewDoubleClick
        {
            get { return _listViewDoubleClick ?? (_listViewDoubleClick = new RelayCommand(p => SelectProxy())); }
        }
        private int _proxySelectedIndex;
        public int ProxySelectedIndex
        {
            get { return _proxySelectedIndex; }
            set { _proxySelectedIndex = value; OnPropertyChanged(); }
        }
        private ProxyModel _proxySelectedItem;
        public ProxyModel ProxySelectedItem
        {
            get { return _proxySelectedItem; }
            set { _proxySelectedItem = value; OnPropertyChanged(); }
        }
        private int _tabSelectedIndex;
        public int TabSelectedIndex
        {
            get { return _tabSelectedIndex; }
            set
            {
                _tabSelectedIndex = value;
                OnPropertyChanged();

                if (TabSelectedIndex == 4)
                {
                    foreach (ProxyModel pm in ProxyList)
                        pm.State = "Проверка...";

                    Ping();
                }
            }
        }
        private bool _isProxyListEnable;
        public bool IsProxyListEnable
        {
            get { return _isProxyListEnable; }
            set { _isProxyListEnable = value; OnPropertyChanged(); }
        }
        #endregion
        #region ProxySettingsPanel Show/Hide

        private ICommand _hideProxyAddCommand;
        public ICommand HideProxyCommand
        {
            get { return _hideProxyAddCommand ?? (_hideProxyAddCommand = new RelayCommand(p => HideProxySettings())); }
        }
        private Visibility _proxySettingsVisibility = Visibility.Hidden;
        public Visibility ProxySettingsVisibility
        {
            get { return _proxySettingsVisibility; }
            set { _proxySettingsVisibility = value; OnPropertyChanged(); }
        }

        private void ShowProxySettings()
        {
            GridBlurRadius = 20;
            ProxySettingsVisibility = Visibility.Visible;
        }
        private void HideProxySettings()
        {
            GridBlurRadius = 0;
            ProxySettingsVisibility = Visibility.Hidden;
        }
        #endregion
        #region Proxy Settings Panel
        private bool _enableProxyCheck;
        public bool EnableProxyCheck
        {
            get { return _enableProxyCheck; }
            set
            {
                _enableProxyCheck = value;
                OnPropertyChanged();
                if (EnableProxyCheck)
                {
                    Properties.Settings.Default.IsProxy = false;
                    Properties.Settings.Default.ProxySettings = 0;
                    Properties.Settings.Default.Save();
                    IsProxyListEnable = false;
                }
            }
        }
        private bool _defaultProxyCheck;
        public bool DefaultProxyCheck
        {
            get { return _defaultProxyCheck; }
            set
            {
                _defaultProxyCheck = value;
                OnPropertyChanged();
                Properties.Settings.Default.ProxySettings = 1;
                Properties.Settings.Default.ProxyInUse = Properties.Settings.Default.DefaultProxy;
                Properties.Settings.Default.Save();
                CreateConnection(defaultProxy);
                if (DefaultProxyCheck)
                    IsProxyListEnable = false;
            }
        }
        private bool _customProxyCheck;
        public bool CustomProxyCheck
        {
            get { return _customProxyCheck; }
            set
            {
                _customProxyCheck = value;
                OnPropertyChanged();
                Properties.Settings.Default.IsProxy = true;
                Properties.Settings.Default.ProxySettings = 2;
                Properties.Settings.Default.Save();
                if (CustomProxyCheck)
                    IsProxyListEnable = true;
            }
        }

        private ICommand _deleteProxyCommand;
        public ICommand DeleteProxyCommand
        {
            get { return _deleteProxyCommand ?? (_deleteProxyCommand = new RelayCommand(p => DeleteProxy())); }
        }
        private ICommand _newProxyAddCommand;
        public ICommand NewProxyAddCommand
        {
            get { return _newProxyAddCommand ?? (_newProxyAddCommand = new RelayCommand(p => AddNewProxy())); }
        }
        #endregion
        #region AddNewProxyPanel Show/Hide
        private ICommand _addProxyCommand;
        public ICommand AddProxyCommand
        {
            get { return _addProxyCommand ?? (_addProxyCommand = new RelayCommand(p => AddProxyShow())); }
        }
        private ICommand _addCancleProxyCommand;
        public ICommand AddCancleProxyCommand
        {
            get { return _addCancleProxyCommand ?? (_addCancleProxyCommand = new RelayCommand(p => CancleAddProxy())); }
        }
        private Visibility _proxyAddVisibility = Visibility.Hidden;
        public Visibility ProxyAddVisibility
        {
            get { return _proxyAddVisibility; }
            set { _proxyAddVisibility = value; OnPropertyChanged(); }
        }

        private void AddProxyShow()
        {
            ProxyAddVisibility = Visibility.Visible;
        }
        private void CancleAddProxy()
        {
            ProxyAddVisibility = Visibility.Hidden;
        }
        #endregion
        #region Proxy Adding Panel
        private bool _isSocks5Check;
        public bool IsSocks5Check
        {
            get { return _isSocks5Check; }
            set { _isSocks5Check = value; OnPropertyChanged(); }
        }
        private bool _isHTTPCheck;
        public bool IsHTTPCheck
        {
            get { return _isHTTPCheck; }
            set { _isHTTPCheck = value; OnPropertyChanged(); }
        }

        private string _newProxyIpAdress;
        public string NewProxyIpAdress
        {
            get { return _newProxyIpAdress; }
            set { _newProxyIpAdress = value; OnPropertyChanged(); }
        }
        private int _newProxyPort;
        public int NewProxyPort
        {
            get { return _newProxyPort; }
            set { _newProxyPort = value; OnPropertyChanged(); }
        }
        private string _newProxyLogin;
        public string NewProxyLogin
        {
            get { return _newProxyLogin; }
            set { _newProxyLogin = value; OnPropertyChanged(); }
        }
        private string _newProxyPassword;
        public string NewProxyPassword
        {
            get { return _newProxyPassword; }
            set { _newProxyPassword = value; OnPropertyChanged(); }
        }


        #endregion
        #region Proxy Methods
        private bool _isProxyEnable;
        public bool IsProxyEnable
        {
            get { return _isProxyEnable; }
            set
            {
                _isProxyEnable = value;
                OnPropertyChanged();

                if (IsProxyEnable)
                    ProxyState = "Connected";
                else
                    ProxyState = "Not connected";
                var proxysplit = Properties.Settings.Default.ProxyInUse?.Split(';');
                if (proxysplit[0] != "")
                    ProxyIp = $"IP: {proxysplit[1]}:{proxysplit[2]}";
            }
        }
        private string _proxyState;
        public string  ProxyState
        {
            get { return _proxyState; }
            set { _proxyState = value;OnPropertyChanged(); }
        }
        private string _proxyIp;
        public string  ProxyIp
        {
            get { return _proxyIp; }
            set { _proxyIp = value; OnPropertyChanged(); }
        }
        private Visibility _proxyConnectingVisibility;
        public Visibility ProxyConnectingVisibility
        {
            get { return _proxyConnectingVisibility; }
            set {
                _proxyConnectingVisibility = value;
                OnPropertyChanged();
                if (ProxyConnectingVisibility == Visibility.Visible)
                    ProxyStateVisibility = Visibility.Hidden;
                else
                    ProxyStateVisibility = Visibility.Visible;
            }
        }
        private Visibility _proxyStateVisibility;
        public Visibility ProxyStateVisibility
        {
            get { return _proxyStateVisibility; }
            set { _proxyStateVisibility = value;OnPropertyChanged(); }
        }

        private async void SelectProxy()
        {
            ProxyModel prev = null;
            var prevProxyString = Properties.Settings.Default.ProxyInUse;
            //Находим предыдущий активный прокси
            try { prev = ProxyList.FirstOrDefault(x => x.IsSelectedVisibility == "Visible"); }
            catch { }

            #region DIACTIVATE PREVIOUS CONNECTION
            if (prev != null)
            {
                prev.IsSelectedVisibility = "Hidden"; //убирам галочку что подключено
                prev.State = prev.State.Replace("Подключен", "Доступен"); //замена статуса на доступен
                ChangeProxyInSettings(prev.IpAdress, "", "Hidden");  //Замена значения  в Settings
            }
            #endregion

            #region TRY TO ACTIVATE SELECTED PROXY
            ProxySelectedItem.State = "Подключение..."; //Изменение статуса н
            ProxySelectedItem.IsConnectingVisibility = "Visible"; //Подказ спинера загрузки

            //Проверяем подключение
            ProxySelectedItem.IsSelectedVisibility = "Visible";
            string state = await CreateConnection(ProxySelectedItem);

            if (!state.Contains("Недоступен")) // если в состояние нет НЕДОСТУПЕН
            {
                ProxySelectedItem.State = state;
                ProxySelectedItem.IsConnectingVisibility = "Hidden";
                string proxystring = $"{ProxySelectedItem.ProxyType};{ProxySelectedItem.IpAdress};{ProxySelectedItem.Port};{ProxySelectedItem.Login};{ProxySelectedItem.Password}";
                ChangeProxyInSettings(ProxySelectedItem.IpAdress, proxystring, "Visible");
            }
            else//Если подключится не удалось 
            {
                ProxySelectedItem.State = "Недоступен"; //Замена состояние на НЕДОСТУПЕН прокси к которому хотели подключится
                ProxySelectedItem.IsSelectedVisibility = "Hidden";
                ProxySelectedItem.IsConnectingVisibility = "Hidden";
                try
                {
                    if (prev != null)
                    {
                        Ping ping = new Ping();
                        var p = await ping.SendPingAsync(prev.IpAdress);
                        prev.IsSelectedVisibility = "Visible";
                        prev.State = $"Подключен, пинг {p.RoundtripTime}";
                        ping.Dispose();
                        //Замена значения  в ProxyList
                        ChangeProxyInSettings(prev.IpAdress, prevProxyString, "Visible");
                    }
                }
                catch
                {
                    prev.IsSelectedVisibility = "Hidden";
                    prev.State = $"Недоступен";
                    //Замена значения  в ProxyList
                    ChangeProxyInSettings(prev.IpAdress, "", "Hidden");
                }
            }
            #endregion
        }
        private async void Ping()
        {
            foreach (ProxyModel pm in ProxyList)
            {
                pm.State = "Проверка...";
                pm.IsConnectingVisibility = "Visible";
            }

            foreach (ProxyModel pm in ProxyList)
            {
                pm.State = await GetProxyStatus(pm);
                pm.IsConnectingVisibility = "Hidden";
            }
        }

        public async Task<string> CreateConnection(ProxyModel pm)
        {
            if (InternetConnection.IsConnectionExist)
            {
                try
                {
                    if (TgUser != null)
                        TelegramSpinnerVisibility = Visibility.Visible;
                    
                    //Connection
                    ProxyConnectingVisibility = Visibility.Visible;
                    var tgconnection = await tg.TryToConnect($"{pm.ProxyType};{pm.IpAdress};{pm.Port};{pm.Login};{pm.Password}");
                    var tgbotconnection = await telebot.TryToConnect($"{pm.ProxyType};{pm.IpAdress};{pm.Port};{pm.Login};{pm.Password}");
                    log.AddLog($"Попытка подключения к {pm.IpAndPort} ", $"Telegram connection - {tgconnection}, TelegramBot connection - {tgbotconnection}");
                    ProxyConnectingVisibility = Visibility.Hidden;
                    var pingstate = await new Ping().SendPingAsync(pm.IpAdress, 5000);
                    IsProxyEnable = tgbotconnection;
                  
                  

                    if (pm.IsSelectedVisibility == "Hidden")
                    {
                        if (pingstate.Status == IPStatus.Success && tgconnection == true && tgbotconnection == true)
                            return $"Доступен, пинг {pingstate.RoundtripTime} мс";
                        else
                            return $"Недоступен";
                    }
                    else
                    {
                        if (pingstate.Status == IPStatus.Success && tgconnection == true && tgbotconnection == true)
                            return $"Подключен, пинг {pingstate.RoundtripTime} мс";
                        else
                            return $"Недоступен";
                    }
                    
                }
                catch (Exception e)
                {
                    log.AddLog("ERROR : ", e.Message, "Error in ", e.TargetSite.DeclaringType.Name);
                    pm.IsSelectedVisibility = "Hidden";
                    return $"Недоступен";
                }
            }
            else
            {
                pm.IsSelectedVisibility = "Hidden";
                return $"Недоступен";
            }
        }
        public async Task<string> GetProxyStatus(ProxyModel pm)
        {
            if (InternetConnection.IsConnectionExist)
            {
                try
                {
                    var tgconnection = await tg.TryToConnect($"{pm.ProxyType};{pm.IpAdress};{pm.Port};{pm.Login};{pm.Password}");
                    var tgbotconnection = await telebot.TryToConnect($"{pm.ProxyType};{pm.IpAdress};{pm.Port};{pm.Login};{pm.Password}");
                    var pingstate = await new Ping().SendPingAsync(pm.IpAdress, 5000);

                    if (pm.IsSelectedVisibility == "Hidden")
                    {
                        if (pingstate.Status == IPStatus.Success && tgconnection == true)
                            return $"Доступен, пинг {pingstate.RoundtripTime} мс";
                        else
                            return $"Недоступен";
                    }
                    else
                    {
                        if (pingstate.Status == IPStatus.Success && tgconnection == true)
                            return $"Подключен, пинг {pingstate.RoundtripTime} мс";
                        else
                            return $"Недоступен";
                    }
                }
                catch
                {
                    pm.IsSelectedVisibility = "Hidden";
                    return $"Недоступен";
                }
            }
            else
            {
                pm.IsSelectedVisibility = "Hidden";
                return $"Недоступен";
            }
        }

        private void ChangeProxyInSettings(string ip, string proxyString, string newVis)
        {
            var item = Properties.Settings.Default.ProxyList;
            for (int i = 0; i < item.Count; i++)
            {
                if (item[i].Contains(ip))
                {
                    var str = item[i].Split(';');
                    str[5] = newVis;
                    //0-type;1-ip;2-port;3-login;4-password;5-selectVis;6-connectinVis;7-state
                    item[i] = $"{str[0]};{str[1]};{str[2]};{str[3]};{str[4]};{str[5]};{str[6]};{str[7]}";
                }
            }
            Properties.Settings.Default.ProxyInUse = proxyString;
            Properties.Settings.Default.Save();

        }
        private async void AddNewProxy()
        {
            string proxytype;

            if (IsSocks5Check)
                proxytype = "Socks5";
            else if (IsHTTPCheck)
                proxytype = "HTTP";
            else
                proxytype = "";

            if (NewProxyIpAdress.Length > 6 && NewProxyPort > 1 && proxytype != "")
            {
                ProxyAddVisibility = Visibility.Hidden;

                Properties.Settings.Default.ProxyList.Add($"{proxytype};{NewProxyIpAdress};{NewProxyPort};{NewProxyLogin};{NewProxyPassword};Hidden;Visible;недоступен;0");
                Properties.Settings.Default.Save();

                var proxy = new ProxyModel
                {
                    ProxyType = proxytype,
                    IpAdress = NewProxyIpAdress,
                    IpAndPort = $"{NewProxyIpAdress}:{NewProxyPort}",
                    Port = NewProxyPort,
                    Login = NewProxyLogin,
                    Password = NewProxyPassword,
                    IsSelectedVisibility = "Hidden",
                    IsConnectingVisibility = "Visible",
                    State = "Проверка...",
                };
                ProxyList.Add(proxy);
                var newitem = ProxyList.Single(x => x.IpAdress == NewProxyIpAdress);
                proxy.State = await GetProxyStatus(proxy);
                proxy.IsConnectingVisibility = "Hidden";

                NewProxyIpAdress = "";
                NewProxyPort = 0;
                NewProxyLogin = "";
                NewProxyPassword = "";
            }
        }
        private void DeleteProxy()
        {
            if (ProxySelectedIndex > -1)
            {
                Properties.Settings.Default.ProxyList.RemoveAt(ProxySelectedIndex);
                ProxyList.RemoveAt(ProxySelectedIndex);
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Telegram Login Panel
        private Visibility _telegramLoginPanelVisibility = Visibility.Hidden;
        public Visibility TelegramLoginPanelVisibility
        {
            get { return _telegramLoginPanelVisibility; }
            set { _telegramLoginPanelVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _telegtamLoginPanelSpinnerVisibility = Visibility.Hidden;
        public Visibility TelegtamLoginPanelSpinnerVisibility
        {
            get { return _telegtamLoginPanelSpinnerVisibility; }
            set { _telegtamLoginPanelSpinnerVisibility = value; OnPropertyChanged(); }
        }

        private bool _isTelegramLoginEnabled = true;
        public bool IsTelegramLoginEnabled
        {
            get { return _isTelegramLoginEnabled; }
            set {
                _isTelegramLoginEnabled = value; OnPropertyChanged();
                if (IsTelegramLoginEnabled)
                    TelegtamLoginPanelSpinnerVisibility = Visibility.Hidden;
                else
                    TelegtamLoginPanelSpinnerVisibility = Visibility.Visible;
            }
        }

        private ICommand _telegramLoginNextCommand;
        public ICommand TelegramLoginNextCommand
        {
            get { return _telegramLoginNextCommand ?? (_telegramLoginNextCommand = new RelayCommand(p => TelegtamLoginNext())); }
        }

        private string _telegramPhone;
        public string TelegramPhone
        {
            get { return _telegramPhone; }
            set { _telegramPhone = value; OnPropertyChanged(); }
        }
        #endregion
        #region Telegram Code Panel
        private Visibility _telegramCodePanelVisibility = Visibility.Hidden;
        public Visibility TelegramCodePanelVisibility
        {
            get { return _telegramCodePanelVisibility; }
            set { _telegramCodePanelVisibility = value; OnPropertyChanged(); }
        }
        private ICommand _telegramCodeCancleCommand;
        public ICommand TelegramCodeCancleCommand
        {
            get { return _telegramCodeCancleCommand ?? (_telegramCodeCancleCommand = new RelayCommand(p => TelegramCodeCancle())); }
        }
        private string _telegramCode;
        public string TelegramCode
        {
            get { return _telegramCode; }
            set
            {
                _telegramCode = value;
                OnPropertyChanged();
                if (TelegramCode.Length == 5)
                    SetTelegramUserData();
            }
        }
        #endregion
        #region Telegram Methods
        private async void TelegtamLoginNext()
        {
            GridBlurRadius = 20;
            IsTelegramLoginEnabled = false;
            var tphone = await tg.PhoneCheck(TelegramPhone);
            if (tphone)
            {
                TelegramPhone = "";
                TelegramLoginPanelVisibility = Visibility.Hidden;
                TelegramCodePanelVisibility = Visibility.Visible;
            }
            IsTelegramLoginEnabled = true;
        }
        private async void SetTelegramUserData()
        {
            TelegramSpinnerVisibility = Visibility.Visible;
            TelegramConnectPanel = Visibility.Visible;
            TelegramConnectionContent = "Connecting...";
            TgUser = await tg.CodeCheck(TelegramCode);
            if (TgUser != null)
            {
                TelegramCode = "";
                TelegramCodePanelVisibility = Visibility.Hidden;
                TelegramLoginVisibility = Visibility.Hidden;
                TelegramConnectPanel = Visibility.Hidden;
                TelegramUserVisibility = Visibility.Visible;
                TelegramUserButtonContent = "@" + TgUser.UserNickName;
                IsTelegramEnabled = true;
                GridBlurRadius = 0 ;
                MainPageViewModel.Messages.Add(new DuckMessage("Поздравляю! Вы подключили Ducky к Telegram! Как использовать Telegram для управления? Все очень просто! Зайдите в Telegram и найдите @Duckyoff_bot если вам не пришло сообщение", "Ducky"));
            }
            else
            {
                TelegramCode = "";
                TelegramConnectionContent = "Not connected";
                TelegramSpinnerVisibility = Visibility.Collapsed;
                MainWindowViewModel.instance.Alert(null, "Telegram", "Введенный код неверный,либо что-то с интернетом");
                IsTelegramEnabled = false;
            }

        }
        private void TelegramLoginCancle()
        {
            GridBlurRadius = 0 ;
            TelegramLoginPanelVisibility = Visibility.Hidden;
        }
        private void TelegramCodeCancle()
        {
            GridBlurRadius = 0 ;
            TelegramCodePanelVisibility = Visibility.Hidden;
        }
        #endregion

        #region TelegramBlock
        //Telegram User Model
        private TelegramUserModel _tguser;
        public TelegramUserModel TgUser
        {
            get { return _tguser; }
            set { _tguser = value; OnPropertyChanged(); }
        }

        //Telegram Login Command
        private ICommand _telegramLoginShowCommand;
        public ICommand TelegramLoginShowCommand
        {
            get { return _telegramLoginShowCommand ?? (_telegramLoginShowCommand = new RelayCommand(p => TelegramLogin())); }
        }
        //Button command
        private ICommand _showTelegramProfileCommand;
        public ICommand ShowTelegramProfileCommand
        {
            get { return _showTelegramProfileCommand ?? (_showTelegramProfileCommand = new RelayCommand(p => ShowTelegramProfile())); }
        }
        //Cancle Login
        private ICommand _telegramLoginCancleCommand;
        public ICommand TelegramLoginCancleCommand
        {
            get { return _telegramLoginCancleCommand ?? (_telegramLoginCancleCommand = new RelayCommand(p => TelegramLoginCancle())); }
        }

        //User Button Visibility
        private Visibility _telegramUserVisibility;
        public Visibility TelegramUserVisibility
        {
            get { return _telegramUserVisibility; }
            set { _telegramUserVisibility = value; OnPropertyChanged(); }
        }
        //Login Visibility
        private Visibility _telegramLoginVisibility;
        public Visibility TelegramLoginVisibility
        {
            get { return _telegramLoginVisibility; }
            set { _telegramLoginVisibility = value; OnPropertyChanged(); }
        }
        //Login button Visibility
        private Visibility _telegramLoginButtonVisibility;
        public Visibility TelegramLoginButtonVisibility
        {
            get { return _telegramLoginButtonVisibility; }
            set { _telegramLoginButtonVisibility = value; OnPropertyChanged(); }
        }
        //Spiner Load Visibility
        private Visibility _telegramSpinnerVisibility = Visibility.Collapsed;
        public Visibility TelegramSpinnerVisibility
        {
            get { return _telegramSpinnerVisibility; }
            set { _telegramSpinnerVisibility = value; OnPropertyChanged(); }
        }
        //StackPanel Visibility
        private Visibility _telegramConnectPanel;
        public Visibility TelegramConnectPanel
        {
            get { return _telegramConnectPanel; }
            set { _telegramConnectPanel = value; OnPropertyChanged(); }
        }

        private Visibility _telegramLogoutVisibility;
        public Visibility TelegramLogoutVisibility
        {
            get { return _telegramLogoutVisibility; }
            set { _telegramLogoutVisibility = value; OnPropertyChanged(); }
        }


        private bool _isTelegramEnabled;
        public bool IsTelegramEnabled
        {
            get { return _isTelegramEnabled; }
            set { _isTelegramEnabled = value; OnPropertyChanged(); }
        }
        
        private string _telegramUserButtonContent;
        public string TelegramUserButtonContent
        {
            get { return _telegramUserButtonContent; }
            set { _telegramUserButtonContent = value; OnPropertyChanged(); }
        }
        private string _telegramConnectionContent;
        public string TelegramConnectionContent
        {
            get { return _telegramConnectionContent; }
            set { _telegramConnectionContent = value; OnPropertyChanged(); }
        }

        void ShowTelegramProfile()
        {
            GridBlurRadius = 20;
            try
            {
                User = (new BaseUserModel
                {
                    social = "telegram",
                    UserName = TgUser.UserNameAndSurname,
                    UserNickName = "@" + TgUser.UserNickName,
                    UserPhoto = TgUser.UserPhoto
                });
                UserInfoPanelVisibility = Visibility.Visible;
            }
            catch { }
        }

        private void TelegramLogin()
        {
            GridBlurRadius = 20;
            TelegramLoginPanelVisibility = Visibility.Visible;
        }

        private async void TelegramLogout()
        {
            await tg.LogOut();
            TelegramConnectionContent = "Not connected";
            TelegramConnectPanel = Visibility.Visible;
            TelegramUserVisibility = Visibility.Hidden;
            TelegramSpinnerVisibility = Visibility.Collapsed;

            IsTelegramEnabled = false;
            TgUser = null;
            TelegramLoginVisibility = Visibility.Visible;
            GridBlurRadius = 0;
        }

        #endregion

        #region TwitchBlock
        //Twitch User Model
        private TwitchUserModel _twitchUser;
        public TwitchUserModel TwitchUser
        {
            get { return _twitchUser; }
            set { _twitchUser = value; OnPropertyChanged(); }
        }
        //Login uri
        private Uri _twitchLoginLink;
        public Uri TwitchLoginLink
        {
            get { return _twitchLoginLink; }
            set { _twitchLoginLink = value; OnPropertyChanged(); }
        }

        //Twitch Login Command
        private ICommand _twitchLoginCommand;
        public ICommand TwitchLoginCommand
        {
            get { return _twitchLoginCommand ?? (_twitchLoginCommand = new RelayCommand(p => TwitchLogin())); }
        }
        //Button command
        private ICommand _showTwitchProfileCommand;
        public ICommand ShowTwitchProfileCommand
        {
            get { return _showTwitchProfileCommand ?? (_showTwitchProfileCommand = new RelayCommand(p => ShowTwitchProfile())); }
        }
        //Cancle Login
        private ICommand _twitchLoginCloseCommand;
        public ICommand TwitchLoginCloseCommand
        {
            get { return _twitchLoginCloseCommand ?? (_twitchLoginCloseCommand = new RelayCommand(p => TwitchLoginClose())); }
        }

        //User Button Visibility
        private Visibility _twitchUserVisibility;
        public Visibility TwitchUserVisibility
        {
            get { return _twitchUserVisibility; }
            set { _twitchUserVisibility = value; OnPropertyChanged(); }
        }
        //WebBrowser Visibility
        private Visibility _twitchLoginlWebBrowserVisibility = Visibility.Hidden;
        public Visibility TwitchLoginlWebBrowserVisibility
        {
            get { return _twitchLoginlWebBrowserVisibility; }
            set { _twitchLoginlWebBrowserVisibility = value; OnPropertyChanged(); }
        }
        //Login button Visibility
        private Visibility _twitchLoginVisibility = Visibility.Hidden;
        public Visibility TwitchLoginlVisibility
        {
            get { return _twitchLoginVisibility; }
            set { _twitchLoginVisibility = value; OnPropertyChanged(); }
        }
        //Spiner Load Visibility
        private Visibility _twitchSpinnerVisibility;
        public Visibility TwitchSpinnerVisibility
        {
            get { return _twitchSpinnerVisibility; }
            set { _twitchSpinnerVisibility = value; OnPropertyChanged(); }
        }
        //StackPanel Visibility
        private Visibility _twitchConnectPanel;
        public Visibility TwitchConnectPanel
        {
            get { return _twitchConnectPanel; }
            set { _twitchConnectPanel = value; OnPropertyChanged(); }
        }

        //StackPanel content
        private string _twitchConnectionContent;
        public string TwitchConnectionContent
        {
            get { return _twitchConnectionContent; }
            set { _twitchConnectionContent = value; OnPropertyChanged(); }
        }
        //Button content
        private string _twitchUserButtonContent;
        public string TwitchUserButtonContent
        {
            get { return _twitchUserButtonContent; }
            set { _twitchUserButtonContent = value; OnPropertyChanged(); }
        }

        //Border enable . Color change
        private bool _isTwitchEnabled;
        public bool IsTwitchEnabled
        {
            get { return _isTwitchEnabled; }
            set { _isTwitchEnabled = value; OnPropertyChanged(); }
        }

        private void ShowTwitchProfile()
        {
            GridBlurRadius = 20;
            try
            {
                User = (new BaseUserModel
                {
                    social = "twitch",
                    UserName = TwitchUser.UserName,
                    UserNickName = "",
                    UserPhoto = TwitchUser.UserImage
                });
                UserInfoPanelVisibility = Visibility.Visible;
            }
            catch { }
        }
        private async void TwitchLogin()
        {
            GridBlurRadius = 20;
            TwitchLoginlWebBrowserVisibility = Visibility.Visible;
            string str = $"https://id.twitch.tv/oauth2/authorize?client_id={Properties.Socials.Default.TwitchAppClientID}&redirect_uri=http://localhost&response_type=code&scope=user_subscriptions+user_read";
            TwitchLoginLink = new Uri(str);
            string code = await th.GetCodeFromCallBack();

            TwitchSpinnerVisibility = Visibility.Visible;
            TwitchConnectionContent = "Connecting...";

            TwitchLoginlWebBrowserVisibility = Visibility.Hidden;
            TwitchUser = await th.GetUserInfoAsync(code);
            if (TwitchUser != null)
            {
                TwitchUserButtonContent = TwitchUser.UserName;
                twitchpage.GetStreamers();
                TwitchLoginlVisibility = Visibility.Hidden;
                IsTwitchEnabled = true;
                TwitchConnectPanel = Visibility.Hidden;
                TwitchUserVisibility = Visibility.Visible;
            }
            else
            {
                IsTwitchEnabled = false;
                TwitchLoginlVisibility = Visibility.Visible;

                TwitchConnectionContent = "Not connected";
            }
            TwitchSpinnerVisibility = Visibility.Collapsed;
            GridBlurRadius =0;
        }
        public void TwitchLogout()
        {
            th.Logout();
            twitchpage.UserLogOut();
            TwitchUser = null;
            TwitchConnectionContent = "Not connected";
            IsTwitchEnabled = false;
            TwitchLoginlVisibility = Visibility.Visible;
            TwitchConnectPanel = Visibility.Visible;
            TwitchUserVisibility = Visibility.Hidden;
            GridBlurRadius = 0 ;
        }
        private void TwitchLoginClose()
        {
            TwitchLoginlWebBrowserVisibility = Visibility.Hidden;
            TwitchLoginLink = new Uri("http://localhost/auth_cancle");
            GridBlurRadius = 0;
        }
        #endregion 

        #region VK Login Panel 
        private string _vkLoginText;
        public string VkLoginText
        {
            get { return _vkLoginText; }
            set { _vkLoginText = value; OnPropertyChanged(); }
        }
        private string _vkPasswordText;
        public string VkPasswordText
        {
            get { return _vkPasswordText; }
            set { _vkPasswordText = value; OnPropertyChanged(); }
        }

        public void VkLoginLogout()
        {
            VkLoginPanelVisibility = Visibility.Visible;
            GridBlurRadius = 20;
        }
        public void VkLoginCancle()
        {
            GridBlurRadius = 0;
            VkLoginPanelVisibility = Visibility.Hidden;
        }
        #endregion
        #region VkBlock
        //Vk User Model
        private VkUserModel _vkuser;
        public VkUserModel VkUser
        {
            get { return _vkuser; }
            set { _vkuser = value; OnPropertyChanged(); }
        }

        //Login  commnad 
        private ICommand _vkLoginCommand;
        public ICommand VkLoginCommand
        {
            get { return _vkLoginCommand ?? (_vkLoginCommand = new RelayCommand(p => VkLogin())); }
        }
        //Button command
        private ICommand _showVkProfileCommand;
        public ICommand ShowVkProfileCommand
        {
            get { return _showVkProfileCommand ?? (_showVkProfileCommand = new RelayCommand(p => ShowVkProfile())); }
        }
        //Cancle login action
        private ICommand _vkLoginCancleCommand;
        public ICommand VkLoginCancleCommand
        {
            get { return _vkLoginCancleCommand ?? (_vkLoginCancleCommand = new RelayCommand(p => VkLoginCancle())); }
        }
        //Show Vk Login Panel
        private ICommand _vkLoginVisibilityCommand;
        public ICommand VkLoginVisibilityCommand
        {
            get { return _vkLoginVisibilityCommand ?? (_vkLoginVisibilityCommand = new RelayCommand(p => ShowVkLoginPanel())); }
        }

        //Spin Visibility
        private Visibility _vkSpinVisibility = Visibility.Collapsed;
        public Visibility VkSpinVisibility
        {
            get { return _vkSpinVisibility; }
            set { _vkSpinVisibility = value; OnPropertyChanged(); }
        }
        //StackPanel Visibility
        private Visibility _vkConnectPanel;
        public Visibility VkConnectPanel
        {
            get { return _vkConnectPanel; }
            set { _vkConnectPanel = value; OnPropertyChanged(); }
        }
        //User Button Visibility
        private Visibility _vkUserVisibility;
        public Visibility VkUserVisibility
        {
            get { return _vkUserVisibility; }
            set { _vkUserVisibility = value; OnPropertyChanged(); }
        }
        //VK Login Panel Visibility
        private Visibility _vkLoginPanelVisibility = Visibility.Hidden;
        public Visibility VkLoginPanelVisibility
        {
            get { return _vkLoginPanelVisibility; }
            set { _vkLoginPanelVisibility = value; OnPropertyChanged(); }
        }
        //Login button Visibillity
        private Visibility _vkLoginButtonVisibility;
        public Visibility VkLoginButtonVisibility
        {
            get { return _vkLoginButtonVisibility; }
            set { _vkLoginButtonVisibility = value; OnPropertyChanged(); }
        }

        //Button Eneble and boreder enable
        private bool _isVkUserLogin;
        public bool IsVkUserLogin
        {
            get { return _isVkUserLogin; }
            set { _isVkUserLogin = value; OnPropertyChanged(); }
        }

        //StackPanel content
        private string _vkConnectionContent = "Not Connected";
        public string VkConnectionContent
        {
            get { return _vkConnectionContent; }
            set { _vkConnectionContent = value; OnPropertyChanged(); }
        }
        //Button content
        private string _vkUserButtonContent;
        public string VkUserButtonContent
        {
            get { return _vkUserButtonContent; }
            set { _vkUserButtonContent = value; OnPropertyChanged(); }
        }

        //Enable border when login //color change
        private bool _vkLoginEnabled = true;
        public bool VkLoginEnabled
        {
            get { return _vkLoginEnabled; }
            set { _vkLoginEnabled = value; OnPropertyChanged(); }
        }


        public void ShowVkLoginPanel()
        {
            GridBlurRadius = 20;
            VkLoginPanelVisibility = Visibility.Visible;
        }
        public async void VkLogin()
        {
            VkSpinVisibility = Visibility.Visible;
            VkConnectionContent = "Connecting...";
            VkLoginPanelVisibility = Visibility.Hidden;
            await Task.Delay(500);

            VkUser = await vk.Auth(VkLoginText, VkPasswordText);
            if (VkUser != null)
            {
                VkLoginText = "";
                VkPasswordText = "";

                VkConnectPanel = Visibility.Hidden;

                VkUserButtonContent = VkUser.FullName;
                VkUserVisibility = Visibility.Visible;
                VkLoginButtonVisibility = Visibility.Hidden;

                IsVkUserLogin = true;
                GridBlurRadius = 0;
                MainPageViewModel.Messages.Add(new DuckMessage("Поздравляю! Вы подключили Ducky к VK! Как использовать VK для управления? Все очень просто! Зайдите в VK и перейдите в сообщество https://vk.com/ducky_bot! Далее нажмите на кнопку 'Написать сообщение' если вам не прищло сообщение в ЛС ", "Ducky"));
                await vk.BotStartMessageRecive();
            }
            else
            {
                IsVkUserLogin = false;
                VkSpinVisibility = Visibility.Collapsed;
                VkLoginPanelVisibility = Visibility.Visible;
                VkConnectionContent = "Not Connected";
            }
            VkLoginEnabled = true;
        }
        public void VkLogout()
        {
            Properties.Socials.Default.VkUserId = 0;
            VkConnectionContent = "Not connected";
            VkConnectPanel = Visibility.Visible;
            VkLoginButtonVisibility = Visibility.Visible;
            VkUserVisibility = Visibility.Hidden;
            IsVkUserLogin = false;
            Properties.Socials.Default.Save();
        }
        public void ShowVkProfile()
        {
            GridBlurRadius = 20;
            try
            {
                User = (new BaseUserModel
                {
                    social = "Vk",
                    UserName = VkUser.FullName,
                    UserNickName = "",
                    UserPhoto = VkUser.photo
                });
                UserInfoPanelVisibility = Visibility.Visible;
            }
            catch { }
        }
        #endregion

        #region UserPanel
        //User info Panel Visibility
        private Visibility _userInfoPanelVisibility = Visibility.Hidden;
        public Visibility UserInfoPanelVisibility
        {
            get { return _userInfoPanelVisibility; }
            set { _userInfoPanelVisibility = value; OnPropertyChanged(); }
        }
        //Hide User Info Panel
        private ICommand _userInfoCancleCommand;
        public ICommand UserInfoCancleCommand
        {
            get { return _userInfoCancleCommand ?? (_userInfoCancleCommand = new RelayCommand(p => UserInfoCancle())); }
        }
        //User info Model
        private BaseUserModel _user;
        public BaseUserModel User
        {
            get { return _user; }
            set { _user = value;OnPropertyChanged(); }
        }

        //Logout 
        private ICommand _logoutCommand;
        public ICommand LogoutCommand
        {
            get { return _logoutCommand ?? (_logoutCommand = new RelayCommand(p => Logout())); }
        }

        private void  UserInfoCancle()
        {
            GridBlurRadius = 0 ;
            UserInfoPanelVisibility = Visibility.Hidden;
        }
        private void Logout()
        {
            if (User.social == "Vk")
                VkLogout();
            else if (User.social == "telegram")
                TelegramLogout();
            else if (User.social == "twitch")
                TwitchLogout();

            UserInfoPanelVisibility = Visibility.Hidden;
            GridBlurRadius = 0 ;
        }
        #endregion

        private async void IsTelegramLogin()
        {
            TgUser = tg.IsUserAuth();

            if (TgUser != null)
            {
                TelegramUserButtonContent = "@" + TgUser.UserNickName;
                TelegramLoginVisibility = Visibility.Hidden;
                TelegramConnectPanel = Visibility.Hidden;
                IsTelegramEnabled = true;
            }
            else
            {
                TelegramConnectionContent = "Not connected";
                TelegramLoginVisibility = Visibility.Visible;
                IsTelegramEnabled = false;
            }
            ProxyConnectingVisibility = Visibility.Visible;
            await tg.CreateConnection();
            IsProxyEnable =  await telebot.TryToConnect();
            ProxyConnectingVisibility = Visibility.Hidden;
        }

        private async void IsTwitchLogin()
        {
            TwitchUser = await th.IsAuth();
            TwitchSpinnerVisibility = Visibility.Collapsed;
            if (TwitchUser != null)
            {
                twitchpage.GetStreamers();
                TwitchUserButtonContent = TwitchUser.UserName;
                IsTwitchEnabled = true;
                TwitchLoginlVisibility = Visibility.Hidden;
                TwitchConnectPanel = Visibility.Hidden;
            }
            else
            {
                TwitchConnectionContent = "Not connected";
                IsTwitchEnabled = false;
                TwitchLoginlVisibility = Visibility.Visible;
            }
        }

        private void IsVkLogin()
        {

        }

        #endregion

    }
}
