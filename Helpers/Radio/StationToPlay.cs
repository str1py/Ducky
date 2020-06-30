using Ducky.Model.Radio;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Ducky.Helpers.Radio
{
    class StationToPlay
    {
        private string path { get; set; } = Directory.GetCurrentDirectory();

        public async Task<PlayingStation> GetStationInfo(int catindex, int stindex)
        {
            if (catindex == 0)
            {
                XDocument xDoc = XDocument.Load(path + @"/Data/RecordStations.xml");
                string json = await GetJsonInfo("https://www.radiorecord.ru/xml/" +
                        xDoc.Root.Element("RecordStation" + (stindex + 1)).Element("ImageWord").Value +
                        "_online_v8.txt");

                //NULLERROR
                var o = JObject.Parse(json);

                bool image = json.Contains("image600");
                ImageSource trackpic;
             
                if (image)
                {
                    string pic = (string)o["image600"];
                    pic = pic.Replace('\\', ' ');
                    trackpic = await LoadImage(pic);
                } else trackpic = new BitmapImage(new Uri("pack://application:,,,/Resources/Radio/Default/record.jpeg"));

                var station = (new PlayingStation
                {
                    RadioName = xDoc.Root.Element("RecordStation" + (stindex + 1)).Element("title").Value,
                    RadioLogo = trackpic,
                    RadioArtist = (string)o["artist"],
                    RadioSongName = (string)o["title"],
                    URL = xDoc.Root.Element("RecordStation" + (stindex + 1)).Element("linkHQ").Value,
                    DeezerLink = (string)o["itunesURL"],
                    SpotifyLink = CreateSpotifyLink((string)o["artist"], (string)o["title"]),
                    ItunesLink = CreateItunesLink((string)o["artist"], (string)o["title"]),
                    YamusicLink = CreateYamusicLink((string)o["artist"], (string)o["title"]),
                });

                return station;
            }
            else if (catindex == 1)
            {
                XDocument xDoc = XDocument.Load(path + @"/Data/MoscowRadioStations.xml");
                switch (xDoc.Root.Element("MRS" + (stindex + 1)).Element("title").Value)
                {
                    case "Energy FM":
                        var energy = await GetRadioEnergyInfo();
                        energy.URL = xDoc.Root.Element("MRS" + (stindex + 1)).Element("link").Value;
                        return energy;
                    case "Europa Plus":
                        var euroopa = await GetRadioEuropaPlusInfo();
                        euroopa.URL = xDoc.Root.Element("MRS" + (stindex + 1)).Element("link").Value;
                        return euroopa;
                    case "Love Radio":
                        var loveradio = await GetRadioEuropaPlusInfo();
                        loveradio.URL = xDoc.Root.Element("MRS" + (stindex + 1)).Element("link").Value;
                        return loveradio;
                }
                return null;
            }
            else if (catindex == 2)
            {
                XDocument xDoc = XDocument.Load(path + @"/Data/BBCRadioStations.xml");             
                var bbc = await GetBBCRadioInfo(xDoc.Root.Element("BBCRadio" + (stindex + 1)).Element("json").Value);
                bbc.URL = xDoc.Root.Element("BBCRadio" + (stindex + 1)).Element("link").Value;
                bbc.RadioName = xDoc.Root.Element("BBCRadio" + (stindex + 1)).Element("title").Value;
                return bbc;
            }
            else if (catindex == 3)
                return null;
            else
                return null;
        }

        /// Get Full Station Info
        private async Task<string> GetJsonInfo(string link)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var txt = await client.GetStringAsync(new Uri(link));
                    return txt;
                }
            }
            catch
            {
                return null;
            }
        }
        public async Task<ImageSource> LoadImage(string imagelink)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    BitmapSource bitmap = null;
                    var httpClient = new HttpClient();
                    using (var response = await httpClient.GetAsync(imagelink))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            using (var stream = new MemoryStream())
                            {
                                await response.Content.CopyToAsync(stream);
                                stream.Seek(0, SeekOrigin.Begin);

                                bitmap = BitmapFrame.Create(
                                    stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            }
                        }
                    }

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        BitmapEncoder enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(bitmap));
                        enc.Save(outStream);
                        Bitmap bitmap1 = new Bitmap(outStream);
                    }
                    return bitmap;
                }
            }
            catch
            {
                return null;
            }
        }

        private async Task<PlayingStation> GetRadioEnergyInfo()
        {
            var json = await GetJsonInfo("http://www.energyfm.ru/api/radiostation/getTitleBroadcast/?dataFormat=json");
            var o = JObject.Parse(json);
            var result = (JObject)o["result"]["short"];
            var cover = (JObject)o["result"]["short"]["cover"];
            string itunes = (string)o["result"]["short"]["Itunes"];
            if (itunes == "False")
                itunes = "";

            var station = (new PlayingStation
            {
                RadioName = "Energy FM",
                RadioArtist = (string)result["titleExecutor"],
                RadioSongName = (string)result["titleTrack"],
                RadioLogo = await LoadImage((string)cover["coverHTTP"]),
                DeezerLink = CreateDeezerLink((string)result["titleExecutor"], (string)result["titleTrack"]),
                SpotifyLink = CreateSpotifyLink((string)result["titleExecutor"], (string)result["titleTrack"]),
                ItunesLink = itunes,
                YamusicLink = CreateYamusicLink((string)result["titleExecutor"], (string)result["titleTrack"])
            });

            return station;
        }
        private async Task<PlayingStation> GetRadioEuropaPlusInfo()
        {
            var json = await GetJsonInfo("http://www.europaplus.ru/online/air/1.air.js?_=1572980779776");
            var o = JObject.Parse(json);

            string str = (string)o["photo"];
            ImageSource image;
            if (str == "")
                image = new BitmapImage(new Uri("pack://application:,,,/Resources/Radio/Default/europa.png"));
            else
            {
                str = str.Remove(0, 6);
                image = await LoadImage(str);
            }

            var station = (new PlayingStation
            {
                RadioName = "Europa Plus",
                RadioArtist = (string)o["artist"],
                RadioSongName = (string)o["song"],
                RadioLogo = image ?? new BitmapImage(new Uri("pack://application:,,,/Resources/Radio/Default/europa.png")),
                DeezerLink = CreateDeezerLink((string)o["artist"], (string)o["song"]),
                SpotifyLink = CreateSpotifyLink((string)o["artist"], (string)o["song"]),
                ItunesLink =CreateItunesLink((string)o["artist"], (string)o["song"]),
                YamusicLink = CreateYamusicLink((string)o["artist"], (string)o["song"])      
        });

            return station;
        }
        private async Task<PlayingStation> GetBBCRadioInfo(string link)
        {
            var json = await GetJsonInfo(link);
            var o = JObject.Parse(json);
            var result = (JObject)o["data"][0]["titles"];
            var result1 = (JObject)o["data"][0];

            string str = (string)result1["image_url"];
            str = str.Replace("{recipe}", "320x320");

            var station = (new PlayingStation
            {
                RadioArtist = (string)result["secondary"],
                RadioSongName = (string)result["primary"],
                RadioLogo = await LoadImage(str),
                DeezerLink = CreateDeezerLink((string)result["secondary"], (string)result["primary"]),
                SpotifyLink = CreateSpotifyLink((string)result["secondary"], (string)result["primary"]),
                ItunesLink = CreateItunesLink((string)result["secondary"], (string)result["primary"]),
                YamusicLink = CreateYamusicLink((string)result["secondary"], (string)result["primary"])
            });
            return station;
        }

        private string CreateSpotifyLink(string artist,string title)
        {
            //spotify:search:Flechewave:We See It
            string link = $"spotify:search:{artist}:{title}";
            return link;
        }
        private string CreateItunesLink(string artist, string title)
        {
            return null;
        }
        private string CreateYamusicLink(string artist, string title)
        {
            string link = $"https://music.yandex.ru/search?text={artist} {title}";
            return link;
        }
        private string CreateDeezerLink(string artist, string title)
        {
            string link = $"https://www.deezer.com/search/{artist} {title}";
            return link;
        }

    }
}
