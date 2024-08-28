using Newtonsoft.Json;
using System.Web;
using System.Net;
using HonkBoard_Backend.Core.Structures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;

namespace HonkBoard_Backend.Core.Controller
{
    public class WrapperOptions {

        public Dictionary<string, WordsListInfo> wordsLists { get; set; }
        public string imagesServerAddress { get; set; }
    }

    public class DatabaseWrapper : IDataAccess
    {
        //private readonly string DatabaseLink;
        private readonly string _usersDataLink;

        private readonly string _wordsDataLink;

        private readonly string _databaseLink;

        private readonly HttpClient _httpClient;

        private IOptions<WrapperOptions> _options;
        public DatabaseWrapper(HttpClient client, IOptions<WrapperOptions> options)
        {
            _httpClient = client;

            _options = options;

            var PROJECT_ID = "honk-board-default-rtdb";
            var PROJECT_REGION = "europe-west1";

            var USERSDATA = "/Users";
            var WORDSDATA = "/WordsSets";

            var DatabaseLink = $"https://{PROJECT_ID}.{PROJECT_REGION}.firebasedatabase.app";

            _databaseLink = DatabaseLink;

            _usersDataLink = $"{DatabaseLink}{USERSDATA}";
            _wordsDataLink = $"{DatabaseLink}{WORDSDATA}";


            UpdateWordsCount();
        }

        public async Task UpdateWordsCount(string? category = null) {

            if (category != null)

            {

                if (_options.Value.wordsLists.ContainsKey(category))
                {
                    var count = await GetWordsCount(_options.Value.wordsLists[category].GetListLink());

                    _options.Value.wordsLists[category].Count = count;
                }

            }

            else
            {

                foreach (var wordsKey in _options.Value.wordsLists.Keys)
                {

                    var count = await GetWordsCount(_options.Value.wordsLists[wordsKey].GetListLink());

                    _options.Value.wordsLists[wordsKey].Count = count;

                }

            }

        }

        public int GetMaxWords(string category) 
        {
            var count = 0;

            if (_options.Value.wordsLists.ContainsKey(category)) {

                count = _options.Value.wordsLists[category].GetCount();

            }

            return count;
        }


        public async Task<string?> GetWord(string category, int wordId) {

            string? word = "";

            if (_options.Value.wordsLists.ContainsKey(category))
            {
                var categoryLink = _options.Value.wordsLists[category].GetListLink();

                var builder = new UriBuilder($"{_wordsDataLink}{categoryLink}/{wordId}.json");

                //var query = HttpUtility.ParseQueryString(builder.Query);

                //query["orderBy"] = "\"Id\"";
                //query["equalTo"] = $"\"{wordId}\"";

                //builder.Query = query.ToString();

                var resp = await _httpClient.GetAsync(builder.ToString());

                var content = await resp.Content.ReadAsStringAsync();

                word = JsonConvert.DeserializeObject<string>(content);

            }

            return word;

        }


        private async Task<int> GetWordsCount(string categoryLink) {

            var builder = new UriBuilder($"{_wordsDataLink}{categoryLink}.json");

            var resp = await _httpClient.GetAsync(builder.ToString());

            var content = await resp.Content.ReadAsStringAsync();

            var words = JsonConvert.DeserializeObject<List<string>>(content);

            var count = 0;

            if (words != null)
            {
                count = words.Count;
            }

            return count;
        }


        public async Task<User> WriteInfo(User user)
        {
            var databaseUser = new User();

            var json = new StringContent(JsonConvert.SerializeObject(user, Formatting.Indented));

            if (!user.IsEmpty())
            {
                var getUser = await GetUser(user.Id);

                if (getUser.IsEmpty())
                {
                    await _httpClient.PostAsync($"{_usersDataLink}.json", json);
                    
                    databaseUser = user;
                    
                }
            }

            return databaseUser;
        }

