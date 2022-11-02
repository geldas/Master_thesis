using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using appDiplo.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace appDiplo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage: ContentPage
    {
        MapPageViewModel mapPageViewModel;
        public MapPage()
        {
            InitializeComponent();
            mapPageViewModel = new();
            this.BindingContext = mapPageViewModel;
        }
    }
}