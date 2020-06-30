using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ducky.Model.Messages
{
    public interface IMessage
    {
        string text { get; set; }
        string fromAndDate { get; set; }
    
    }
}
