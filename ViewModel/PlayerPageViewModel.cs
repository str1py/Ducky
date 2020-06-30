using Ducky.Model;
using System;
using System.Windows;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ducky.Helpers;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Input;
using System.Linq;
using Un4seen.Bass;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Ducky.ViewModel
{

    public class PlayerPageViewModel : DependencyObject, INotifyCollectionChanged, INotifyPropertyChanged
    {
       
        #region Fields
        private ObservableCollection<SongModel> Music { get; set; }
        private List<string> Files { get; set; }
        private readonly SongInfo s_info;
        private readonly DispatcherTimer dispatcherTimer;
        private readonly AudioVizualHelpers au;
        private readonly SearchMusicResources smr;
        readonly BassNetHelper bh;
        readonly LogsPageViewModel log;
        SongModel PreviousSong { get; set; }

        #endregion

        public PlayerPageViewModel()
        {
            #region Init
            Music = new ObservableCollection<SongModel>();
            Files = new List<string>();
            dispatcherTimer = new DispatcherTimer();
            au = new AudioVizualHelpers();
            smr = new SearchMusicResources();
            s_info = new SongInfo();
            bh = MainWindowViewModel.GetBassNetHelper();
            log = MainWindowViewModel.GetLogVM();
            #endregion

            MusicCollection = CollectionViewSource.GetDefaultView(AddMusic(Properties.Settings.Default.MusicFolders));

            MusicCollection.Filter = MusicFilter;

            SliderVolumePos = Properties.Settings.Default.UserVolume;
        }

        #region Dependency Filter

        public string FilterText
        {
            get { return (string)GetValue(FilterTextProperty); }
            set { SetValue(FilterTextProperty, value); }
        }
        public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register("FilterText",
            typeof(string),
            typeof(PlayerPageViewModel),
            new PropertyMetadata("", FilterText_Changed));

        private static void FilterText_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var current = d as PlayerPageViewModel;
            if (d != null)
            {
                current.MusicCollection.Filter = null;
                current.MusicCollection.Filter = current.MusicFilter;
            }
        }

        public ICollectionView MusicCollection
        {
            get => (ICollectionView)GetValue(MusicCollectionProperty);

            set { SetValue(MusicCollectionProperty, value); }
        }
        public static readonly DependencyProperty MusicCollectionProperty = DependencyProperty.Register("MusicCollection",
           typeof(ICollectionView),
           typeof(PlayerPageViewModel),
           new PropertyMetadata(null));

        #endregion

        #region Properties

        #region MUSICDATA
        private SongModel _selectedItem;
        public SongModel SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; OnPropertyChanged(); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; OnPropertyChanged(); }
        }
        private string _artist;
        public string Artist
        {
            get { return _artist; }
            set { _artist = value; OnPropertyChanged(); }
        }
        private string _fullSongTime;
        public string FullSongTime
        {
            get { return _fullSongTime; }
            set { _fullSongTime = value; OnPropertyChanged(); }
        }
        private string _songTimeNow;
        public string SongTimeNow
        {
            get { return _songTimeNow; }
            set { _songTimeNow = value; OnPropertyChanged(); }
        }


        private BitmapImage _songImage;
        public BitmapImage SongImage
        {
            get { return _songImage; }
            set { _songImage = value; OnPropertyChanged(); }
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
        #endregion

        #region MusicResources
        private string _youTubeLink;
        public string YouTubeLink
        {
            get { return _youTubeLink; }
            set { _youTubeLink = value; OnPropertyChanged(); }
        }
        private string _yaMusicLink;
        public string YaMusicLink
        {
            get { return _yaMusicLink; }
            set { _yaMusicLink = value; OnPropertyChanged(); }
        }
        private string _appleMusic;
        public string AppleMusic
        {
            get { return _appleMusic; }
            set { _appleMusic = value; OnPropertyChanged(); }
        }
        #endregion

        #region Slider
        private float _sliderPos;
        public float SliderPos
        {
            get { return _sliderPos; }
            set { _sliderPos = value; OnPropertyChanged(); }
        }
        private double _sliderMax;
        public double SliderMax
        {
            get { return _sliderMax; }
            set { _sliderMax = value; OnPropertyChanged(); }
        }
        private float _sliderVolumePos;
        public float SliderVolumePos
        {
            get { return _sliderVolumePos; }
            set { _sliderVolumePos = value; OnPropertyChanged(); }
        }
        #endregion

        #region VizualEffects
        private double _shadowRadius;
        public double ShadowRadius
        {
            get { return _shadowRadius; }
            set { _shadowRadius = value; OnPropertyChanged(); }
        }
        private double _blurRadius;
        public double BlurRadius
        {
            get { return _blurRadius; }
            set { _blurRadius = value; OnPropertyChanged(); }
        }
        private string _shadowColor;
        public string ShadowColor
        {
            get { return _shadowColor; }
            set { _shadowColor = value; OnPropertyChanged(); }
        }
        private double _imageSize;
        public double ImageSize
        {
            get { return _imageSize; }
            set { _imageSize = value; OnPropertyChanged(); }
        }
        #endregion

        #region ViewVisibilityProperties
        private Visibility _detailedMusicGridVisibility = Visibility.Visible;
        public Visibility DetailedMusicGridVisibility
        {
            get { return _detailedMusicGridVisibility; }
            set { _detailedMusicGridVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _minimalMusicGridVisibility = Visibility.Hidden;
        public Visibility MinimalMusicGridVisibility
        {
            get { return _minimalMusicGridVisibility; }
            set { _minimalMusicGridVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _youtubeMusicVisibility = Visibility.Hidden;
        public Visibility YoutubeMusicVisibility
        {
            get { return _youtubeMusicVisibility; }
            set { _youtubeMusicVisibility = value; OnPropertyChanged(); }
        }
        #endregion

        #endregion

        #region Commands
        private ICommand _dataGridDoubleClick;
        public ICommand DataGridDoubleClick
        {
            get { return _dataGridDoubleClick ?? (_dataGridDoubleClick = new RelayCommand(p => PlayFromGrid())); }
        }

        private ICommand _playCommand;
        public ICommand PlayCommand
        {
            get { return _playCommand ?? (_playCommand = new RelayCommand(p => Play())); }
        }
        private ICommand _pauseCommand;
        public ICommand PauseCommand
        {
            get { return _pauseCommand ?? (_pauseCommand = new RelayCommand(p => Pause())); }
        }
        private ICommand _nextCommand;
        public ICommand NextCommand
        {
            get { return _nextCommand ?? (_nextCommand = new RelayCommand(p => Next())); }
        }
        private ICommand _prevCommand;
        public ICommand PrevCommand
        {
            get { return _prevCommand ?? (_prevCommand = new RelayCommand(p => Previous())); }
        }

        private ICommand _playCommandFromGrid;
        public ICommand PlayCommandFromGrid
        {
            get { return _playCommandFromGrid ?? (_playCommandFromGrid = new RelayCommand(p => PlayFromGrid())); }
        }
        private ICommand _pauseCommandFromGrid;
        public ICommand PauseCommandFromGrid
        {
            get { return _pauseCommandFromGrid ?? (_pauseCommandFromGrid = new RelayCommand(p => Pause())); }
        }

        private ICommand _slideCompletedCommand;
        public ICommand SlideCompletedCommand
        {
            get { return _slideCompletedCommand ?? (_slideCompletedCommand = new RelayCommand(p => SlideCompleted())); }
        }
        private ICommand _slideVolumeCommand;
        public ICommand SlideVolumeCommand
        {
            get { return _slideVolumeCommand ?? (_slideVolumeCommand = new RelayCommand(p => SlideSetVolume())); }
        }
        private ICommand _slideDragStartedCommand;
        public ICommand SlideDragStartedCommand
        {
            get { return _slideDragStartedCommand ?? (_slideDragStartedCommand = new RelayCommand(p => DragStart())); }
        }

        private ICommand _changeViewCommand;
        public ICommand ChangeViewCommand
        {
            get { return _changeViewCommand ?? (_changeViewCommand = new RelayCommand(p => SetGridVisibility())); }
        }

        #endregion

#region Functions
        public async void SetMusicData()
        {
            try
            {
                Title = SelectedItem.FullTitle;
                Artist = SelectedItem.FullArtist;
                FullSongTime = SelectedItem.SongTime;
                SongImage = s_info.GetSongImage(SelectedItem.SongPath);
                SliderMax = SelectedItem.SliderMaximum;
                ShadowColor = await Task.Run(() => au.GetShadowColor(SongImage));
                log.AddLog("SetMusicData. Title - ", Title, " Artist - ", Artist);

                YouTubeLink = await smr.GetYouTubeLink(Title, Artist);

                SetVisibility(Visibility.Hidden, Visibility.Visible);
            }
            catch (Exception e)
            {
                log.AddLog("ERROR : ", e.Message, "Error in ", e.TargetSite.Name);
            }
        }
        public async void SetMusicData(SongModel song)
        {
            try
            {
                Title = song.FullTitle;
                Artist = song.FullArtist;
                FullSongTime = song.SongTime;
                SongImage = s_info.GetSongImage(song.SongPath);
                SliderMax = song.SliderMaximum;
                ShadowColor = await Task.Run(() => au.GetShadowColor(SongImage));
                log.AddLog("SetMusicData. Title - ", song.Title, " Artist - ", song.Artist);

                SetVisibility(Visibility.Hidden, Visibility.Visible);
            }
            catch (Exception e)
            {
                log.AddLog("ERROR : ", e.Message, "Error in ", e.TargetSite.Name);
            }
        }
        private ObservableCollection<SongModel> AddMusic(StringCollection fl)
        {
            Music.Clear();
            Files.Clear();
            if (fl != null)
            {
                foreach (string st in fl)
                    Files.AddRange(Directory.GetFiles(st, "*.mp3"));

                log.AddLog("Добавлена музыка из ", fl.Count.ToString());

                foreach (string str in Files)
                {
                    string[] info = s_info.GetSongInfo(Path.GetFileName(str)); //Get filename without path
                    using (var audioFile = TagLib.File.Create(str))
                    {
                        Music.Add(new SongModel
                        {
                            Title = s_info.TitleEditor(audioFile.Tag?.Title) ?? s_info.TitleEditor(info[1]),
                            FullTitle = audioFile.Tag?.Title ?? info[1],
                            Artist = s_info.ArtistEditor(audioFile.Tag?.FirstPerformer) ?? s_info.ArtistEditor(info[0]),
                            FullArtist = audioFile.Tag?.FirstPerformer ?? info[0],
                            Album = audioFile.Tag?.Album ?? "no album",
                            //  FullAlbum = audioFile.Tag?.Album ?? "no album",
                            SongPath = str,
                            SongTime = s_info.GetSongTime(audioFile.Properties.Duration.ToString()),
                            SliderMaximum = bh.GetTimeOfStream(str),
                            PlayVisibility = Visibility.Visible,
                            PauseVisibility = Visibility.Hidden
                        });
                    }
                }
                log.AddLog("Добавлено треков - ", Files.Count().ToString());
                OnCollectionChanged(NotifyCollectionChangedAction.Reset);
            }
            return Music;
        } 

        #region SlidersMethods
        private void SlideCompleted()
        {
            bh.SetPosOfScroll(bh.Stream, SliderPos);
            bh.dragStarted = false;
        }
        private void SlideSetVolume()
        {
            bh.SetStreamVolume(bh.Stream, SliderVolumePos);
        }
       // НУЖНО ДЛЯ ТОГО ЧТОБЫ НЕ СКАКАЛ THUMB ПРИ УДЕРЖАНИИ
        private void DragStart()
        {
            bh.dragStarted = true;
        }
        #endregion

        #region PlayerMethods
        private bool ToNextTrack()
        {
            if (Bass.BASS_ChannelIsActive(bh.Stream) == BASSActive.BASS_ACTIVE_STOPPED && (!bh.isStopped))
            {
                var item = Music.IndexOf(PreviousSong);
                if (Files.Count > item + 1)
                {
                    item++;
                    bh.Stop();
                    SelectedItem = Music[item];
                    TimerStart();
                    bh.Play(SelectedItem.SongPath, Properties.Settings.Default.UserVolume);
                    SetMusicData();
                    SliderMax = bh.GetTimeOfStream(bh.Stream);
                    log.AddLog("Cейчас играет : ", SelectedItem.Title, " - ", SelectedItem.Artist);
                    PreviousSong = SelectedItem;
                    SetVisibility(Visibility.Hidden, Visibility.Visible);
                    bh.EndPlayList = false;
                    return true;
                }
                else
                    bh.EndPlayList = true;
            }
            return false;
        }
        private void Play()
        {
            if (PreviousSong?.SongPath != null)
            {
                if (File.Exists(PreviousSong?.SongPath))
                {
                    //Continue playing stop song
                    TimerStart();
                    bh.Play(PreviousSong.SongPath, Properties.Settings.Default.UserVolume);
                    SetMusicData(PreviousSong);
                    log.AddLog("Cейчас продлжает играть : ", PreviousSong.Title, " - ", PreviousSong.Artist);
                    //MainWindowViewModel.mvvm.Alert("/Resources/Images/duck-face.jpg", "Player", PreviousSong.Artist + " - "+PreviousSong.Title);
                }
                //else
                //    Music.Remove(Music.FirstOrDefault(x => x.SongPath == PreviousSong?.SongPath));
            }
        }
        private void PlayFromGrid()
        {
            if (PreviousSong?.SongPath == SelectedItem.SongPath)
            {
                //Continue playing stop song       
                TimerStart();
                bh.Play(PreviousSong.SongPath, Properties.Settings.Default.UserVolume);

                log.AddLog("Cейчас продлжает играть : ", PreviousSong.Title, " - ", PreviousSong.Artist);
                SetVisibility(Visibility.Hidden, Visibility.Visible);
            }
            else
            {
                bh.Stop();
                TimerStart();
                bh.Play(SelectedItem.SongPath, Properties.Settings.Default.UserVolume);
                SetMusicData();
                log.AddLog("Cейчас играет : ", SelectedItem.Title, " - ", SelectedItem.Artist);
                PreviousSong = SelectedItem;
                SetVisibility(Visibility.Hidden, Visibility.Visible);
            }
        }

        public void PlayPause()
        {
            if (bh.isStopped)
                Play();
            else
                Pause();
        }
        public void Pause()
        {
            TimerStop();
            bh.Pause();
            SetVisibility(Visibility.Visible, Visibility.Hidden);
            log.AddLog("Поставленно на паузу : ", SelectedItem.Title, " - ", SelectedItem.Artist);
        }
        public void Next()
        {
            var nextitem = Music.IndexOf(PreviousSong);
            nextitem++;
            if (nextitem < Music.Count)
            {
                if (Music?[nextitem]?.SongPath != null)
                {
                    bh.Stop();
                    SelectedItem = Music[nextitem];
                    bh.Play(SelectedItem.SongPath, Properties.Settings.Default.UserVolume);
                    TimerStart();
                    SetMusicData();

                    log.AddLog("Cейчас играет : ", Music[nextitem].Title, " - ", Music[nextitem].Artist);
                    PreviousSong = SelectedItem;
                    SliderMax = bh.GetTimeOfStream(bh.Stream);
                    SetVisibility(Visibility.Hidden, Visibility.Visible);
                }
            }
        }
        public void Previous()
        {
            var nextitem = Music.IndexOf(PreviousSong);
            nextitem--;
            if (nextitem >= 0)
            {
                if (Music?[nextitem]?.SongPath != null)
                {
                    bh.Stop();
                    TimerStart();
                    SelectedItem = Music[nextitem];
                    bh.Play(SelectedItem.SongPath, Properties.Settings.Default.UserVolume);
                    SetMusicData();

                    log.AddLog("Cейчас играет : ", Music[nextitem].Title, " - ", Music[nextitem].Artist);
                    PreviousSong = SelectedItem;
                    SliderMax = bh.GetTimeOfStream(bh.Stream);
                    SetVisibility(Visibility.Hidden, Visibility.Visible);
                }
            }
        }
        public void PlayByCommand()
        {
            if (PreviousSong?.SongPath != null)
            {
                TimerStart();
                bh.Play(PreviousSong.SongPath, Properties.Settings.Default.UserVolume);
                SetMusicData(PreviousSong);
            }
            else
            {
                TimerStart();
                SelectedItem = (Music[0]);
                bh.Play(Music[0].SongPath, Properties.Settings.Default.UserVolume);
                SetMusicData();
            }
        }
        public void IncreaseVolume()
        {
            int slider = (int)SliderVolumePos;
            if (slider != 100 && slider + 5 < 100)
            {
                SliderVolumePos = slider + 5;
                bh.SetStreamVolume(bh.Stream, SliderVolumePos);
            }
            else if (slider + 5 >= 100)
            {
                bh.SetStreamVolume(bh.Stream, SliderVolumePos);
                SliderVolumePos = 100;
            }
        }
        public void DicreaseVolume()
        {
            int slider = (int)SliderVolumePos;
            if (slider != 0 && slider - 5 > 0)
            {
                SliderVolumePos = slider - 5;
                bh.SetStreamVolume(bh.Stream, SliderVolumePos);

            }
            else if (slider - 10 < 0)
            {
                bh.SetStreamVolume(bh.Stream, 0);
                SliderPos = 0;
            }
        }
        #endregion

        //View
        private void SetGridVisibility()
        {
            if (MinimalMusicGridVisibility == Visibility.Visible)
            {
                DetailedMusicGridVisibility = Visibility.Visible;
                MinimalMusicGridVisibility = Visibility.Hidden;
            }
            else
            {
                DetailedMusicGridVisibility = Visibility.Hidden;
                MinimalMusicGridVisibility = Visibility.Visible;
            }
        }
        private void SetVisibility(Visibility playv, Visibility pausev)
        {
            Music.AsParallel().ForAll(x =>
            {
                x.PlayVisibility = Visibility.Visible;
                x.PauseVisibility = Visibility.Hidden;
            });
            var item = Music.FirstOrDefault(i => i.SongPath == SelectedItem.SongPath);
            if (item != null)
            {
                item.PlayVisibility = playv;
                item.PauseVisibility = pausev;
            }

            PlayingVisibility = playv;
            PauseVisibility = pausev;
        }

        public void OnFolderChange()
        {
            bh.Stop();
            Music.Clear();
            MusicCollection = CollectionViewSource.GetDefaultView(AddMusic(Properties.Settings.Default.MusicFolders));
            MusicCollection.Refresh();
        }

        #endregion

        #region SidePanel 
        private ICommand _youtubeCommand;
        public ICommand YoutubeCommand
        {
            get { return _youtubeCommand ?? (_youtubeCommand = new RelayCommand(p => YoutubeStart())); }
        }
        private ICommand _showHideSideCommand;
        public ICommand ShowHideSideCommand
        {
            get { return _showHideSideCommand ?? (_showHideSideCommand = new RelayCommand(p => SidePanelState())); }
        }

        private bool _isYoutubeEnable;
        public bool IsYoutubeEnable
        {
            get { return _isYoutubeEnable; }
            set { _isYoutubeEnable = value; OnPropertyChanged(); }
        }
        private bool _isSidePanelOn;
        public bool IsSidePanelOn
        {
            get { return _isSidePanelOn; }
            set { _isSidePanelOn = value; OnPropertyChanged(); }
        }

        private Visibility _showPanelButtonVisibility = Visibility.Visible;
        public Visibility ShowPanelButtonVisibility
        {
            get { return _showPanelButtonVisibility; }
            set { _showPanelButtonVisibility = value; OnPropertyChanged(); }
        }
        private Visibility _hidePanelButtonVisibility = Visibility.Hidden;
        public Visibility HidePanelButtonVisibility
        {
            get { return _hidePanelButtonVisibility; }
            set { _hidePanelButtonVisibility = value; OnPropertyChanged(); }
        }

        private async void YoutubeStart()
        {
            if (YouTubeLink != null)
            {
                Process.Start("chrome.exe", YouTubeLink + "&feature=youtu.be&t=" + s_info.GetSeconds(SongTimeNow));
                await Task.Delay(3000);
                Pause();
            }
            else
                MainWindowViewModel.instance.Alert(null, "Player", "Ссылка на песню не найдена или отсутствует подключение к интернету. Internet connection " + InternetConnection.IsConnectionExist);
        }
        private void SidePanelState()
        {
            if (ShowPanelButtonVisibility == Visibility.Visible)
            {
                ShowPanelButtonVisibility = Visibility.Hidden;
                HidePanelButtonVisibility = Visibility.Visible;
            }
            else
            {
                ShowPanelButtonVisibility = Visibility.Visible;
                HidePanelButtonVisibility = Visibility.Hidden;
            }
        }
        #endregion

        #region Timer
        private void TimerStart()
        {
            dispatcherTimer.Tick += new EventHandler(TimerTick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            dispatcherTimer.Start();
        }
        private void TimerStop()
        {
            dispatcherTimer.Stop();
            dispatcherTimer.IsEnabled = false;
        }
        public void TimerTick(object sender, EventArgs e)
        {
            //SONG TIME CHANGE
            SongTimeNow = s_info.GetSongTime(TimeSpan.FromSeconds(bh.GetStreamPos(bh.Stream)).ToString());
            Bass.BASS_ChannelGetData(bh.Stream, bh.buffer, (int)BASSData.BASS_DATA_FFT256);

            //VISUAL EFFECTS
            var vizual = au.AudioVizual(bh.buffer, BlurRadius, ShadowRadius, MinimalMusicGridVisibility, ImageSize, MainWindowViewModel.winstate);
            BlurRadius = vizual.Item1;
            ShadowRadius = vizual.Item2;
            ImageSize = vizual.Item3;

            //Drag Change
            if (bh.dragStarted == false)
                SliderPos = bh.GetStreamPos(bh.Stream);

            //NEXT TRACK 
            if (ToNextTrack()) { }
            if (bh.EndPlayList) { SelectedItem = Music[0]; bh.EndPlayList = false; }
        }
        #endregion

        private bool MusicFilter(object ob)
        {
            bool result = true;
            SongModel current = ob as SongModel;
            if (!string.IsNullOrWhiteSpace(FilterText) && current != null && !current.Artist.Contains(FilterText) && !current.Title.Contains(FilterText) && !current.Album.Contains(FilterText))
                result = false;

            return result;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
        public void OnCollectionChanged(NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
