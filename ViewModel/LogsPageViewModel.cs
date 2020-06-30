using Ducky.Model;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Ducky.ViewModel
{
    public class LogsPageViewModel : ViewModelBase, INotifyCollectionChanged
    {
        public ObservableCollection<LogsModel> LogList { get; } 
        public LogsPageViewModel()
        {
            LogList = new ObservableCollection<LogsModel>();
            AddLog("Начало логирвания. Сообщение из ", (typeof(LogsPageViewModel).Name));
        }
      
        public void AddLog(string fmes,string fdata)
        {
            LogList.Add(new LogsModel{
               date = DateTime.Now.ToLongTimeString(),
               fmessage = $" : {fmes}",
               fdata = fdata,
            });;
            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        }
        public void AddLog(string fmes, string fdata, string smes, string sdata)
        {
            LogList.Add(new LogsModel
            {
                date = DateTime.Now.ToLongTimeString(),
                fmessage = $" : {fmes}",
                fdata = fdata,
                smessage = $" {smes}",
                sdata = sdata
            }); ;
            OnCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
        public void OnCollectionChanged(NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }

    }
}
