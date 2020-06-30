using Ducky.Helpers.Socials.Telegram;
using Ducky.Helpers.Socials.Vk;
using Ducky.ViewModel;
using System;
using System.Net.NetworkInformation;
using System.Windows.Threading;

namespace Ducky.Helpers
{
    public class InternetConnection : ViewModelBase
    {
        // NOT CPU
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public static bool IsConnectionExist { get; set; }

        public InternetConnection()
        {
            InternetConnectionTimerStart();
        }


        [System.Runtime.InteropServices.DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);
        public void CheckNet()
        {
            int desc;
            IsConnectionExist = InternetGetConnectedState(out desc, 0);
        }

        private void InternetConnectionTimerStart()
        {
            dispatcherTimer.Tick += new EventHandler(timerTick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            dispatcherTimer.Start();
        }
        private void timerStop()
        {
            dispatcherTimer.Stop();
            dispatcherTimer.IsEnabled = false;
        }
        private void timerTick(object sender, EventArgs e)
        {
            CheckNet();
        }
    }
}
