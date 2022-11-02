using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;
using appDiplo.Models;
using System.Windows.Input;
using Xamarin.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MvvmHelpers.Commands;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;


namespace appDiplo.ViewModels
{
    public class Grouping<K, T> : ObservableRangeCollection<T>
    {
        public K GroupKey { get; private set; }

        public Grouping(K key, IEnumerable<T> items)
        {
            GroupKey = key;
            foreach (var item in items)
                this.Items.Add(item);
        }
    }

    public class DayGroup : ObservableRangeCollection<POI>
    {
        public string Name { get; private set; }

        public DayGroup(string name)
            : base()
        {
            Name = name;
        }

        public DayGroup(string name, IEnumerable<POI> source)
            : base(source)
        {
            Name = name;
        }
    }
    public class ResultsViewModel : ObservableObject
    {
        public ObservableRangeCollection<POI> ResultsPOIs { get; set; }
        public ObservableRangeCollection<Grouping<string, POI>> POIGroupingDays { get; set; }
        public ICommand ShowMapPage { get; }
        public ICommand StartILS { get; }
        public ICommand StartSAILS { get; }
        public ICommand StartACO { get; }
        public ICommand StartSAILSACS { get; }


        private CitySearchViewModel city;
        private CategoriesViewModel cat;
        private StartPointViewModel startPoint;
        private EndPointViewModel endPoint;
        private TripSettingsViewModel settings;
        private POISViewModel pois;
        public RetrievingData RD;
        public MainPageViewModel main;
        public AlgoSettingsViewModel algoSettings;
        private Dictionary<string,string> mapData;
        private double totalRating;
        private int totalLength;
        private bool isBusy;
        public List<string> DayList;
        public ObservableRangeCollection<Grouping<string, POI>> POIS {get; set;}
        public ObservableRangeCollection<DayGroup> POIsList { get; set; } = new ObservableRangeCollection<DayGroup>();
        public ResultsViewModel(CitySearchViewModel city, CategoriesViewModel cat, StartPointViewModel startPoint, EndPointViewModel endPoint, TripSettingsViewModel settings, POISViewModel pois, AlgoSettingsViewModel algoSettings, MainPageViewModel main)
        {
            this.city = city;
            this.cat = cat;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.settings = settings;
            this.pois = pois;
            this.algoSettings = algoSettings;
            this.main = main;

            ResultsPOIs = new ObservableRangeCollection<POI>();
            ShowMapPage = new MvvmHelpers.Commands.Command(DoShowMapPage);
            StartILS = new AsyncCommand(DoStartILS);
            StartSAILS = new AsyncCommand(DoStartSAILS);
            StartACO = new AsyncCommand(DoStartACO);
            StartSAILSACS = new AsyncCommand(DoStartSAILSACS);

            RD = new RetrievingData();
            mapData = new Dictionary<string, string>();
            totalRating = 0;
            POIS = new ObservableRangeCollection<Grouping<string, POI>>();
            isBusy = false;
            DayList = new List<string>();
        }

        public double TotalRating
        {
            get => totalRating;
            set
            {
                if (totalRating == value)
                    return;
                totalRating = value;
                OnPropertyChanged();
            }
        }

