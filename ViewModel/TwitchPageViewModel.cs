using Ducky.Helpers;
using Ducky.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Ducky.ViewModel
{
    public class TwitchPageViewModel : DependencyObject, INotifyPropertyChanged
    {
        #region Fields
        public ObservableCollection<TwitchChannelModel> Streams { get; set; }
        DispatcherTimer dispatcherTimer;
        TwitchHelper th;
        #endregion

        public TwitchPageViewModel()
        {
            dispatcherTimer = new DispatcherTimer();
            th = MainWindowViewModel.GetTwitchHelper();
        }

        #region StreamCollection
        public ICollectionView StreamCollection
        {
            get => (ICollectionView)GetValue(StreamCollectionProperty);

            set { SetValue(StreamCollectionProperty, value); }
        }
        private bool ShowAllFilter(object ob)
        {
            bool result = true;
            TwitchChannelModel current = ob as TwitchChannelModel;
            return result;
        }
        private bool OnlineRerunFilter(object ob)
        {
            bool result = true;
            TwitchChannelModel current = ob as TwitchChannelModel;
            if (current.channel_status == "offline")
                result = false;

            return result;
        }
        private bool OnlineFilter(object ob)
        {
            bool result = true;
            TwitchChannelModel current = ob as TwitchChannelModel;
            if (current.channel_status == "offline" || current.channel_status == "rerun")
                result = false;

            return result;
        }

        private bool _allFilterChecked;
        public bool AllFilterChecked
        {
            get { return _allFilterChecked; }
            set { _allFilterChecked = value; OnPropertyChanged();
                if (AllFilterChecked)
                    StreamCollection.Filter = ShowAllFilter;
            }
        }
        private bool _onlineRerunFilterChecked;
        public bool OnlineRerunFilterChecked
        {
            get { return _onlineRerunFilterChecked; }
            set { _onlineRerunFilterChecked = value; OnPropertyChanged();

                if(OnlineRerunFilterChecked)
                    StreamCollection.Filter = OnlineRerunFilter;
            }
        }
        private bool _onlineFilterChecked;
        public bool OnlineFilterChecked
        {

            get { return _onlineFilterChecked; }
            set { _onlineFilterChecked = value; OnPropertyChanged();
                if (OnlineFilterChecked)
                    StreamCollection.Filter = OnlineFilter;   
            }
        }

        private string _allFilterContent;
        public string AllFilterContent
        {
            get { return _allFilterContent; }
            set { _allFilterContent = value; OnPropertyChanged(); }
        }
        private string _onlineRerunFilterContent;
        public string OnlineRerunFilterContent
        {
            get { return _onlineRerunFilterContent; }
            set { _onlineRerunFilterContent = value; OnPropertyChanged(); }
        }
        private string _onlineFilterContent;
        public string OnlineFilterContent
        {
            get { return _onlineFilterContent; }
            set { _onlineFilterContent = value; OnPropertyChanged(); }
        }

        private Visibility _filtersVisibility = Visibility.Hidden;
        public Visibility FiltersVisibility
        {
            get { return _filtersVisibility; }
            set { _filtersVisibility = value; OnPropertyChanged(); }
        }

        public static readonly DependencyProperty StreamCollectionProperty = DependencyProperty.Register("StreamCollection",
        typeof(ICollectionView),
        typeof(TwitchPageViewModel),
        new PropertyMetadata(null));
        #endregion

        #region Properties
        private Visibility _loadVisibility = Visibility.Hidden;
        public Visibility LoadVisibility
        {
            get { return _loadVisibility; }
            set { _loadVisibility = value; OnPropertyChanged(); }
        }
        #endregion

        public async void GetStreamers()
        {
            LoadVisibility = Visibility.Visible;

            int online = 0;
            int rerun = 0;

            try
            {
                Streams = await th.GetFollows();
            }
            catch
            {
                App.Current.Dispatcher.Invoke((Action)async delegate { Streams = await th.GetFollows(); });
            }
          
            foreach(TwitchChannelModel twitch in Streams)
            {
                if (twitch.channel_status == "rerun")
                    rerun++;
                if (twitch.channel_status == "live")
                    online++;
            }
            StreamCollection = CollectionViewSource.GetDefaultView(Streams);

            int onlinererun = online + rerun;

            AllFilterContent = $"Отслеживаемые каналы {Streams.Count}";
            OnlineRerunFilterContent = $"Каналов в сети/повторе {onlinererun}";
            OnlineFilterContent = $"Каналов в сети {online}";
            OnlineFilterChecked = true;
            LoadVisibility = Visibility.Hidden;
            FiltersVisibility = Visibility.Visible;
            TimerStart();
        }
        private async void Update()
        {
            if (InternetConnection.IsConnectionExist)
            {
                foreach (TwitchChannelModel twitch in Streams)
                {
                    var info = await th.UpdateStreamInfo(twitch);
                    twitch.game = info.game;
                    twitch.livetime = info.livetime;
                    twitch.channel_status = info.channel_status;
                    twitch.dotcolor = info.dotcolor;
                    twitch.viewers = info.viewers;
                }
                int online = 0;
                int rerun = 0;
                foreach (TwitchChannelModel twitch in Streams)
                {
                    if (twitch.channel_status == "rerun")
                        rerun++;
                    if (twitch.channel_status == "live")
                        online++;
                }

                int onlinererun = online + rerun;
                AllFilterContent = $"Отслеживаемые каналы {Streams.Count}";
                OnlineRerunFilterContent = $"Каналов в сети/повторе {onlinererun}";
                OnlineFilterContent = $"Каналов в сети {online}";
            }
            else
            {
                foreach (TwitchChannelModel twitch in Streams)
                {
                    twitch.game = "";
                    twitch.channel_status = "Unavalible";
                    twitch.dotcolor = "";
                    twitch.viewers = "";
                }
                FiltersVisibility = Visibility.Hidden;
            }
        }
        public void UserLogOut()
        {
            FiltersVisibility = Visibility.Hidden;
            TimerStop();
            Streams?.Clear();
            StreamCollection?.Refresh();
        }

        #region Timer
        private void TimerStart()
        {
            dispatcherTimer.Tick += new EventHandler(TimerTick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2, 0, 0);
            dispatcherTimer.Start();
        }
        private void TimerStop()
        {
            dispatcherTimer.Stop();
            dispatcherTimer.IsEnabled = false;
        }
        public void TimerTick(object sender, EventArgs e)
        {
            Update();
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
