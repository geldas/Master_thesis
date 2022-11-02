using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using appDiplo.Models;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using appDiplo.Models;
using System.Collections.ObjectModel;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.ComponentModel;

namespace appDiplo.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        public CitySearchViewModel CitySearchViewModel { get; set; }
        public CategoriesViewModel CategorySearchViewModel { get; set; }
        public StartPointViewModel StartPointSearchViewModel { get; set; }
        public EndPointViewModel EndPointSearchViewModel { get; set; }
        public TripSettingsViewModel TripSettingsViewModel { get; set; }
        public POISViewModel SearchPOIsViewModel { get; set; }
        public ResultsViewModel ResultsViewModel { get; set; }
        public AlgoSettingsViewModel AlgoSettingsViewModel { get; set; }
        public ICommand FindPOIsCommand { get; set; }
        public bool IsBusy { get; set; }
        public bool POIsChanged { get; set; }
        public AsyncCommand RefreshCommand { get; }
        public AsyncCommand<AutoCompleteResults> SelectedCityCommand { get; }
        public AsyncCommand<Results> SelectedStartPointCommand { get; }
        public AsyncCommand<Results> SelectedEndPointCommand { get; }
        //public ICommand StartILS { get; }
        private AutoCompleteResults city;
        public ObservableRangeCollection<POI> ResultsPOIs { get; set; }
        public ObservableRangeCollection<Grouping<string, POI>> POIGroupingDays { get; set; }
        public MainPageViewModel()
        {
            CitySearchViewModel = new();
            StartPointSearchViewModel = new();
            EndPointSearchViewModel = new();
            TripSettingsViewModel = new();
            SearchPOIsViewModel = new();
            AlgoSettingsViewModel = new();

            RefreshCommand = new AsyncCommand(Refresh);
            string localSource = DependencyService.Get<IBaseUrl>().Get();
            string localSource2 = Path.Combine(localSource, "test2.html");
            UrlSource = localSource2;
            Graph = new Graph();
            SelectedCityCommand = new AsyncCommand<AutoCompleteResults>(Selected);
            SelectedStartPointCommand = new AsyncCommand<Results>(SelectedStartPoint);
            SelectedEndPointCommand = new AsyncCommand<Results>(SelectedEndPoint);
            List<Category> catList = CreateCategoriesList();
            CategorySearchViewModel = new(catList);
            city = new AutoCompleteResults();
            FindPOIsCommand = new AsyncCommand(DoFindPOIs);
            //StartILS = new AsyncCommand<object>(async (o) => await DoStartILS(o));
            ResultsPOIs = new ObservableRangeCollection<POI>();
            POIGroupingDays = new ObservableRangeCollection<Grouping<string, POI>>();
            ResultsViewModel = new(CitySearchViewModel, CategorySearchViewModel, StartPointSearchViewModel, EndPointSearchViewModel, TripSettingsViewModel, SearchPOIsViewModel, AlgoSettingsViewModel, this);
            POIsChanged = false;
        }
        private async Task DoFindPOIs()
        {
            List<int> catId = new List<int>();
            List<int> duration = new List<int>();
            SearchPOIsViewModel.POIs.Clear();
            foreach (CategoryViewModel cat in CategorySearchViewModel.Categories)
            {
                if (cat.Duration > 0)
                {
                    catId.Add(cat.Category.id);
                    duration.Add(cat.Duration * 60);
                }
            }
            await ResultsViewModel.RD.GetData(city.geo, catId, duration, StartPointSearchViewModel.Point, EndPointSearchViewModel.Point);
            for (int i=1; i<ResultsViewModel.RD.POIs.Count; i++)
            {
                SearchPOIsViewModel.POIs.Add(new POIViewModel(ResultsViewModel.RD.POIs[i]));
            }
            POIsChanged = true;
        }
        async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            IsBusy = false;
        }
        public string UrlSource
        {
            get { return urlSource; }
            set
            {
                if (value == urlSource)
                    return;
                urlSource = value;
                OnPropertyChanged();
            }
        }

        public AutoCompleteResults City
        {
            get => city;
            set
            {
                if (city == value)
                    return;
                city = value;
                CitySearchViewModel.City = city;
                //StartPointSearchViewModel.City = city;
                //EndPointSearchViewModel.City = city;
            }
        }


        async Task Selected(AutoCompleteResults city)
        {
            if (city == null)
                return;
            City = city;
            StartPointSearchViewModel.City = city;
            StartPointSearchViewModel.CityOkay = true;
            EndPointSearchViewModel.City = city;
            EndPointSearchViewModel.CityOkay = true;
            CitySearchViewModel.IsOkay = true;
            StartPointSearchViewModel.PointSearchResults.Clear();
            EndPointSearchViewModel.PointSearchResults.Clear();
            SearchPOIsViewModel.POIs.Clear();
            ResultsViewModel.RD.POIs.Clear();
            await Application.Current.MainPage.DisplayAlert("Selected City", city.geo.name, "OK");
        }

        async Task SelectedStartPoint(Results city)
        {
            if (city == null)
                return;
            StartPointSearchViewModel.Point = new POI(city.name, city.geocodes.main, city.location);
            if (EndPointSearchViewModel.IsStartPointEndPoint)
            {
                EndPointSearchViewModel.Point = StartPointSearchViewModel.Point;
                EndPointSearchViewModel.Point.Opening = new Dictionary<int, OpeningHours>();
            }
            StartPointSearchViewModel.IsOkay = true;
            EndPointSearchViewModel.CityOkay = true;
            await Application.Current.MainPage.DisplayAlert("Selected Start Point", String.Format("{0}\n{1}", city.name, city.location.formatted_address), "OK");
        }

        async Task SelectedEndPoint(Results city)
        {
            if (city == null)
                return;
            EndPointSearchViewModel.Point = new POI(city.name, city.geocodes.main, city.location);
            EndPointSearchViewModel.Point.Opening = new Dictionary<int, OpeningHours>();
            EndPointSearchViewModel.IsOkay = true;
            await Application.Current.MainPage.DisplayAlert("Selected End Point", String.Format("{0}\n{1}",city.name,city.location.formatted_address), "OK");
        }

        public string urlSource;
        public Graph Graph { get; set; }
        //public ObservableCollection<POI> AllPOIs { get; set; }
        public POISViewModel AllPOIs { get; set; }

        private List<Category> CreateCategoriesList()
        {
            List<Category> catList = new List<Category>();
            catList.Add(new Category() { id = 10027, name = "Museum" });
            catList.Add(new Category() { id = 10044, name = "Planetarium" });
            catList.Add(new Category() { id = 10047, name = "Public Art" });
            catList.Add(new Category() { id = 10056, name = "ZOO" });
            catList.Add(new Category() { id = 10002, name = "Aquarium" });
            catList.Add(new Category() { id = 10004, name = "Art Gallery" });
            catList.Add(new Category() { id = 12099, name = "Spiritual Center" });
            catList.Add(new Category() { id = 16000, name = "Landmarks and Outdoors" });
            return catList;
        }
    }
}