using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Ducky.Helpers
{
    public class SongInfo
    {
        private int delay = 3;

        public string[] GetSongInfo(string songname)
        {
            string[] words = songname.Split(new char[] { '-' });// [0] - artist, [1] - title
            for (int i = 0; i < words.Count(); i++)
            {
                if (words[i].Contains(".mp3"))
                    words[i] = words[i].Remove(words[i].Length - 4);

                words[i] = words[i].Trim();
            }
            return words;
        }
        public string GetSongTime(string songtime)
        {
            string[] time = songtime.Split(new char[] { ':' });
            if (time[0] == "00")
                return time[1] + ':' + time[2].Substring(0, 2);
            else if (time[1] == "00")
                return time[2];
            else
                return time[0] + ':' + time[1] + ':' + time[2].Substring(0, 2);
        }
        public int GetSeconds(string curtime)
        {
            string[] time = curtime.Split(new char[] { ':' });
            int minutes = Convert.ToInt32(time[0]);
            int seconds = Convert.ToInt32(time[1]);

            return minutes * 60 + seconds + delay;
        }

        public string ArtistEditor(string str)
        {
            if (str?.Length > 25)
            {
                string s = str.Substring(0, 25);
                s += '.';
                s += '.';
                s += '.';
                return s;
            }
            return str;
        }
        public string TitleEditor(string str)
        {
            if (str?.Length > 33)
            {
                string s = str.Substring(0, 33);
                s += '.';
                s += '.';
                s += '.';
                return s;
            }
            return str;
        }

        public BitmapImage GetSongImage(string filepath)
        {
            var audioFile = TagLib.File.Create(filepath);

            if (audioFile.Tag.Pictures.Length != 0)
                return IpicToBitmap(audioFile.Tag.Pictures[0]);
            else
                return new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/songicon.png", UriKind.Absolute));
        }
        public BitmapImage IpicToBitmap(TagLib.IPicture pic)
        {
            using (MemoryStream ms = new MemoryStream(pic.Data.Data))
            {
                BitmapImage bitmap = new BitmapImage();
                ms.Seek(0, SeekOrigin.Begin);
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                return bitmap;
            }
        }
    }
}
