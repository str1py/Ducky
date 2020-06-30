using Ducky.Helpers.Socials.Telegram;
using Ducky.Model.Messages;
using Ducky.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using Telegram.Bot.Args;

namespace Ducky.Helpers.AnwserLogic
{
    public class CommandList
    {
        private readonly PlayerPageViewModel player;
        public Dictionary<string, Delegate> DicCommands = new Dictionary<string, Delegate>();
        readonly static string path = Directory.GetCurrentDirectory();
        private SystemCommands sc = new SystemCommands();

        public delegate string Delegate1();

        private XDocument xDoc;

        public CommandList()
        {
            player = MainWindowViewModel.GetPlayerVM();  
            //init commands 
            xDoc = XDocument.Load(path + "/Data/Commands.xml");
            AddAnswersFromXAML();
        }

        public string Hello()
        {
            string str = xDoc.Root.Element("Hello").Element("Answers").Value;
            string[] randHi = str.Split(',');
            Random rnd = new Random();
            return randHi[rnd.Next(randHi.Count())];
        }
        public string HowAreU()
        {
            string str = xDoc.Root.Element("HowAreU").Element("Answers").Value;
            string[] randHi = str.Split(',');
            Random rnd = new Random();
            return randHi[rnd.Next(randHi.Count())]; ;
        }
        public string PlayMusic()
        {
            if (Properties.Settings.Default.MusicFolders == null)
                return "Вы не выбрали папки с музыкой(. Вы можете выбрать ее в Настрйка. Кря";
            else
            {
                player.PlayByCommand();
                return "Включаю музычку";
            }

        }
        public string PlayNextSong()
        {
            player.Next();
            return "Ставлю следующий трек, кря";
        }
        public string PlayPrevSong()
        {
            player.Previous();
            return "Ставлю предыдущий";
        }
        public string SongPause()
        {
            player.Pause();
            return "Ладно, ставлю на паузу...";
        }
        public string PlayerExit()
        {
            return "Метод не реализован";
        }
        public string WhatSongNow()
        {
            return "Метод не реализован";
        }

        #region System Commands
        public string LockPC()
        {
            sc.Lock();
            return "Компьютер заблокирован";
        }
        public string RebootPC()
        {
            sc.halt(true, false); //мягкая перезагрузка
            return "Перезагружаю компьютер";
        }
        public string Shutdown()
        {
            sc.halt(false, false); //мягкое выключение
            return "Выключаю компьютер";
        }
        #endregion

        #region TelegramButton
        public string ShowPlayerButtons()
        {
            if (MainPageViewModel.Messages.Last() is TelegramMessage)
            {
                TelegramBot tb = MainWindowViewModel.GetTelebot();
                tb.ShowPlayerButtons(this, null);
                return "Вот кнопачки";
            }
            else
                return "Сообщение отправленно не из телеграмма. Кнопачки только в телеграмме, сорян";
        }
        #endregion
        private void AddAnswersFromXAML()
        {
            try
            {
                List<XElement> list = xDoc.Root.Elements().ToList();// Получает все команды
                foreach(var name in list)
                {
                    string str = xDoc.Root.Element(name.Name.LocalName).Element("Command").Value;// Достаем все варианты комманд
                    string[] commands = str.Split(',');
                    foreach (string word in commands)
                    {
                        var method = Delegate.CreateDelegate(typeof(Delegate1), this, name.Name.LocalName); //Создаем делегат
                        DicCommands.Add(word, method);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