        public async Task<bool> IsRegistered(string googleId)
        {
            try
            {
                var user = await GetUser(googleId);

                return !user.IsEmpty();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<HttpStatusCode> PatchUser(User user)
        {
            var content = await GetJsonUser(user.Id);

            if (content.Trim() == "{}" || content.Trim() == "[]")
            {

                return HttpStatusCode.BadRequest;

            }

            var result = JsonConvert.DeserializeObject<Dictionary<string, User>>(content);

            result[result.Keys.ToArray()[0]] = user;

            var updatedUser = result[result.Keys.ToArray()[0]];

            var builder = new UriBuilder($"{_usersDataLink}/{result.Keys.ToArray()[0]}.json");

            HttpContent userContent = new StringContent(JsonConvert.SerializeObject(updatedUser));

            var resp = await _httpClient.PatchAsync(builder.ToString(), userContent);
            
            return HttpStatusCode.OK;
            
        }
        
        public async Task<bool> PatchUserAvatar(string googleId, string avatarUrl)
        {
            try
            {
                var content = await GetJsonUser(googleId);

                if (content.Trim() == "{}" || content.Trim() == "[]")
                {

                    return false;

                }

                var result = JsonConvert.DeserializeObject<Dictionary<string, User>>(content);

                var userDatabaseId = result.Keys.ToArray()[0];

                var builder = new UriBuilder($"{_usersDataLink}/{userDatabaseId}/Avatar.json");

                HttpContent avatarUrlContent = new StringContent(avatarUrl);

                var resp = await _httpClient.PatchAsync(builder.ToString(), avatarUrlContent);

                if (resp.StatusCode == HttpStatusCode.OK) 
                { 
                    
                    return true;
                
                }

                return false;

            }
            catch
            {

                return false;

            }

        }

        public async Task<User> GetUser(string googleId)
        {
            var content = await GetJsonUser(googleId);
            var user = new User();

            if (content.Trim() == "{}" || content.Trim() == "[]")
            {

                return user;

            }

            var result = JsonConvert.DeserializeObject<Dictionary<string, User>>(content);

            user = result[result.Keys.ToArray()[0]];

            //var result = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(content);

            //var user = JsonConvert.DeserializeObject<User>("{" + string.Join(",", result[result.Keys.First()].Select(kv => $"{kv.Key}:\"{kv.Value}\"").ToArray()) + "}");

            return user;
        }

        public async Task<string> PostImage(string googleId, IFormFile image) 
        {
            var GoogleId = googleId;
            var Image = image;

            var builder = new UriBuilder($"{_options.Value.imagesServerAddress}/api/upload-image");

            var query = HttpUtility.ParseQueryString(builder.Query);
            query["id"] = $"{GoogleId}";

            builder.Query = query.ToString();

            var memoryStream = new MemoryStream();
            await Image.CopyToAsync(memoryStream);

            var content = new ByteArrayContent(memoryStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            var resp = await _httpClient.PostAsync(builder.ToString(), content);
            
            var respContent = await resp.Content.ReadAsStringAsync();

            await PatchUserAvatar(GoogleId, respContent);

            return respContent;

        }

        public async Task UpdateUserStatistics(string GoogleId, string GameName, string StatName, StatisticsField Data) 
        {
            
            var googleId = GoogleId;
            var gameName = GameName;
            var statName = StatName;
            var statData = Data;

            statData.StatText = Data.StatText;
            statData.StatValue = Data.StatValue;
            statData.StatMesure = Data.StatMesure;
            statData.StatType = Data.StatType;

            var content = await GetJsonUser(googleId);

            if (content.Trim() == "{}" || content.Trim() == "[]")
            {

                return;

            }

            var result = JsonConvert.DeserializeObject<Dictionary<string, User>>(content);

            var userDatabaseId = result.Keys.ToArray()[0];

            var builder = new UriBuilder($"{_usersDataLink}/{userDatabaseId}/Statistics/{gameName}/{statName}.json");

            var resp = await _httpClient.GetAsync(builder.ToString());

            if (resp.StatusCode == HttpStatusCode.OK) 
            {

                content = await resp.Content.ReadAsStringAsync();

                var dbStatData = JsonConvert.DeserializeObject<StatisticsField>(content);

                if (dbStatData != null)
                {

                    if (dbStatData.StatType == typeof (int)) 
                    {

                        statData.StatValue += dbStatData.StatValue;

                    }

                    var patchContent = new StringContent(JsonConvert.SerializeObject(statData));

                    await _httpClient.PatchAsync(builder.ToString(), patchContent);

                }
                else
                {

                    var postContent = new StringContent(JsonConvert.SerializeObject(statData));

                    await _httpClient.PostAsync(builder.ToString(), postContent);

                }
            }       
        }

        public async Task UpdateGlobalStatistics(string GameName, string StatName, StatisticsField Data)
        {
            var statData = new StatisticsField();
            var statName = StatName;
            var gameName = GameName;

            statData.StatText = Data.StatText;
            statData.StatValue = Data.StatValue;
            statData.StatMesure = Data.StatMesure;
            statData.StatType = Data.StatType;

           

            var GlobalStasLink = $"{_databaseLink}/GlobalStatistics/{gameName}/{statName}.json";

            var builder = new UriBuilder(GlobalStasLink);

            var resp = await _httpClient.GetAsync(builder.ToString());

            if (resp.StatusCode == HttpStatusCode.OK)
            {

                var content = await resp.Content.ReadAsStringAsync();

                var dbStatData = JsonConvert.DeserializeObject<StatisticsField>(content);

                if (dbStatData != null)
                {
                        
                    if (dbStatData.StatType == typeof(int))
                    {

                        statData.StatValue += dbStatData.StatValue;

                    }

                    var patchContent = new StringContent(JsonConvert.SerializeObject(statData));

                    await _httpClient.PatchAsync(builder.ToString(), patchContent);

                }
                else
                {

                    var postContent = new StringContent(JsonConvert.SerializeObject(statData));

                    await _httpClient.PostAsync(builder.ToString(), postContent);

                }
            }
        }

        private async Task<string> GetJsonUser(string googleId)
        {
            //req.Headers.Add("Referer", "login.microsoftonline.com");
            //req.Headers.Add("Accept", "application/x-www-form-urlencoded");
            //req.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var builder = new UriBuilder($"{_usersDataLink}.json");

            var query = HttpUtility.ParseQueryString(builder.Query);
            query["orderBy"] = "\"Id\"";
            query["equalTo"] = $"\"{googleId}\"";

            builder.Query = query.ToString();

            var resp = await _httpClient.GetAsync(builder.ToString());

            var content = await resp.Content.ReadAsStringAsync();

            return content;
        }        
    }
}
