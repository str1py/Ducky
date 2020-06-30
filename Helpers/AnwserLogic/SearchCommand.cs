using System;
using System.Collections.Generic;
using Ducky.ViewModel;

namespace Ducky.Helpers.AnwserLogic
{
    public class SearchCommand
    {
        private CommandList cl = new CommandList();
        public static int LevenshteinDistance(string string1, string string2)
        {
            if (string1 == null) throw new ArgumentNullException("string1");
            if (string2 == null) throw new ArgumentNullException("string2");
            int diff;
            int[,] m = new int[string1.Length + 1, string2.Length + 1];

            for (int i = 0; i <= string1.Length; i++) { m[i, 0] = i; }
            for (int j = 0; j <= string2.Length; j++) { m[0, j] = j; }

            for (int i = 1; i <= string1.Length; i++)
            {
                for (int j = 1; j <= string2.Length; j++)
                {
                    diff = (string1[i - 1] == string2[j - 1]) ? 0 : 1;

                    m[i, j] = Math.Min(Math.Min(m[i - 1, j] + 1,
                                             m[i, j - 1] + 1),
                                             m[i - 1, j - 1] + diff);
                }
            }
            return m[string1.Length, string2.Length];
        }
        public string GetAnswer(string uc)
        {
            string usercommmandAsTyped = uc; //Команда написанная пользователем
            string usercommand = uc.Trim(); //Удаляем пробелы вначале и в конце
            bool CommandHasFound = false;
            string commandToAction = null;
            string[] words = null;

            #region GET WORDS AND WORDS COUNT
            if (usercommand.Length > 3) //Если длинна больше 3 символов
            {
                int wordscount = 1;
                char[] command = usercommand.ToCharArray(); //Переводи команду в массив символов

                for (int i = 0; i < usercommand.Length; i++)
                {
                    if (command[i] == ' ') //если есть проббел
                        wordscount++;
                }//количество слов
                words = usercommand.Split(' ');//слова которые содержаться в запросе
                #endregion
                switch (wordscount)
                {
                    case 1:
                        commandToAction = words[0];
                        break;
                    case 2:
                        commandToAction = words[0] + " " + words[1];
                        break;
                    case 3:
                        commandToAction = words[0] + " " + words[1] + " " + words[2];
                        break;
                    case 4:
                        commandToAction = words[0] + " " + words[1] + " " + words[2] + " " + words[3];
                        break;
                }

                foreach (KeyValuePair<string, Delegate> keyValue in cl.DicCommands)// Ищем целую команду с погрешностью < 3
                {
                    if (LevenshteinDistance(commandToAction, keyValue.Key.ToString()) < 3) //если погрешеость меньше 3 то выполняем коммаду
                    {
                        if (cl.DicCommands.ContainsKey(keyValue.Key.ToString().ToLower()))
                        {
                            CommandHasFound = true;
                            return cl.DicCommands[keyValue.Key.ToString().ToLower()].DynamicInvoke().ToString();
                        }
                    }
                }
                if (CommandHasFound == false)
                {
                   // return dialogflow.GetAnswer(usercommand);
                  return "Не понимаю о чем вы...(((";
                }
            }
            return "Э блэ тухдыгиндэ... Не понял? Вот и я тоже";

        }
    }
}

