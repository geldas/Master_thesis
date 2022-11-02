using appDiplo.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace appDiplo
{
    public interface IBaseUrl { string Get(); }
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            Routing.RegisterRoute("mainPage", typeof(MainPage));
            Routing.RegisterRoute("mapPage", typeof(MapPage));
            InitializeComponent();
        }
    }
}
