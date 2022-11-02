using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using MvvmHelpers;

namespace appDiplo.Models
{
    // Classes for representing ListViews of routes grouped in days.
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
}
