using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var first = getTopicCount("pizza");
            Console.WriteLine(first);

            var res = getRelevantFoodOutlets("Houston", 30);
            foreach (var item in res)
            {
                Console.WriteLine(item);
            }
        }

        private class WikiResponse
        {
            public Parse Parse { get; set; }
        }

        private class Parse
        {
            public string Title { get; set; }
            public long PageId { get; set; }
            public string Text { get; set; }
        }

        static int getTopicCount(string topic)
        {
            string url = "https://en.wikipedia.org/w/api.php?action=parse&section=0&prop=text&format=json&page=" + topic;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                int startIndex = responseBody.IndexOf("{\"*\":");
                responseBody = responseBody.Remove(startIndex, 5);
                startIndex = responseBody.LastIndexOf('}');
                responseBody = responseBody.Remove(startIndex, 1);
                var wikiResponse = JsonConvert.DeserializeObject<WikiResponse>(responseBody);
                if (wikiResponse != null && wikiResponse.Parse != null && !String.IsNullOrWhiteSpace(wikiResponse.Parse.Text))
                {
                    string parseText = wikiResponse.Parse.Text.ToUpper();
                    int topicCount = 0;
                    int x = 0;
                    while ((x = parseText.IndexOf(topic.ToUpper(), x)) != -1)
                    {
                        x += topic.Length;
                        topicCount++;
                    }

                    return topicCount;
                }
            }
            return 0;
        }

        public class FoodOutletResponse
        {
            public int Page { get; set; }
            public int Per_Page { get; set; }
            public int Total { get; set; }
            public int Total_Pages { get; set; }
            public List<Data> Data { get; set; }
        }

        public class Data
        {
            public string City { get; set; }
            public string Name { get; set; }
            public int Estimated_Cost { get; set; }
            public int Id { get; set; }
        }


        public static List<string> getRelevantFoodOutlets(string city, int maxCost)
        {
            int page = 0;
            int totalPages = 0;
            bool shouldContinue = true;

            var resultList = new List<string>();

            HttpClient client = new HttpClient();
            while(shouldContinue)
            {
                string url = $"https://jsonmock.hackerrank.com/api/food_outlets?city={city}&page={page}";
                HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var foodOutletResponse = JsonConvert.DeserializeObject<FoodOutletResponse>(responseBody);
                    if (foodOutletResponse != null && foodOutletResponse.Data != null)
                    {
                        totalPages = foodOutletResponse.Total_Pages;
                        foreach (var data in foodOutletResponse.Data)
                        {
                            if (data.Estimated_Cost <= maxCost)
                            { 
                                if (!resultList.Contains(data.Name))
                                    resultList.Add(data.Name);
                            }
                        }
                    }
                }
                else
                    shouldContinue = false;

                if (page >= totalPages)
                    shouldContinue = false;
                else
                    page++;
            }

            return resultList;
        }
    }
}
