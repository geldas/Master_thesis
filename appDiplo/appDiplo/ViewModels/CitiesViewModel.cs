using System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Xamarin.Forms;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System.Windows.Input;
using appDiplo.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace appDiplo.ViewModels
{
    public class CitySearchViewModel : ObservableObject
    {
        public AutoCompleteResults City { get; set; }
        public bool IsOkay { get; set; }
        public ObservableRangeCollection<AutoCompleteResults> SearchResultsAutoComplete { get; set; }
        public CitySearchViewModel()
        {
            SearchResultsAutoComplete = new();
            IsOkay = false;
        }
        static string CreateFSQAutoSuggestUriGeo(string query)
        {
            return String.Format("/v3/autocomplete?query={0}&types=geo", query);
        }
        static async Task<string> GetDataFsq(string key, string requestUri, string uri = "https://api.foursquare.com")
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            client.BaseAddress = new Uri("https://api.foursquare.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", key);

            HttpResponseMessage response = await client.GetAsync(requestUri);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
        private async Task<AutoComplete> FindCity(string query)
        {
            string key = ""; // FSQ key
            string responseString = await GetDataFsq(key, query);
            AutoComplete? autoComplete = JsonConvert.DeserializeObject<AutoComplete>(responseString);
            return autoComplete;
        }

        public Xamarin.Forms.Command CitySearchNew => new Xamarin.Forms.Command<string>(async (string query) =>
        {
            string requestUriAutoSugest = CreateFSQAutoSuggestUriGeo(query);
            SearchResultsAutoComplete.Clear();
            AutoComplete results = new AutoComplete();
            results = await FindCity(requestUriAutoSugest);
            SearchResultsAutoComplete.AddRange(results.results);
        });
    }


    public class StartPointViewModel : ObservableObject
    {
        private bool isOkay;

        public bool CityOkay { get; set; }
        public ObservableRangeCollection<Results> PointSearchResults { get; set; }
        public AutoCompleteResults City { get; set; }
        public POI Point { get; set; }
        public ICommand GoToPage { get; }
        public StartPointViewModel()
        {
            isOkay = false;
            PointSearchResults = new();
            City = new();
            CityOkay = false;
            GoToPage = new MvvmHelpers.Commands.Command(ChangePage);
            Point = new POI();
        }

        private async void ChangePage()
        {
            await Shell.Current.GoToAsync("mapPageNew");
        }

        public bool IsOkay
        {
            get => isOkay;
            set
            {
                if (isOkay == value)
                    return;
                isOkay = value;
            }
        }

        static async Task<string> GetDataFsq(string key, string requestUri, string uri = "https://api.foursquare.com")
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            client.BaseAddress = new Uri("https://api.foursquare.com");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", key);

            HttpResponseMessage response = await client.GetAsync(requestUri);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
        static String CreateFSQStartEndPointUri(string query, string latitude, string longitude)
        {
            return String.Format("/v3/places/search?query={0}&ll={1}%2C{2}&radius=30000", query.Replace((string)" ", "%20"), latitude, longitude);
        }

        public Xamarin.Forms.Command PointSearch => new Xamarin.Forms.Command<string>(async (string query) =>
        {
            if (CityOkay)
            {
                string requestUriStartPoint = CreateFSQStartEndPointUri(query, City.geo.center.latitude.ToString(), City.geo.center.longitude.ToString());
                PointSearchResults.Clear();
                string key = ""; // FSQ key
                string results = await GetDataFsq(key, requestUriStartPoint);

                FsqData? fsqData = JsonConvert.DeserializeObject<FsqData>(results);
                PointSearchResults.AddRange(fsqData.results);
            }
            else
                await DisplayBadCity();
        });

        private async Task DisplayBadCity()
        {
            await Application.Current.MainPage.DisplayAlert("City not specified!", "Please, in the preceeding steps, specify the city you want to visit.", "OK");
        }
    }

    public class EndPointViewModel : StartPointViewModel
    {
        private bool isStartPointEndPoint;
        private bool isSearchVisible;
        private readonly bool startPointState;
        private bool isOkay;
        public EndPointViewModel()
        {
            PointSearchResults = new();
            City = new();
            isStartPointEndPoint = true;
            isSearchVisible = false;
            startPointState = false;
            CityOkay = false;
        }

        public new bool IsOkay
        {
            get => isOkay;
            set
            {
                if (isOkay == value)
                    return;
                isOkay = value;
            }
        }
        public bool IsSearchVisible
        {
            get => isSearchVisible;
            set
            {
                if (isSearchVisible == value)
                    return;
                isSearchVisible = value;
                OnPropertyChanged();
            }
        }
        public bool IsStartPointEndPoint
        {
            get => isStartPointEndPoint;
            set
            {
                if (isStartPointEndPoint == value)
                    return;
                isStartPointEndPoint = value;
                if (isStartPointEndPoint && !startPointState)
                    IsOkay = false;
                IsSearchVisible = !IsSearchVisible;
                OnPropertyChanged();
            }
        }
    }
}