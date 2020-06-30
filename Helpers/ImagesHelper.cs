using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ducky.Helpers
{
    class ImagesHelper
    {
        private LogsPageViewModel log;

        public ImagesHelper()
        {
            log = MainWindowViewModel.GetLogVM();
        }


        public BitmapImage byteArrayToImage(byte[] byteArrayIn)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // here
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
            }
            catch { return new BitmapImage(new Uri(@"pack://application:,,,/Resources/Icons/telegram.png", UriKind.Absolute)); }
        }

        public BitmapImage GetImage(string link)
        {
            try
            {
                var imgUrl = new Uri(link);
                var imageData = new WebClient().DownloadData(imgUrl);

                var bitmapImage = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(imageData);
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch (Exception e)
            {
                log.AddLog("ERROR : ", e.Message, "Error in ", e.TargetSite.Name);
                return null;
            }
        }

    }
}
