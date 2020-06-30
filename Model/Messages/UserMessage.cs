using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.Model.Messages
{
    public class UserMessage : BaseMessage
    {
        public UserMessage(string text, string from) : base(text, from) { }
    }
}
