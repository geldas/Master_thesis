using System;
using System.Collections.Generic;
using System.Text;
using appDiplo.Models;
using MvvmHelpers;
using Xamarin.Forms;
using System.ComponentModel;

namespace appDiplo.ViewModels
{
    public class CategoryViewModel : ObservableObject
    {
        public Category Category { get; set; }
        private bool isSelected;
        private string categoryColor;
        private int duration;
        
        private CategoriesViewModel parent;
        public CategoryViewModel(Category category, CategoriesViewModel parent)
        {
            Category = category;
            IsSelected = false;
            CategoryColor = "#605B5B";
            this.parent = parent;
            duration = -1;

        }
        public int Duration
        {
            get => duration;
            set
            {
                if (duration == value)
                    return;
                duration = value;
                OnPropertyChanged();
            }
        }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                bool change = true;
                if (isSelected == value)
                    return;
                isSelected = value;
                if (isSelected)
                {
                    CategoryColor = "Green";
                    if (duration == -1)
                        Duration = 60;
                    parent.IsOkay = true;
                }
                else
                {
                    foreach (CategoryViewModel cat in parent.Categories)
                    {
                        if (IsSelected)
                        {
                            change = false;
                            break;
                        }
                    }
                    if (change)
                        parent.IsOkay = false;
                    CategoryColor = "#605B5B";
                }
            }
        }

        public string CategoryColor
        {
            get => categoryColor;
            set
            {
                if (categoryColor == value)
                    return;
                categoryColor = value;
                OnPropertyChanged();
            }
        }
    }
    public class CategoriesViewModel : ObservableObject
    {
        public bool IsOkay { get; set; }
        public ObservableRangeCollection<CategoryViewModel> Categories { get; set; }

        public CategoriesViewModel() { }
        public CategoriesViewModel(List<Category> categories)
        {
            Categories = new ObservableRangeCollection<CategoryViewModel>();
            for (int i = 0; i < categories.Count; i++)
            {
                Categories.Add(new CategoryViewModel(categories[i], this));
            }
        }
    }
    public class DayViewModel : ObservableObject
    {
        public string Name { get; set; }
        public int Num { get; set; }
        private bool isSelected;
        private string dayColor;
        private string duration;
        private int start;
        private int end;

        private TripSettingsViewModel parent;
        public DayViewModel(string name, int num, TripSettingsViewModel parent)
        {
            Name = name;
            Num = num;
            IsSelected = false;
            DayColor = "#605B5B";
            this.parent = parent;
            Start = 10;
            End = 18;
        }

        public int Start
        {
            get => start;
            set
            {
                if (start == value)
                    return;
                start = value;
                OnPropertyChanged();
            }
        }
        public int End
        {
            get => end;
            set
            {
                if (end == value)
                    return;
                end = value;
                OnPropertyChanged();
            }
        }

        public string Duration
        {
            get => duration;
            set
            {
                if (duration == value)
                    return;
                duration = value;
                OnPropertyChanged();
            }
        }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                bool change = true;
                if (isSelected == value)
                    return;
                isSelected = value;
                if (isSelected)
                {
                    DayColor = "Green";
                    if (duration == null)
                        Duration = "120";
                    parent.IsOkay = true;
                }
                else
                {
                    DayColor = "#605B5B";
                    foreach (DayViewModel day in parent.Days)
                    {
                        if (day.IsSelected)
                        {
                            change = false;
                            break;
                        }
                    }
                    if (change)
                        parent.IsOkay = false;
                }
            }
        }

        public string DayColor
        {
            get => dayColor;
            set
            {
                if (dayColor == value)
                    return;
                dayColor = value;
                OnPropertyChanged();
            }
        }
    }
    public class TripSettingsViewModel : ObservableObject
    {
        private bool isOkay;
        public ObservableRangeCollection<DayViewModel> Days { get; set; }
        public string TripStart { get; set; }
        public string TripEnd { get; set; }
        public TripSettingsViewModel()
        {
            CreateDays();
            isOkay = false;
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
        private void CreateDays()
        {
            List<string> days = new List<string>() { "Mon.", "Tue.", "Wed.", "Thu.", "Fri.","Sat.", "Sun." };
            List<int> daysNum = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
            Days = new ObservableRangeCollection<DayViewModel>();
            for (int i = 0; i < daysNum.Count; i++)
            {
                Days.Add(new DayViewModel(days[i], daysNum[i], this));
            }
            TripStart = "10";
            TripEnd = "18";
        }
    }
}