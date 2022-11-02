using System;
using System.Collections.Generic;
using System.Text;
using appDiplo.Models;
using System.Windows.Input;
using Xamarin.Forms;
using MvvmHelpers;

namespace appDiplo.ViewModels
{
    public class POIViewModel : ObservableObject
    {
        private POI poi;
        public ICommand ChangeRating { get; set; }
        public ICommand ResetRating { get; set; }

        public POIViewModel(POI poi)
        {
            this.poi = poi;
            ChangeRating = new MvvmHelpers.Commands.Command(ChangeRatingPOI);
            ResetRating = new MvvmHelpers.Commands.Command(ResetRatingPOI);
        }

        private async void ChangeRatingPOI()
        {
            string result = await Application.Current.MainPage.DisplayPromptAsync("Change Rating", "Type new rating (0-10)", initialValue: "0.0", maxLength: 4, keyboard: Keyboard.Numeric);
            double newRating;
            try
            {
                newRating = Convert.ToDouble(result);
                if (newRating >= 0.0 && newRating <= 10.0)
                    Rating = newRating;
                else
                    await Application.Current.MainPage.DisplayAlert("Invalid range for rating!", "Please insert rating in range 0.0-10.0", "OK");
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Invalid rating!", "Please insert rating in range 0.0-10.0", "OK");
            }
            Console.WriteLine(result);
        }

        private void ResetRatingPOI()
        {
            Rating = poi.OriginalRating;
        }
        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(poi.Name))
                    return poi.Name;
                else
                    return "Not found.";
            }
        }

        public string Address
        {
            get
            {
                if (!string.IsNullOrEmpty(poi.Location.formatted_address))
                    return poi.Location.formatted_address;
                else
                {
                    if (!string.IsNullOrEmpty(poi.Location.address))
                        return poi.Location.address;
                    else
                        return "Not fournd.";
                }
            }
        }

        public string Website 
        { 
            get 
            {
                if (!string.IsNullOrEmpty(poi.Website))
                    return poi.Website;
                else
                    return "Not found.";
            } 
        }

        public string Tel
        {
            get
            {
                if (!string.IsNullOrEmpty(poi.Tel))
                    return poi.Tel;
                else
                    return "Not found.";
            }
        }
        public string Email
        {
            get
            {   
                if (!string.IsNullOrEmpty(poi.Email))
                    return poi.Email;
                else
                    return "Not found.";
            }
        }

        public double Rating
        {
            get
            {
                return Math.Round(poi.Rating,2,MidpointRounding.AwayFromZero);
            }
            set
            {
                poi.Rating = value;
                OnPropertyChanged("Rating");
            }
        }

        public POI Poi
        {
            get => poi;
        }
    }
}
