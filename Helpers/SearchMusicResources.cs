using Ducky.ViewModel;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Threading.Tasks;

namespace Ducky.Helpers
{
    class SearchMusicResources
    {
        public async Task<string> GetYouTubeLink(string artist , string title)
        {
            if (InternetConnection.IsConnectionExist && Properties.Settings.Default.YoutubeSearch)
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = Properties.Settings.Default.YouTubeApiKey,
                    ApplicationName = this.GetType().ToString()
                });

                var searchListRequest = youtubeService.Search.List("snippet");
                searchListRequest.Q = $"{artist} - {title}"; 
                searchListRequest.MaxResults = 1;

                var searchListResponse = await searchListRequest.ExecuteAsync();

                string video = null;

                foreach (var searchResult in searchListResponse.Items)
                {
                    switch (searchResult.Id.Kind)
                    {
                        case "youtube#video":
                            video = "https://www.youtube.com/watch?v=" + searchResult.Id.VideoId;
                            break;
                    }
                }
                return video;
            }
            else
                return null;
        }
        private string GetBeatportLink()
        {
            //https://www.beatport.com/search?q=JOYRYDE+madden
            //https://www.beatport.com/track/madden-original-mix/12447656
            return null;
        }
        private string GetYaMusicLink()
        {
            //https://music.yandex.ru/search?text=alison%20wonderland%20run
            return null;
        }

    }
}
