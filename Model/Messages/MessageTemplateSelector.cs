using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ducky.Model.Messages
{
    class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DuckMessage { get; set; }
        public DataTemplate UserMessage { get; set; }
        public DataTemplate TelegramMessage { get; set; }
        public DataTemplate VkMessage { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item is DuckMessage)
                return DuckMessage;
            if (item is UserMessage)
                return UserMessage;
            if (item is TelegramMessage)
                return TelegramMessage;
            if (item is VkMessage)
                return VkMessage;

            return null;
        }
    }
}
