using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ducky.Model
{
    public class VkUserModel : ViewModelBase
    {
        public int userId { get; set; }
        private BitmapImage _photo;
        public BitmapImage photo
        {
            get { return _photo; }
            set { _photo = value; OnPropertyChanged(); }
        }

        public string userName { get; set; }
        public string userSurname { get; set; }
        public string userToken { get; set; }
        public string screenName { get; set; }
        private string _fullName;
        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; OnPropertyChanged(); }
        }
    }
}
