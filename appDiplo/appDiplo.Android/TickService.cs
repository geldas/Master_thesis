using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace appDiplo.Droid
{
    [Service]
    class TickService : Service
    {
        CancellationTokenSource cts;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int id)
        {
            cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                try
                {
                    appDiplo.Ticker t = new appDiplo.Ticker();
                    appDiplo.ViewModels.MapPageViewModel m = App.Current.MainPage.BindingContext as appDiplo.ViewModels.MapPageViewModel;
                    t.TickTock(cts.Token).Wait();
                    var main = App.Current.MainPage.BindingContext as ViewModels.MapPageViewModel;
                }
                catch (Android.OS.OperationCanceledException)
                {
                    if (cts.IsCancellationRequested)
                    {
                        appDiplo.CancelledMessage msg = new CancelledMessage();
                        Device.BeginInvokeOnMainThread(() => MessagingCenter.Send(msg, "CancelledMessage"));
                    }
                }
            }, cts.Token);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            if (cts != null)
            {
                cts.Token.ThrowIfCancellationRequested();
                cts.Cancel();
            }
            base.OnDestroy();
        }
    }
}