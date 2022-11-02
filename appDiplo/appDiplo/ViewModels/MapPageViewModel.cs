using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using Newtonsoft.Json;

namespace appDiplo.ViewModels
{
    [QueryProperty("DisplayData", "displayData")]
    [QueryProperty("Latitude", "latitude")]
    [QueryProperty("Longitude", "longitude")]
    public class MapPageViewModel : ObservableObject
    {
        private string displayData;
        private string urlSource;
        private string latitude;
        private string longitude;
        public ICommand ShowMap { get; }
        
        public MapPageViewModel()
        {
            string localSource = DependencyService.Get<IBaseUrl>().Get();
            string localSource2 = Path.Combine(localSource, "test2.html");
            UrlSource = localSource2;
            ShowMap = new Command<object>(o => DoShowMap(o));
        }

        private void DoShowMap(object webView)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var view = webView as Xamarin.Forms.WebView;
                    string jsonData = JsonConvert.ToString(DisplayData);
                    view.Eval(string.Format("showRoutes({0}, {1}, {2})", jsonData, Latitude, Longitude));
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }


            }

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

        public string DisplayData
        {
            get { return displayData; }
            set { displayData = value; }
        }

        public string Latitude
        {
            get { return latitude; }
            set { latitude = value; }
        }

        public string Longitude
        {
            get { return longitude; }
            set { longitude = value; }
        }
    }
}
