using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.SplashScreen
{
    class SplashScreenViewModel : ViewModelBase
    {
        public SplashScreenViewModel()
        {
            Load();          
        }

        private async Task Load()
        {
            SplashScreenText = "Creating pond...";
            await Task.Delay(1000);
            SplashScreenText = "Creating ducklings...";
            await Task.Delay(1000);
            splashScreenText = "Watching ducklings swim in the pond";
            await Task.Delay(1000);
            SplashScreenText = "Awing...";
            await Task.Delay(1000);
        }

        private string splashScreenText = "Initializing...";
        public string SplashScreenText
        {
            get { return splashScreenText; }
            set { splashScreenText = value; OnPropertyChanged();}
        }
    }
}
