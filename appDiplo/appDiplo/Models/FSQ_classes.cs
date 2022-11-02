using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace appDiplo.Models
{
    //===================================================================================
    // Objects for JSON deserialization from FSQ
    public class FsqData
    {
        public IList<Results> results { get; set; }
    }

    public class Results
    {
        public string fsq_id { get; set; }
        public List<Category> categories { get; set; }
        public int distance { get; set; }
        public string email { get; set; }
        public Geocodes geocodes { get; set; }
        public Hours hours { get; set; }
        public Location location { get; set; }
        public string name { get; set; }
        public float rating { get; set; }
        public Stats stats { get; set; }
        public string tel { get; set; }
        public string website { get; set; }
    }

    public class Category
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class Geocodes
    {
        public MainGeocodes main { get; set; }
    }

    public class MainGeocodes
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }

    public class Hours
    {
        public string? display { get; set; }
        public bool is_local_holiday { get; set; }
        public bool open_now { get; set; }
        public IList<Regular> regular { get; set; }
    }

    public class Regular
    {
        public string close { get; set; }
        public int day { get; set; }
        public string open { get; set; }
    }

    public class Location
    {
        public string address { get; set; }
        public string country { get; set; }
        public string formatted_address { get; set; }
        public string locality { get; set; }
        public IList<string> neighborhood { get; set; }
        public string postcode { get; set; }
        public string region { get; set; }
    }

    public class Stats
    {
        public int total_photos { get; set; }
        public int total_ratings { get; set; }
        public int total_tips { get; set; }
    }

    public class AutoComplete
    {
        public List<AutoCompleteResults> results { get; set; }
    }

    public class AutoCompleteResults
    {
        public Geo geo { get; set; }
    }
    public class Geo
    {
        public string name { get; set; }
        public MainGeocodes center { get; set; }
    }
}