        public int TotalLength
        {
            get => totalLength;
            set
            {
                if (totalLength == value)
                    return;
                totalLength = value;
                OnPropertyChanged();
            }
        }
        private async void DoShowMapPage()
        {
            string day = await Application.Current.MainPage.DisplayActionSheet("Select the day!", null, "Cancel", DayList.ToArray());
            if (day != null)
            {
                if (day != "Cancel") 
                {
                    try
                    {
                        await Shell.Current.GoToAsync($"mapPage?displayData={mapData[day]}&latitude={startPoint.Point.Geocodes.latitude}&longitude={startPoint.Point.Geocodes.longitude}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
        }

        private async Task DisplayIsComputingy()
        {
            await Application.Current.MainPage.DisplayAlert("Already computing!", null, "OK");
        }

        private async Task DoStartILS()
        {
            ILS ils;
            List<int> days = new List<int>();
            List<FinalRoute> finalRoutes = new();
            List<string> finalData = new List<string>();
            Route[] results;
            int length = 0;
            int dayLength = 0;
            int actInd;
            int hour, minute;
            string dayName = "Monday";
            string endNodeArrival;
            double finalRating = 0;

            if (isBusy)
            {
                await DisplayIsComputingy();
                return;
            }

            isBusy = true;

            foreach (POIViewModel poi in pois.POIs)
            {
                if (poi.Poi.Rating == 0)
                    RD.POIs.Remove(poi.Poi);
            }
            if (main.POIsChanged)
            {
                await RD.CreateGraph(startPoint.Point, endPoint.Point);
                main.POIsChanged = false;
            }
            foreach (DayViewModel day in settings.Days)
            {
                if (day.IsSelected)
                {
                    days.Add(day.Num);
                    if (endPoint.Point == startPoint.Point)
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600, CloseHour = day.End * 3600 };
                    }
                    else
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { CloseHour = day.End * 3600 };
                        startPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600 };
                    }
                }

            }

            ils = new ILS(RD.Graph, days, algoSettings.IlsThreshold1, algoSettings.IlsThreshold2, algoSettings.IlsThreshold3, algoSettings.IlsF, startPoint.Point, endPoint.Point);
            await Task.Run(() => ils.StartILS(algoSettings.IlsTime));
            results = ils.S0_star;

            DayList.Clear();
            mapData.Clear();
            POIsList.Clear();

            //for (int i = 0; i < results.Length; i++)
            //{
            //    actInd = ils.startNode.Neighbours.IndexOf(results[i].RouteList[0]);
            //    length += (int)ils.startNode.Distance[actInd];
            //    length += (int)results[i].RouteList[0].Duration;
            //    for (int j = 1; j < results[i].RouteList.Count; j++)
            //    {
            //        actInd = results[i].RouteList[j - 1].Neighbours.IndexOf(results[i].RouteList[j]);
            //        length += (int)results[i].RouteList[j - 1].Distance[actInd];
            //        length += (int)results[i].RouteList[j].Duration;
            //    }
            //    actInd = ils.endNode.Neighbours.IndexOf(results[i].RouteList[results[i].RouteList.Count - 1]);
            //    length += (int)ils.endNode.Distance[actInd];
            //}

            for (int i = 0; i < results.Length; i++)
            {
                actInd = ils.startNode.Neighbours.IndexOf(results[i].RouteList[0]);
                dayLength = ils.startNode.Opening[results[i].DayNum].OpenHour;
                length += (int)ils.startNode.Distance[actInd];
                dayLength += (int)ils.startNode.Distance[actInd];
                results[i].RouteList[0].Arrival = dayLength;
                hour = (int)Math.Floor((double)dayLength / 3600);
                minute = dayLength % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                length += (int)results[i].RouteList[0].Duration;
                dayLength += (int)results[i].RouteList[0].Duration;
                finalRating += results[i].RouteList[0].Rating;
                for (int j = 1; j < results[i].RouteList.Count; j++)
                {
                    actInd = results[i].RouteList[j - 1].Neighbours.IndexOf(results[i].RouteList[j]);
                    length += (int)results[i].RouteList[j - 1].Distance[actInd];
                    dayLength += (int)results[i].RouteList[j - 1].Distance[actInd];
                    results[i].RouteList[j].Arrival = dayLength;
                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = dayLength % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                    length += (int)results[i].RouteList[j].Duration;
                    dayLength += (int)results[i].RouteList[j].Duration;
                    finalRating += results[i].RouteList[j].Rating;
                }
                actInd = results[i].RouteList[results[i].RouteList.Count - 1].Neighbours.IndexOf(ils.endNode);
                length += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];
                dayLength += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];

