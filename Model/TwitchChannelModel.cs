using Ducky.ViewModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ducky.Model
{
    public class TwitchChannelModel:ViewModelBase
    {
        public string _id { get; set; }
        public string display_name { get; set; }
        public string url { get; set; }
        public string logo { get; set; }
        public BitmapImage logo_image { get; set; }
        public Visibility issub { get; set; }
        public string subTier { get; set; }
        public string broadcast_discr { get; set; }
        public Visibility partner { get; set; }
        private string Game;
        public string game
        {
            get { return Game; }
            set { Game = value; OnPropertyChanged(); }
        }
         
        private string _channel_status;
        public string channel_status
        {
            get { return _channel_status; }
            set { _channel_status = value; OnPropertyChanged(); }
        }

        private string _dotcolor;
        public string dotcolor
        {
            get { return _dotcolor; }
            set { _dotcolor = value; OnPropertyChanged(); }
        }
        private string _viewers;
        public string viewers
        {
            get { return _viewers; }
            set { _viewers = value; OnPropertyChanged(); }
        }
        private string _livetime;
        public string livetime
        {
            get { return _livetime; }
            set { _livetime = value; OnPropertyChanged(); }
        }

    }
}
