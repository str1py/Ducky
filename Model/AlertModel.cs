using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Ducky.Model
{
    public class AlertModel
    {
        public BitmapImage AlertImage { get; set; }
        public string AlertFrom { get; set; }
        public string AlertMessage { get; set; }
        public string Time { get; set; }
    }
}
