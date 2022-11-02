using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using Xamarin.Forms;
using System.Windows.Input;
using appDiplo.Models;

namespace appDiplo.ViewModels
{
    public class POISViewModel : ObservableObject
    {
        public ObservableRangeCollection<POIViewModel> POIs { get; set; }

        public POISViewModel()
        {
            POIs = new ObservableRangeCollection<POIViewModel>();
        }

        public POISViewModel(List<POI> pois)
        {
            POIs = new ObservableRangeCollection<POIViewModel>();
            foreach (POI poi in pois)
            {
                POIs.Add(new POIViewModel(poi));
            }
        }

        public void FindPOIs()
        {
            RetrievingData rd = new RetrievingData();
        }
    }
}
