using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ducky.Model.Messages
{
    public abstract class BaseMessage : IMessage
    {
        public string text { get; set; }
        public string fromAndDate { get; set; }

        public BaseMessage(string txt,string from)
        {
            text = txt;
            fromAndDate = $"from {from} at {DateTime.Now.ToLongTimeString()}";
        }
    }
}
