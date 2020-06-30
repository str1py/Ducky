using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.Model.Messages
{
    class VkMessage : BaseMessage
    {
        public VkMessage(string text, string from) : base(text, from) { }
    }
}
