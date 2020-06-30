using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.Model.Messages
{
    public class TelegramMessage : BaseMessage
    {
        public string link { get; set; }

        public TelegramMessage(string text, string from,string link) : base(text, from) { }

    }
}
