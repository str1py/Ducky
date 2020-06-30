using Ducky.ViewModel;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace Ducky.Model
{
    public class TelegramUserModel : ViewModelBase
    {
        private BitmapImage _userPhoto;
        public BitmapImage UserPhoto
        {
            get { return _userPhoto; }
            set { _userPhoto = value; OnPropertyChanged(); }
        }
        public string UserID;
        private string _userNameAndSurname;
        public string UserNameAndSurname
        {
            get { return _userNameAndSurname; }
            set { _userNameAndSurname = value; OnPropertyChanged(); }
        }
        public string UserNickName;

    }
}