                hour = (int)Math.Floor((double)dayLength / 3600);
                minute = dayLength % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                if (minute < 10)
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                else
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                results[i].RouteList.Add(new POI() { ArrivalString = endNodeArrival, Name = ils.endNode.Name, Rating = finalRating, Geocodes = new MainGeocodes() { latitude = ils.endNode.Geocodes.latitude, longitude = ils.endNode.Geocodes.longitude } });
                hour = (int)Math.Floor((double)ils.startNode.Opening[results[i].DayNum].OpenHour / 3600);
                minute = ils.startNode.Opening[results[i].DayNum].OpenHour % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                if (minute < 10)
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                else
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                results[i].RouteList.Insert(0, new POI() { ArrivalString = endNodeArrival, Name = ils.startNode.Name, Geocodes = new MainGeocodes() { latitude = ils.startNode.Geocodes.latitude, longitude = ils.startNode.Geocodes.longitude } });

                finalRating = 0;
            }

            foreach (Route route in results)
            {
                finalRoutes.Add(new FinalRoute { coords = new List<FinalCoords>() });
                switch (route.DayNum)
                {
                    case 1:
                        dayName = "Monday";
                        break;
                    case 2:
                        dayName = "Tuesday";
                        break;
                    case 3:
                        dayName = "Wednesday";
                        break;
                    case 4:
                        dayName = "Thursday";
                        break;
                    case 5:
                        dayName = "Friday";
                        break;
                    case 6:
                        dayName = "Saturday";
                        break;
                    case 7:
                        dayName = "Sunday";
                        break;
                }
                POIsList.Add(new DayGroup(dayName, route.RouteList));
                DayList.Add(dayName);
                finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = ils.startNode.Geocodes.latitude, longitude = ils.startNode.Geocodes.longitude });
                // finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = ils.startNode.Name, email = ils.startNode.Email, Opening = ils.startNode.Opening, tel = ils.startNode.Tel, website = ils.startNode.Website, rating = ils.startNode.Rating, geocodes = ils.startNode.Geocodes });
                foreach (POI poi in route.RouteList)
                {
                    finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = poi.Geocodes.latitude, longitude = poi.Geocodes.longitude });
                    //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = poi.Name, email = poi.Email, Opening = poi.Opening, tel = poi.Tel, website = poi.Website, rating = poi.Rating, geocodes = poi.Geocodes });
                }
                finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = ils.endNode.Geocodes.latitude, longitude = ils.endNode.Geocodes.longitude });
                //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = ils.startNode.Name, email = ils.startNode.Email, Opening = ils.startNode.Opening, tel = ils.startNode.Tel, website = ils.startNode.Website, rating = ils.startNode.Rating, geocodes = ils.startNode.Geocodes });
                mapData.Add(dayName, JsonConvert.SerializeObject(finalRoutes[finalRoutes.Count - 1]));
            }

            TotalLength = length;
            TotalRating = ils.S0_star_score;

            isBusy = false;
        }

        private async Task DoStartSAILS()
        {
            SAILS sails;
            List<int> days = new List<int>();
            List<FinalRoute> finalRoutes = new();
            List<string> finalData = new List<string>();
            Route[] results;
            int length = 0;
            int dayLength = 0;
            int actInd;
            int hour, minute;
            string dayName = "Monday";
            string endNodeArrival;
            double finalRating = 0;

            if (isBusy)
            {
                await DisplayIsComputingy();
                return;
            }

            isBusy = true;

            foreach (POIViewModel poi in pois.POIs)
            {
                if (poi.Poi.Rating == 0)
                    RD.POIs.Remove(poi.Poi);
            }
            if (main.POIsChanged)
            {
                await RD.CreateGraph(startPoint.Point, endPoint.Point);
                main.POIsChanged = false;
            }
            foreach (DayViewModel day in settings.Days)
            {
                if (day.IsSelected)
                {
                    days.Add(day.Num);
                    if (endPoint.Point == startPoint.Point)
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600, CloseHour = day.End * 3600 };
                    }
                    else
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { CloseHour = day.End * 3600 };
                        startPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600 };
                    }
                }

            }
            sails = new SAILS(RD.Graph, days, algoSettings.SailsThreshold2, algoSettings.SailsThreshold3, algoSettings.SailsF, algoSettings.SailsT0, algoSettings.SailsAlpha, algoSettings.SailsMaxInnerLoop, algoSettings.SailsLimit, startPoint.Point, endPoint.Point);
            await Task.Run(() => sails.StartSAILS(algoSettings.SailsTime));
            results = sails.S0_star;

            DayList.Clear();
            mapData.Clear();
            POIsList.Clear();

            for (int i = 0; i < results.Length; i++)
            {
                actInd = sails.startNode.Neighbours.IndexOf(results[i].RouteList[0]);
                dayLength = sails.startNode.Opening[results[i].DayNum].OpenHour;
                length += (int)sails.startNode.Distance[actInd];
                dayLength += (int)sails.startNode.Distance[actInd];
                results[i].RouteList[0].Arrival = dayLength;
                hour = (int)Math.Floor((double)dayLength / 3600);
                minute = dayLength % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                finalRating += results[i].RouteList[0].Rating;
                if (minute < 10)
                    results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                else
                    results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                length += (int)results[i].RouteList[0].Duration;
                dayLength += (int)results[i].RouteList[0].Duration;
                for (int j = 1; j < results[i].RouteList.Count; j++)
                {
                    actInd = results[i].RouteList[j - 1].Neighbours.IndexOf(results[i].RouteList[j]);
                    length += (int)results[i].RouteList[j - 1].Distance[actInd];
                    dayLength += (int)results[i].RouteList[j - 1].Distance[actInd];
                    results[i].RouteList[j].Arrival = dayLength;
                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = dayLength % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    if (minute < 10)
                    {
                        results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    }
                    else
                        results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                    length += (int)results[i].RouteList[j].Duration;
                    dayLength += (int)results[i].RouteList[j].Duration;
                    finalRating += results[i].RouteList[j].Rating;
                }
                actInd = results[i].RouteList[results[i].RouteList.Count - 1].Neighbours.IndexOf(sails.endNode);
                length += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];
                dayLength += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];

                hour = (int)Math.Floor((double)dayLength / 3600);
                minute = dayLength % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                if (minute < 10)
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                else
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                results[i].RouteList.Add(new POI() { ArrivalString = endNodeArrival, Name = sails.endNode.Name, Rating = finalRating, Geocodes = new MainGeocodes() { latitude = sails.endNode.Geocodes.latitude, longitude = sails.endNode.Geocodes.longitude } });
                hour = (int)Math.Floor((double)sails.startNode.Opening[results[i].DayNum].OpenHour / 3600);
                minute = sails.startNode.Opening[results[i].DayNum].OpenHour % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                if (minute < 10)
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                else
                    endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                results[i].RouteList.Insert(0, new POI() { ArrivalString = endNodeArrival, Name = sails.startNode.Name, Geocodes = new MainGeocodes() { latitude = sails.startNode.Geocodes.latitude, longitude = sails.startNode.Geocodes.longitude } });

                finalRating = 0;
            }

            foreach (Route route in results)
            {
                finalRoutes.Add(new FinalRoute { coords = new List<FinalCoords>() });
                switch (route.DayNum)
                {
                    case 1:
                        dayName = "Monday";
                        break;
                    case 2:
                        dayName = "Tuesday";
                        break;
                    case 3:
                        dayName = "Wednesday";
                        break;
                    case 4:
                        dayName = "Thursday";
                        break;
                    case 5:
                        dayName = "Friday";
                        break;
                    case 6:
                        dayName = "Saturday";
                        break;
                    case 7:
                        dayName = "Sunday";
                        break;
                }
                POIsList.Add(new DayGroup(dayName, route.RouteList));
                DayList.Add(dayName);
                finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = sails.startNode.Geocodes.latitude, longitude = sails.startNode.Geocodes.longitude });
                // finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = sails.startNode.Name, email = sails.startNode.Email, Opening = sails.startNode.Opening, tel = sails.startNode.Tel, website = sails.startNode.Website, rating = sails.startNode.Rating, geocodes = sails.startNode.Geocodes });
                foreach (POI poi in route.RouteList)
                {
                    finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = poi.Geocodes.latitude, longitude = poi.Geocodes.longitude });
                    //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = poi.Name, email = poi.Email, Opening = poi.Opening, tel = poi.Tel, website = poi.Website, rating = poi.Rating, geocodes = poi.Geocodes });
                }
                finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = sails.endNode.Geocodes.latitude, longitude = sails.endNode.Geocodes.longitude });
                //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = sails.startNode.Name, email = sails.startNode.Email, Opening = sails.startNode.Opening, tel = sails.startNode.Tel, website = sails.startNode.Website, rating = sails.startNode.Rating, geocodes = sails.startNode.Geocodes });
                mapData.Add(dayName, JsonConvert.SerializeObject(finalRoutes[finalRoutes.Count - 1]));
            }


            TotalLength = length;
            TotalRating = sails.S0_star_score;

            isBusy = false;
        }


        private async Task DoStartSAILSACS()
        {
            List<int> days = new List<int>();
            List<FinalRoute> finalRoutes = new();
            List<string> finalData = new List<string>();
            Route[] results;
            int length = 0;
            int dayLength = 0;
            int actInd;
            int hour, minute;
            string dayName = "Monday";
            double finalRating = 0;
            string endNodeArrival;

            if (isBusy)
            {
                await DisplayIsComputingy();
                return;
            }

            isBusy = true;

            foreach (POIViewModel poi in pois.POIs)
            {
                if (poi.Poi.Rating == 0)
                    RD.POIs.Remove(poi.Poi);
            }
            if (main.POIsChanged)
            {
                await RD.CreateGraph(startPoint.Point, endPoint.Point);
                main.POIsChanged = false;
            }
            foreach (DayViewModel day in settings.Days)
            {
                if (day.IsSelected)
                {
                    days.Add(day.Num);
                    if (endPoint.Point == startPoint.Point)
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600, CloseHour = day.End * 3600 };
                    }
                    else
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { CloseHour = day.End * 3600 };
                        startPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600 };
                    }
                }

            }
            SAILSACS sailsacs = new SAILSACS(RD.Graph, days, algoSettings.SailsThreshold2, algoSettings.SailsThreshold3, algoSettings.SailsF, algoSettings.SailsT0, algoSettings.SailsAlpha, algoSettings.SailsMaxInnerLoop, algoSettings.SailsLimit, algoSettings.AntsNumber, algoSettings.AntsRho, algoSettings.AntsPsi, algoSettings.AntsQ0, algoSettings.AntsPheromone, startPoint.Point, endPoint.Point);
            await Task.Run(() => sailsacs.StartSAILS(algoSettings.SailsTime, algoSettings.AntsTime));
            results = sailsacs.S0_star;


            DayList.Clear();
            mapData.Clear();
            POIsList.Clear();

            //for (int i = 0; i < results.Length; i++)
            //{
            //    actInd = sailsacs.startNode.Neighbours.IndexOf(results[i].RouteList[0]);
            //    length += (int)sailsacs.startNode.Distance[actInd];
            //    length += (int)results[i].RouteList[0].Duration;
            //    for (int j = 1; j < results[i].RouteList.Count; j++)
            //    {
            //        actInd = results[i].RouteList[j-1].Neighbours.IndexOf(results[i].RouteList[j]);
            //        length += (int)results[i].RouteList[j - 1].Distance[actInd];
            //        length += (int)results[i].RouteList[j].Duration;
            //    }
            //    actInd = sailsacs.endNode.Neighbours.IndexOf(results[i].RouteList[results[i].RouteList.Count-1]);
            //    length += (int)sailsacs.endNode.Distance[actInd];
            //}

            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].RouteList.Count > 0)
                {
                    actInd = sailsacs.startNode.Neighbours.IndexOf(results[i].RouteList[0]);
                    dayLength = sailsacs.startNode.Opening[results[i].DayNum].OpenHour;
                    length += (int)sailsacs.startNode.Distance[actInd];
                    dayLength += (int)sailsacs.startNode.Distance[actInd];
                    results[i].RouteList[0].Arrival = dayLength;
                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = dayLength % 3600;
                    minute = (int)Math.Round((double)minute / 60);
                    if (minute < 10)
                        results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    else
                        results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                    length += (int)results[i].RouteList[0].Duration;
                    dayLength += (int)results[i].RouteList[0].Duration;
                    finalRating += results[i].RouteList[0].Rating;
                    for (int j = 1; j < results[i].RouteList.Count; j++)
                    {
                        actInd = results[i].RouteList[j - 1].Neighbours.IndexOf(results[i].RouteList[j]);
                        length += (int)results[i].RouteList[j - 1].Distance[actInd];
                        dayLength += (int)results[i].RouteList[j - 1].Distance[actInd];
                        results[i].RouteList[j].Arrival = dayLength;
                        hour = (int)Math.Floor((double)dayLength / 3600);
                        minute = dayLength % 3600;
                        minute = (int)Math.Round((double)minute / 60);
                        if (minute < 10)
                            results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                        else
                            results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                        length += (int)results[i].RouteList[j].Duration;
                        dayLength += (int)results[i].RouteList[j].Duration;
                        finalRating += results[i].RouteList[j].Rating;
                    }
                    actInd = results[i].RouteList[results[i].RouteList.Count - 1].Neighbours.IndexOf(sailsacs.endNode);
                    length += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];
                    dayLength += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];

                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = dayLength % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    if (minute < 10)
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    else
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                    results[i].RouteList.Add(new POI() { ArrivalString = endNodeArrival, Name = sailsacs.endNode.Name, Rating = finalRating, Geocodes = new MainGeocodes() { latitude = sailsacs.endNode.Geocodes.latitude, longitude = sailsacs.endNode.Geocodes.longitude } });
                    hour = (int)Math.Floor((double)sailsacs.startNode.Opening[results[i].DayNum].OpenHour / 3600);
                    minute = sailsacs.startNode.Opening[results[i].DayNum].OpenHour % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    if (minute < 10)
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    else
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                    results[i].RouteList.Insert(0, new POI() { ArrivalString = endNodeArrival, Name = sailsacs.startNode.Name, Geocodes = new MainGeocodes() { latitude = sailsacs.startNode.Geocodes.latitude, longitude = sailsacs.startNode.Geocodes.longitude } });

                    finalRating = 0;
                }
            }

            foreach (Route route in results)
            {
                finalRoutes.Add(new FinalRoute { coords = new List<FinalCoords>() });
                switch (route.DayNum)
                {
                    case 1:
                        dayName = "Monday";
                        break;
                    case 2:
                        dayName = "Tuesday";
                        break;
                    case 3:
                        dayName = "Wednesday";
                        break;
                    case 4:
                        dayName = "Thursday";
                        break;
                    case 5:
                        dayName = "Friday";
                        break;
                    case 6:
                        dayName = "Saturday";
                        break;
                    case 7:
                        dayName = "Sunday";
                        break;
                }
                POIsList.Add(new DayGroup(dayName, route.RouteList));
                DayList.Add(dayName);
                finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = sailsacs.startNode.Geocodes.latitude, longitude = sailsacs.startNode.Geocodes.longitude });
                //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = sailsacs.startNode.Name, email = sailsacs.startNode.Email, Opening = sailsacs.startNode.Opening, tel = sailsacs.startNode.Tel, website = sailsacs.startNode.Website, rating = sailsacs.startNode.Rating, geocodes = sailsacs.startNode.Geocodes });
                foreach (POI poi in route.RouteList)
                {
                    finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = poi.Geocodes.latitude, longitude = poi.Geocodes.longitude });
                    //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = poi.Name, email = poi.Email, Opening = poi.Opening, tel = poi.Tel, website = poi.Website, rating = poi.Rating, geocodes = poi.Geocodes });
                }
                finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = sailsacs.endNode.Geocodes.latitude, longitude = sailsacs.endNode.Geocodes.longitude });
                //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = sailsacs.startNode.Name, email = sailsacs.startNode.Email, Opening = sailsacs.startNode.Opening, tel = sailsacs.startNode.Tel, website = sailsacs.startNode.Website, rating = sailsacs.startNode.Rating, geocodes = sailsacs.startNode.Geocodes });
                mapData.Add(dayName, JsonConvert.SerializeObject(finalRoutes[finalRoutes.Count - 1]));
            }

            TotalLength = length;
            TotalRating = sailsacs.S0_star_score;

            isBusy = false;
        }

        private async Task DoStartACO()
        {
            ACO aco;
            List<int> days = new List<int>();
            List<AntRoute> results;
            List<FinalRoute> finalRoutes = new();
            List<string> finalData = new List<string>();
            int length = 0;
            int dayLength = 0;
            int actInd;
            int hour, minute;
            string dayName = "Monday";
            double finalRating = 0;
            string endNodeArrival;

            if (isBusy)
            {
                await DisplayIsComputingy();
                return;
            }

            isBusy = true;

            foreach (POIViewModel poi in pois.POIs)
            {
                if (poi.Poi.Rating == 0)
                    RD.POIs.Remove(poi.Poi);
            }
            if (main.POIsChanged)
            {
                await RD.CreateGraph(startPoint.Point, endPoint.Point);
                main.POIsChanged = false;
            }
            foreach (DayViewModel day in settings.Days)
            {
                if (day.IsSelected)
                {
                    days.Add(day.Num);
                    if (endPoint.Point == startPoint.Point)
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600, CloseHour = day.End * 3600 };
                    }
                    else
                    {
                        endPoint.Point.Opening[day.Num] = new OpeningHours { CloseHour = day.End * 3600 };
                        startPoint.Point.Opening[day.Num] = new OpeningHours { OpenHour = day.Start * 3600 };
                    }
                }

            }
            aco = new ACO(5, RD.Graph.Nodes, startPoint.Point, endPoint.Point, days);
            await Task.Run(() => aco.StartACO(algoSettings.AntsTime, algoSettings.AntsPheromone, algoSettings.AntsRho, algoSettings.AntsPsi, algoSettings.AntsQ0));
            results = aco.BestRoute;

            DayList.Clear();
            mapData.Clear();
            POIsList.Clear();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].RouteList.Count > 0)
                {
                    actInd = aco.startNode.Neighbours.IndexOf(results[i].RouteList[0]);
                    dayLength = aco.startNode.Opening[results[i].DayNum].OpenHour;
                    length += (int)aco.startNode.Distance[actInd];
                    dayLength += (int)aco.startNode.Distance[actInd];
                    results[i].RouteList[0].Arrival = dayLength;
                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = dayLength % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    if (minute < 10)
                        results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    else
                        results[i].RouteList[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                    finalRating += results[i].RouteList[0].Rating;
                    length += (int)results[i].RouteList[0].Duration;
                    dayLength += (int)results[i].RouteList[0].Duration;
                    for (int j = 1; j < results[i].RouteList.Count; j++)
                    {
                        finalRating += results[i].RouteList[j].Rating;
                        actInd = results[i].RouteList[j - 1].Neighbours.IndexOf(results[i].RouteList[j]);
                        length += (int)results[i].RouteList[j - 1].Distance[actInd];
                        dayLength += (int)results[i].RouteList[j - 1].Distance[actInd];
                        results[i].RouteList[j].Arrival = dayLength;
                        hour = (int)Math.Floor((double)dayLength / 3600);
                        minute = dayLength % 3600;
                        minute = (int)Math.Floor((double)minute / 60);
                        if (minute < 10)
                            results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                        else
                            results[i].RouteList[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                        length += (int)results[i].RouteList[j].Duration;
                        dayLength += (int)results[i].RouteList[j].Duration;
                    }
                    actInd = results[i].RouteList[results[i].RouteList.Count - 1].Neighbours.IndexOf(aco.endNode);
                    length += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];
                    dayLength += (int)results[i].RouteList[results[i].RouteList.Count - 1].Distance[actInd];

                    if (dayLength > 64800)
                        Console.WriteLine("Huh");

                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = dayLength % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    if (minute < 10)
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    else
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                    results[i].RouteList.Add(new POI() { ArrivalString = endNodeArrival, Name = aco.endNode.Name, Rating = finalRating, Geocodes = new MainGeocodes() { latitude = aco.endNode.Geocodes.latitude, longitude = aco.endNode.Geocodes.longitude } });
                    hour = (int)Math.Floor((double)aco.startNode.Opening[results[i].DayNum].OpenHour/ 3600);
                    minute = aco.startNode.Opening[results[i].DayNum].OpenHour % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    if (minute < 10)
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute.ToString("D2"));
                    else
                        endNodeArrival = string.Format("Arrival at {0}:{1}", hour, minute);
                    results[i].RouteList.Insert(0, new POI() { ArrivalString = endNodeArrival, Name = aco.startNode.Name, Geocodes = new MainGeocodes() { latitude = aco.startNode.Geocodes.latitude, longitude = aco.startNode.Geocodes.longitude } });
                    
                    finalRating = 0;
                }
            }

            foreach (AntRoute route in results)
            {
                finalRoutes.Add(new FinalRoute { coords = new List<FinalCoords>() });
                switch (route.DayNum)
                {
                    case 1:
                        dayName = "Monday";
                        break;
                    case 2:
                        dayName = "Tuesday";
                        break;
                    case 3:
                        dayName = "Wednesday";
                        break;
                    case 4:
                        dayName = "Thursday";
                        break;
                    case 5:
                        dayName = "Friday";
                        break;
                    case 6:
                        dayName = "Saturday";
                        break;
                    case 7:
                        dayName = "Sunday";
                        break;
                }
                POIsList.Add(new DayGroup(dayName, route.RouteList));
                DayList.Add(dayName);
                //finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = aco.startNode.Geocodes.latitude, longitude = aco.startNode.Geocodes.longitude });
                //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = aco.startNode.Name, email = aco.startNode.Email, Opening = aco.startNode.Opening, tel = aco.startNode.Tel, website = aco.startNode.Website, rating = aco.startNode.Rating, geocodes = aco.startNode.Geocodes });
                foreach (POI poi in route.RouteList)
                {
                    finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = poi.Geocodes.latitude, longitude = poi.Geocodes.longitude });
                    //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = poi.Name, email = poi.Email, Opening = poi.Opening, tel = poi.Tel, website = poi.Website, rating = poi.Rating, geocodes = poi.Geocodes });
                }
                //finalRoutes[finalRoutes.Count - 1].coords.Add(new FinalCoords { latitude = aco.endNode.Geocodes.latitude, longitude = aco.endNode.Geocodes.longitude });
                //finalRoutes[finalRoutes.Count - 1].pois.Add(new FinalPOIS() { name = aco.startNode.Name, email = aco.startNode.Email, Opening = aco.startNode.Opening, tel = aco.startNode.Tel, website = aco.startNode.Website, rating = aco.startNode.Rating, geocodes = aco.startNode.Geocodes });
                mapData.Add(dayName,JsonConvert.SerializeObject(finalRoutes[finalRoutes.Count - 1]));
            }

            TotalLength = length;
            TotalRating = aco.BestScore;

            isBusy = false;
        }
    }
}