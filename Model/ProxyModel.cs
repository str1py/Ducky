using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ducky.Model
{
    public class ProxyModel: ViewModelBase
    {
        public string ProxyType { get; set; }
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public string IpAndPort { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        private string _state;
        public string State
        {
            get { return _state; }
            set { _state = value; OnPropertyChanged(); }
        }
        private string _isSelectedVisibility;
        public string IsSelectedVisibility
        {
            get { return _isSelectedVisibility; }
            set { _isSelectedVisibility = value; OnPropertyChanged(); }
        }
        private string _isConnectingVisibility;
        public string IsConnectingVisibility
        {
            get { return _isConnectingVisibility; }
            set { _isConnectingVisibility = value;OnPropertyChanged(); }
        }
    }
}
