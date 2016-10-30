using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace GTFAQ_DotNet_Bot.Data
{
    public class NewsArticleAccess
    {
        public static async Task<List<NewsArticle>> GetNewsArticlesAsync()
        {
            var result = "";

            //Create new list
            List<NewsArticle> articleList = new List<NewsArticle>();

            try
            {
                //Send request and get the JSON
                HttpClient httpClient = new HttpClient();
                var httpResponse = await httpClient.GetAsync("http://govtechdemo.azurewebsites.net/service/newsretrieve");
                httpResponse.EnsureSuccessStatusCode();
                result = await httpResponse.Content.ReadAsStringAsync();
                Debug.WriteLine(result);

                //Deserialize the JSON array into a list
                articleList = JsonConvert.DeserializeObject<List<NewsArticle>>(result);
            }
            catch (Exception ex)
            {
                result = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                Debug.WriteLine(result);
            }

            return articleList;
        }
    }
}