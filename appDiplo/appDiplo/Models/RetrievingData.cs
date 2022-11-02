using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using System.Globalization;
using MoreLinq;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace appDiplo.Models
{
    // Classes for geting data from Foursquare and Bing Maps.
    /// <summary>
    /// Class for retrieving data.
    /// </summary>
    public class RetrievingData
    {
        public List<POI> POIs;
        public Graph Graph { get; set; }
        public RetrievingData()
        {
            POIs = new List<POI>();
            Graph = new Graph();
        }

        /// <summary>
        /// Method that creates Foursquare URI.
        /// </summary>
        /// <param name="latitude">Latitude of the center.</param>
        /// <param name="longitude">Longitude of the center.</param>
        /// <param name="radius">Radius of searching POIs.</param>
        /// <param name="categories">List of categories.</param>
        /// <param name="limit">Limit of results.</param>
        /// <returns></returns>
        string CreateFSQUri(double latitude, double longitude, int radius, List<int> categories, int limit)
        {
            NumberFormatInfo nfi = new()
            {
                NumberDecimalSeparator = "."
            };
            var categories_string = string.Join("%2C", categories.Select(cat => $"{cat}"));
            return String.Format(NumberFormatInfo.InvariantInfo, "/v3/places/search?ll={0}%2C{1}&radius={2}&categories={3}&fields=fsq_id%2Cname%2Cgeocodes%2Ccategories%2Clocation%2Crelated_places%2Cdistance%2Ctel%2Cemail%2Cwebsite%2Chours%2Crating%2Cstats%2Cprice&sort=RATING&limit={4}", latitude, longitude, radius, categories_string, limit);
        }

        /// <summary>
        /// Method for getting data. There needs to be specified key for Foursquare.
        /// </summary>
        /// <param name="location">Center of search.</param>
        /// <param name="categories">List of ints representing categories.</param>
        /// <param name="duration">List of duration for categories to create list of POIs.</param>
        /// <param name="startPOI">Starting POI.</param>
        /// <param name="endPOI">Ending POI.</param>
        /// <param name="radius">Radius of search.</param>
        /// <returns></returns>
        public async Task GetData(Geo location, List<int> categories, List<int> duration, POI startPOI, POI endPOI, int radius = 5000)
        {
            string key = ""; // FSQ key!!!
            string requestUri = CreateFSQUri(location.center.latitude, location.center.longitude, radius, categories, 50);
            string responseString = await GetDataFsq(key, requestUri);
            FsqData data = JsonConvert.DeserializeObject<FsqData>(responseString);
            CreatePOIList(data, startPOI, endPOI, categories, duration);
        }
        /// <summary>
        /// Method for  reaching for Foursquare.
        /// </summary>
        /// <param name="key">Foursqare key.</param>
        /// <param name="requestUri">Uri of the request.</param>
        /// <param name="uri">Uri.</param>
        /// <returns>Returns response.</returns>
        public async Task<string> GetDataFsq(string key, string requestUri, string uri = "https://api.foursquare.com")
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            string Fsqkey; // fsq key!!!
            client.BaseAddress = new Uri("https://api.foursquare.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", key);

            HttpResponseMessage response = await client.GetAsync(requestUri);
            var responseString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseString);
            Console.WriteLine("DONE!");
            return responseString;
        }
        /// <summary>
        /// Originali created for getting data from TomTom.
        /// </summary>
        /// <param name="requestUri">Request URI.</param>
        /// <param name="body">Body of the request.</param>
        /// <returns></returns>
        public  async Task<string> GetDataTT(string requestUri, string body)
        {
            var client = new HttpClient();
            var response = await client.PostAsync(
                requestUri,
                    new StringContent(body, Encoding.UTF8, "application/json"));
            var contents = await response.Content.ReadAsStringAsync();
            return contents;
        }


        /// <summary>
        /// Creates request matrix for Bing.
        /// </summary>
        /// <param name="origin">Origin node.</param>
        /// <param name="destinations">List of destinations.</param>
        /// <param name="mode">Mode of transportation, set default to driving.</param>
        /// <returns></returns>
        public string CreateRequestBodyBing(POI origin, List<POI> destinations, string mode = "driving")
        {
            BingRequest b_request = new BingRequest();
            b_request.origins = new List<Point>();
            b_request.destinations = new List<Point>();
            string body;
            double latitude, longitude;

            latitude = origin.Geocodes.latitude;
            longitude = origin.Geocodes.longitude;
            b_request.origins.Add(new Point { latitude = latitude, longitude = longitude });
            foreach (POI poi in destinations)
            {
                if (poi != origin)
                {
                    latitude = poi.Geocodes.latitude;
                    longitude = poi.Geocodes.longitude;
                    b_request.destinations.Add(new Point { latitude = latitude, longitude = longitude });
                }
            }
            b_request.travelMode = mode;
            b_request.timeUnit = "second";
            body = JsonConvert.SerializeObject(b_request);
            return body;
        }
        /// <summary>
        /// Method that creates list of POIs.
        /// </summary>
        /// <param name="fsqData">Data from Foursquare.</param>
        /// <param name="startNode">Starting node.</param>
        /// <param name="endNode">Ending node.</param>
        /// <param name="categories">List of categories.</param>
        /// <param name="duration">List of duations for categories.</param>
        private void CreatePOIList(FsqData fsqData, POI startNode, POI endNode, List<int> categories, List<int> duration)
        {
            int poiDuration = 60*60;
            int landmarkID = categories.IndexOf(16000);
            POI temp_poi;
            if (POIs.Count > 0)
                POIs.Clear();
            if (fsqData != null)
            {
                foreach (Results poi in fsqData.results)
                {
                    if (poi.categories[0].id % 16000 < 1000)
                        poiDuration = duration[landmarkID];
                    else
                    {
                        for (int i = 0; i < categories.Count; i++)
                        {
                            if (categories[i] == poi.categories[0].id)
                            {
                                poiDuration = duration[i];
                            }
                        }
                    }
                    temp_poi = new(poi.name, poi.geocodes.main, poi.location, poi.rating, poi.stats, poi.hours.regular, poi.tel, poi.email, poi.website, poi.categories, poiDuration);
                    POIs.Add(temp_poi);
                }
            }
        }
        /// <summary>
        /// Method that creates graph.
        /// </summary>
        /// <param name="originNode">Starting node.</param>
        /// <param name="endNode">Ending Node.</param>
        /// <returns></returns>
        public async Task CreateGraph(POI originNode, POI endNode)
        {
            POI origin;
            string requestBodyBing, responseBing;
            string bingKey = ""; //Bing key !!!
            string uri = string.Format("https://dev.virtualearth.net/REST/v1/Routes/DistanceMatrix?key={0}", bingKey);
            string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string localPath;
            if (Graph.Nodes.Count > 0)
                Graph.Nodes.Clear();
            Graph.AddNode(originNode);
            foreach (POI poi in POIs)
            {
                Graph.AddNode(poi);
            }
            Graph.AddNode(endNode);
            for (int i = 0; i < Graph.Nodes.Count; i++)
            {
                origin = Graph.Nodes[i];
                requestBodyBing = CreateRequestBodyBing(origin, Graph.Nodes, "walking");
                responseBing = await GetDataTT(uri, requestBodyBing);
                localPath = Path.Combine(documentPath, String.Format("b_response_test_walking_all_cats_new_new_{0}.json", i));
                File.WriteAllText(localPath, responseBing);
                BingMatrixResponse? b_data = JsonConvert.DeserializeObject<BingMatrixResponse>(responseBing);
                b_data.resourceSets[0].resources[0].results.Sort((a, b) => a.destinationIndex.CompareTo(b.destinationIndex));
                Graph.AddDirectedEdge(origin, Graph.Nodes, b_data.resourceSets[0].resources[0].results);
            }
        }
    }
}
