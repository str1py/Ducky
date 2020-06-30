using Ducky.ViewModel;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ducky.Model
{
    public class TwitchUserModel :ViewModelBase
    {
        public string UserId { get; set; }
        private string _username;
        public string UserName
        {
            get { return _username; }
            set { _username = value; OnPropertyChanged(); }
        }

        public bool Partnered { get; set; }
        public string Type { get; set; }
        public string email { get; set; }
        public List<string> Fol_channels { get; set; }
        public List<string> Sub_channels { get; set; }

        private BitmapImage _userImage;
        public BitmapImage UserImage
        {
            get { return _userImage; }
            set { _userImage = value; OnPropertyChanged(); }
        }
    }
}
