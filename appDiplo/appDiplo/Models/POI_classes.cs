using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appDiplo.Models
{
    //===================================================================================
    //POI object
    public struct OpeningHours
    {
        public int DayNumber;
        public int CloseHour;
        public int CloseMinute;
        public int OpenHour;
        public int OpenMinute;
    }

    /// <summary>
    /// Main class representing Point of Interest
    /// </summary>
    public class POI
    {
        public string Name { get; set; }
        public MainGeocodes Geocodes { get; set; }
        public Location Location { get; set; }
        public double Rating { get; set; }
        public double OriginalRating { get; }
        public int MaxShift { get; set; }
        public int Arrival { get; set; }
        public string ArrivalString { get; set; }
        public int Wait { get; set; }
        public Dictionary<int, OpeningHours> Opening { get; set; }
        public int? Duration { get; set; }
        public int RatingNumber { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public int Day { get; set; }
        private List<int>? travelTime;
        private List<double>? distance;
        private List<POI>? neighbours;
        private List<double>? pheromones;
        public POI() { }
        public List<POI> Neighbours
        {
            get
            {
                if (neighbours == null)
                    neighbours = new List<POI>();
                return neighbours;
            }
            set
            {
                neighbours = value;
            }
        }

        public List<double> Pheromones
        {
            get
            {
                if (pheromones == null)
                    pheromones = new List<double>();
                return pheromones;
            }
            set
            {
                pheromones = value;
            }
        }
        public List<int> TravelTime
        {
            get
            {
                if (travelTime == null)
                    travelTime = new List<int>();
                return travelTime;
            }
        }

        public List<double> Distance
        {
            get
            {
                if (distance == null)
                    distance = new List<double>();
                return distance;
            }
        }
        public POI(string poi_name, MainGeocodes poi_geocodes, int duration, double poi_rating, int open_hour, int close_hour)
        {
            Name = poi_name;
            Geocodes = poi_geocodes;
            Duration = duration;
            Rating = poi_rating;
            Opening = new Dictionary<int, OpeningHours>();
            Opening[0] = new OpeningHours() { CloseHour = close_hour, OpenHour = open_hour};
            OriginalRating = poi_rating;
        }

        public POI(string poiName, MainGeocodes poiGeocodes, Location poiLocation)
        {
            Name = poiName;
            Geocodes = poiGeocodes;
            Location = poiLocation;
        }

        public POI(string poi_name, MainGeocodes poi_geocodes, Location poi_location, double poi_rating, Stats? poi_stats, IList<Regular> poi_oh, string? poi_tel, string? poi_email, string? poi_website, List<Category> categories, int duration = 120)
        {
            MaxShift = 0;
            Arrival = 0;
            ArrivalString = "";
            Wait = 0;
            int hour, minute;
            bool open_given;
            Opening = new Dictionary<int, OpeningHours>();
            Name = poi_name;
            Geocodes = poi_geocodes;
            Location = poi_location;
            Rating = Math.Round(poi_rating,2);
            OriginalRating = Rating;
            Duration = duration;
            if (poi_oh != null)
            {
                foreach (Regular day in poi_oh)
                {
                    OpeningHours oh = new OpeningHours();
                    hour = Convert.ToInt16(char.GetNumericValue(day.close[0]) * 10 + char.GetNumericValue(day.close[1]));
                    minute = Convert.ToInt16(char.GetNumericValue(day.close[2]) * 10 + char.GetNumericValue(day.close[3]));
                    oh.CloseHour = hour * 3600 + minute * 60;
                    oh.CloseMinute = minute;
                    hour = Convert.ToInt16(char.GetNumericValue(day.open[0]) * 10 + char.GetNumericValue(day.open[1]));
                    minute = Convert.ToInt16(char.GetNumericValue(day.open[2]) * 10 + char.GetNumericValue(day.open[3]));
                    oh.OpenHour = hour * 3600 + minute * 60;
                    oh.OpenMinute = minute;
                    oh.DayNumber = day.day;
                    Opening[day.day] = oh;
                }
                open_given = true;
            }
            else
            {
                for (int i = 0; i < categories.Count; i++)
                {
                    if (categories[i].id > 15999 && categories[i].id < 16060)
                    {
                        for (int j = 1; j < 8; j++)
                        {
                            Opening[j] = new OpeningHours() { OpenHour = 0, CloseHour = 23*3600+59*60 };
                        }
                        open_given = true;
                        break;
                    }
                    else
                        open_given=false;
                }
            }
            
            if (poi_stats != null)
                RatingNumber = poi_stats.total_ratings;
            else
                RatingNumber = 0;
            if (poi_tel != null)
                Tel = poi_tel;
            else
                Tel = "";
            if (poi_email != null)
                Email = poi_email;
            else
                Email = "";
            if (poi_website != null)
                Website = poi_website;
            else
                Website = "";
        }
    }

    public class JsonObject
    {
        public List<FinalRoute> route { get; set; }
    }

    public class FinalRoute
    {
        public IList<FinalCoords> coords { get; set; }
    }

    public class FinalPOIS
    {
        public MainGeocodes geocodes { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string tel { get; set; }
        public string website { get; set; }
        public double rating { get; set; }
        public Dictionary<int, OpeningHours> Opening { get; set; }
    }

    public class FinalCoords
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
