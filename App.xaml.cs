using Ducky.SplashScreen;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;

namespace Ducky
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            SplashScreenView splashScreen = new SplashScreenView();
            splashScreen.Show();
      
            base.OnStartup(e);
            await Task.Delay(4000);
            MainWindow main = new MainWindow();
            main.Show();
            splashScreen.Close();
        }
    }
}
