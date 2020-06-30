using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace Ducky.Model
{
    public class SongModel :ViewModelBase
    {
        public string Title { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public string FullTitle {get;set; }
        public string FullArtist { get; set;}
        public string FullAlbum { get; set; }
        public string SongTime { get; set; }
        public string SongPath { get; set; }
        public int SliderMaximum { get; set; }

        private Visibility _playVisibility;
        public Visibility PlayVisibility {
            get { return _playVisibility; }
            set {
                _playVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _pauseVisibility;
        public Visibility PauseVisibility
        {
            get { return _pauseVisibility; }
            set
            {
                _pauseVisibility = value;
                OnPropertyChanged();
            }
        }
    }
}
