using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.Model
{
    class ChatStatus
    {
        public string status { get; set; }
        public string color { get; set; }
        public ChatStatus SetStatus(string status, string color)
        {
            var stat = new ChatStatus {
                status = status,
                color = color};
            return stat;
        }
    }
}
