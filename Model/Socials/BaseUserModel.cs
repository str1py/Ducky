using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Ducky.Model.Socials
{
    public class BaseUserModel : ViewModelBase
    {
        public string social;
        private ImageSource _userPhoto;
        public ImageSource UserPhoto
        {
            get { return _userPhoto; }
            set { _userPhoto = value; OnPropertyChanged(); }
        }
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { _userName = value; OnPropertyChanged(); }
        }
        private string _userNickName;
        public string UserNickName
        {
            get { return _userNickName; }
            set { _userNickName = value;OnPropertyChanged(); }
        }
    }
}
