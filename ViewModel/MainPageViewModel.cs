using Ducky.Helpers.AnwserLogic;
using Ducky.Model;
using Ducky.Model.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ducky.ViewModel
{
    public class MainPageViewModel : ViewModelBase, INotifyCollectionChanged
    {
        #region  Fields
        private ChatStatus cs;
        private LogsPageViewModel log;
        private SearchCommand sc;
        private List<ChatStatus> statuses;
        public static ObservableCollection<BaseMessage> Messages { get; set; }

        private DispatcherTimer timer;
        private Random rnd;
        private int nextchange;
        private int selectedStatus;
        #endregion

        public MainPageViewModel()
        {
            Messages = new ObservableCollection<BaseMessage>();
            statuses = new List<ChatStatus>();
            log = MainWindowViewModel.GetLogVM();
            sc = new SearchCommand();
            cs = new ChatStatus();
            timer = new DispatcherTimer();
            rnd = new Random();
            InitStatuses();

            TimerStart();
            DuckyImage = "/Resources/Images/duck.png";
        }
     

        #region Messages
        private string _userMassageTB;
        public string UserMassageTB
        {
            get { return _userMassageTB; }
            set { _userMassageTB = value; OnPropertyChanged(); }
        }
        private ICommand _sendCommand;
        public ICommand SendCommand
        {
            get { return _sendCommand ?? (_sendCommand = new RelayCommand(p => Send())); }
        }
        private string _duckyImage;
        public string DuckyImage
        {
            get { return _duckyImage; }
            set { _duckyImage = value; OnPropertyChanged(); }
        }

        private void Send()
        {
            if (UserMassageTB?.Any() ?? false)
            {
                log.AddLog("Получена команда от пользвателя - ", UserMassageTB);
                Messages.Add(new UserMessage(UserMassageTB,"user")); //Add msg from user
                //Invoke after message is send 
                GiveAnswer(new DuckMessage(sc.GetAnswer(UserMassageTB),"Ducky"));//Msg from Ducky
                UserMassageTB = "";
            }
        }
        public string SendFromTelegram(string message)
        {
            Messages.Add(new TelegramMessage(message,"telegram",""));
            string answer = sc.GetAnswer(message);
            GiveAnswer(new DuckMessage(answer, "Ducky"));
            return answer;
        }
        public string SendFromVk(string message)
        {
            Messages.Add(new VkMessage(message, "VK"));
            string answer = sc.GetAnswer(message);
            GiveAnswer(new DuckMessage(answer, "Ducky"));
            return answer;
        }

        public async void GiveAnswer(DuckMessage duckmsg)
        {
            await Task.Delay(500);
            Status = "Typing..."; await Task.Delay(500);
            Status = statuses[selectedStatus].status;
            await Task.Delay(500);
            Messages.Add(duckmsg); //Msg from Ducky
        }
        #endregion

        #region Status
        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set { _color = value; OnPropertyChanged(); }
        }

        private void TimerStart()
        {
            SetData();
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, object e)
        {
            SetData();
        }
        private void InitStatuses()
        {

            statuses.Add(cs.SetStatus("Online", "Green")); statuses.Add(cs.SetStatus("Away", "Orange"));
            statuses.Add(cs.SetStatus("Busy", "Yellow"));

            statuses.Add(cs.SetStatus("Playing Counter-Strike: GO", "Green")); statuses.Add(cs.SetStatus("Playing Dota2", "Green"));
            statuses.Add(cs.SetStatus("Playing Fortnite", "Green")); statuses.Add(cs.SetStatus("Playing PUBG", "Green"));

            statuses.Add(cs.SetStatus("Internet serfing", "Green")); statuses.Add(cs.SetStatus("Internet serfing", "Orange"));
            statuses.Add(cs.SetStatus("Watching MEMES", "Orange")); statuses.Add(cs.SetStatus("Watching Twitch", "Orange"));
            statuses.Add(cs.SetStatus("Watching YouTube", "Orange")); statuses.Add(cs.SetStatus("Reading 2ch", "Orange"));
            statuses.Add(cs.SetStatus("Coocking", "Orange")); statuses.Add(cs.SetStatus("Reading Reddit", "Orange"));
            statuses.Add(cs.SetStatus(">__<", "Green"));
        }
        void SetData()
        {
            selectedStatus = rnd.Next(0, statuses.Count);
            Status = statuses[selectedStatus].status; //получить статус из списка
            Color = statuses[selectedStatus].color;
            nextchange = rnd.Next(3, 15);// знаение через сколько сменится статус
            timer.Interval = new TimeSpan(0, nextchange, 0);
            //log.AddLog("Изменение статуса на ", Status, " Следующее изменение через ", nextchange + " минут");

        }
        #endregion

        public event NotifyCollectionChangedEventHandler CollectionChanged = delegate { };
        public void OnCollectionChanged(NotifyCollectionChangedAction action)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action));
        }
    }
}
