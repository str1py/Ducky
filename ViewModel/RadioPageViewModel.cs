using Ducky.Helpers;
using Ducky.Helpers.Radio;
using Ducky.Model;
using Ducky.Model.Radio;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Ducky.ViewModel
{
    public class RadioPageViewModel : ViewModelBase
    {
        private DispatcherTimer UpdateStationInfo;
        StationFill fill;
        StationToPlay st;
        private BassNetHelper bh;
        private LogsPageViewModel log;
        public ObservableCollection<RadioCategories> Categories { get; set; } = new ObservableCollection<RadioCategories>();
        public ObservableCollection<Station> StationList { get; set; }  = new ObservableCollection<Station>();

        public RadioPageViewModel()
        {
            #region Init
            UpdateStationInfo = new DispatcherTimer();
            fill = new StationFill();
            st = new StationToPlay();
            bh = MainWindowViewModel.GetBassNetHelper();
            log = MainWindowViewModel.GetLogVM();
            #endregion

            InitCategories();
       
            CurrentStation = (new PlayingStation
            {
                RadioLogo = new BitmapImage(new Uri("pack://application:,,,/Resources/Radio/custom.jpg")),
                RadioName = "No selected radio"
            });
            CategoryIndex = 0;
        }

        private int _categoryIndex = 0;
        public int CategoryIndex
        {
            get { return _categoryIndex; }
            set {
                _categoryIndex = value;
                OnPropertyChanged();

                switch (CategoryIndex)
                {
                    case 0:
                        StationList = fill.RecordStationFill();
                        break;
                    case 1:
                        StationList = fill.MoscowStationFill();
                        break;
                    case 2:
                        StationList = fill.BBCStationFill();
                        break;
                }
            }
        }
        private int _stationIndex;
        public int StationIndex
        {
            get { return _stationIndex; }
            set { _stationIndex = value; OnPropertyChanged(); }
        }
        private PlayingStation _currentStation;
        public PlayingStation CurrentStation
        {
            get { return _currentStation; }
            set { _currentStation = value;OnPropertyChanged(); }
        }

        private ICommand _stationDoubleClickCommand;
        public ICommand StationDoubleClickCommand
        {
            get { return _stationDoubleClickCommand ?? (_stationDoubleClickCommand = new RelayCommand(p => PlayStation())); }
        }
        private ICommand _playRadioCommand;
        public ICommand PlayRadioCommand
        {
            get { return _playRadioCommand ?? (_playRadioCommand = new RelayCommand(p => ContinuePlay())); }
        }
        private ICommand _pauseRadioCommand;
        public ICommand PauseRadioCommand
        {
            get { return _pauseRadioCommand ?? (_pauseRadioCommand = new RelayCommand(p => StopPlay())); }
        }
        private ICommand _songLikeCommand;
        public ICommand SongLikeCommand
        {
            get { return _songLikeCommand ?? (_songLikeCommand = new RelayCommand(p => SongLike())); }
        }

        private Visibility _radioSpinVisibility = Visibility.Hidden;
        public Visibility RadioSpinVisibility
        {
            get { return _radioSpinVisibility; }
            set { _radioSpinVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _playingVisibility = Visibility.Visible;
        public Visibility PlayingVisibility
        {
            get { return _playingVisibility; }
            set { _playingVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _pauseVisibility = Visibility.Hidden;
        public Visibility PauseVisibility
        {
            get { return _pauseVisibility; }
            set { _pauseVisibility = value; OnPropertyChanged(); }
        }

        private async void ContinuePlay()
        {
            if (CurrentStation.URL == null)
            {
                CurrentStation = await st.GetStationInfo(0, 0);
                bh.PlayFromURL(CurrentStation.URL, Properties.Settings.Default.UserVolume);
            }else
            {
                bh.PlayFromURL(CurrentStation.URL, Properties.Settings.Default.UserVolume);
            }
            PlayingVisibility = Visibility.Hidden;
            PauseVisibility = Visibility.Visible;
            timerStart();
        }
        private void StopPlay()
        {
            timerStop();
            bh.StopUrlStream();
            PlayingVisibility = Visibility.Visible;
            PauseVisibility = Visibility.Hidden;
        }
        private async void PlayStation()
        {
            timerStart();
            RadioSpinVisibility = Visibility.Visible;
            //CurrentStation.RadioLogo = empt;
          // CurrentStation.RadioSongName = "buffering...";
            try
            {
                bh.StopUrlStream();
                CurrentStation = await st.GetStationInfo(CategoryIndex, StationIndex);
                bh.PlayFromURL(CurrentStation.URL, Properties.Settings.Default.UserVolume);
            }
            catch (Exception e )
            {
                log.AddLog("ERROR", e.Message);
            }
            RadioSpinVisibility = Visibility.Hidden;
            PlayingVisibility = Visibility.Hidden;
            PauseVisibility = Visibility.Visible;
        }

        private void InitCategories()
        {
            Categories.Add(new RadioCategories()
            {
                RadioName = "Record Radio",
                RadioPic = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Radio/record.jpeg"))
            });
            Categories.Add(new RadioCategories()
            {
                RadioName = "Russian Radios",
                RadioPic = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Radio/russia.png"))
            }) ;
            Categories.Add(new RadioCategories()
            {
                RadioName = "BBC Radio",
                RadioPic = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Radio/bbc.jpg"))
            });
            Categories.Add(new RadioCategories()
            {
                RadioName = "Custom Radios",
                RadioPic = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Radio/custom.jpg"))
            });
        }

        private async void Update()
        {
            try
            {
                CurrentStation = await st.GetStationInfo(CategoryIndex, StationIndex);
            }
            catch (Exception e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                log.AddLog("ERROR : ", e.Message, "Error in ", m.DeclaringType.Name);
            }
        }

        private void SongLike()
        {
            //artist,title,spotify,ya.music,itunes,dezeer
    
            if (CurrentStation != null && (CurrentStation.RadioArtist != "" || CurrentStation.RadioSongName != ""))
            {
                string str = $"{CurrentStation.RadioArtist};{CurrentStation.RadioSongName};{CurrentStation.SpotifyLink};{CurrentStation.YamusicLink};{CurrentStation.ItunesLink};{CurrentStation.DeezerLink}";
              //  Properties.Settings.Default.LikedSongs = new System.Collections.Specialized.StringCollection();
                if (Properties.Settings.Default.LikedSongs.Contains(str))
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Alerts/antenna.png")), "Radio", "This song already contains in your list");
                else
                {
                    MainWindowViewModel.instance.Alert(new BitmapImage(new Uri("pack://application:,,,/Resources/Alerts/antenna.png")), "Radio", $"{CurrentStation.RadioArtist}-{CurrentStation.RadioSongName} added to your liked songs list!");
                    Properties.Settings.Default.LikedSongs.Add(str);
                    Properties.Settings.Default.Save();
                }
            }
         

        }

        private void timerStart()
        {
            UpdateStationInfo.Tick += new EventHandler(timerTick);
            UpdateStationInfo.Interval = new TimeSpan(0, 0, 0, 30, 0);
            UpdateStationInfo.Start();
        }
        private void timerStop()
        {
            UpdateStationInfo.Stop();
        }
        private void timerTick(object sender, EventArgs e)
        {
            Update();
        }


    }
}
