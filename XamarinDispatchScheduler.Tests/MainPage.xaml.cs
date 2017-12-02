using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinDispatchScheduler.Tests
{
    public partial class MainPage : ContentPage
    {
        public static Func<bool> OnMainThread;

        public MainPage()
        {
            InitializeComponent();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            Observable
                .Timer(TimeSpan.FromSeconds(2))
                .Do(_ =>
                {
                    if (OnMainThread())
                    {
                        throw new Exception("Your test needs to be better");
                    }
                })
                .ObserveOn(XamarinDispatcherScheduler.Current)
                .Subscribe(_ =>
                {
                    if (!OnMainThread())
                    {
                        throw new Exception("Scheduler fail");
                    }
                    lblChangeMe.Text = "Changed";
                });

            var periodic =
                XamarinDispatcherScheduler
                    .Current
                    .SchedulePeriodic(TimeSpan.FromSeconds(2),
                    () =>
                    {
                        lblTimer.Text = DateTime.Now.TimeOfDay.ToString();
                    });

            btnStop.Clicked += (_, __) => periodic.Dispose();

            //test cancellation
            Observable
               .Timer(TimeSpan.FromSeconds(2))
                .ObserveOn(XamarinDispatcherScheduler.Current)
                .Subscribe(_ =>
                {
                    throw new Exception("test");
                })
                .Dispose();

            XamarinDispatcherScheduler.Current.Schedule(TimeSpan.FromSeconds(5), () =>
            {
                if (!String.IsNullOrWhiteSpace(lblSlowScheduler.Text))
                {
                    throw new Exception("Scheduler ran more than once");
                }

                lblSlowScheduler.Text = "slowly scheduled";
            }
            );
        }
    }
}
