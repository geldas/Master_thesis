using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using appDiplo.Models;

namespace appDiplo.Views.CarouselViews.Templates
{
    public class CarouselDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CitySearch { get; set; }
        public DataTemplate Categories { get; set; }

        public DataTemplate StartPoint { get; set; }
        public DataTemplate EndPoint { get; set; }
        public DataTemplate TripSettings { get; set; }
        public DataTemplate SearchPOIs { get; set; }
        public DataTemplate Results { get; set; }
        public DataTemplate AlgoSettings { get; set; }

        public CarouselDataTemplateSelector()
        {
            CitySearch = new DataTemplate(typeof(CitySearchView));
            Categories = new DataTemplate(typeof(CategoriesView));
            StartPoint = new DataTemplate(typeof(StartPointView));
            EndPoint = new DataTemplate(typeof(EndPointView));
            TripSettings = new DataTemplate(typeof(TripSettingsView));
            SearchPOIs = new DataTemplate(typeof(SearchPOIsView));
            AlgoSettings = new DataTemplate(typeof(AlgoSettingsView));
            Results = new DataTemplate(typeof(ResultsView));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            var position = $"{item}";

            return position switch
            {
                "1" => CitySearch,
                "2" => Categories,
                "3" => StartPoint,
                "4" => EndPoint,
                "5" => TripSettings,
                "6" => SearchPOIs,
                "7" => AlgoSettings,
                "8" => Results,
                _ => null,
            };
        }
    }
}
