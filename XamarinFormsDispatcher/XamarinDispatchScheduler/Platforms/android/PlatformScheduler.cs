using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//https://github.com/xamarin/Xamarin.Forms/blob/d3d59ee4f0b3098457e1debe8d7b03d0d0061a53/Xamarin.Forms.Platform.Android/Forms.cs
namespace XamarinDispatchScheduler
{
    public class PlatformScheduler
    {
        static Handler s_handler;
        public static void BeginInvokeOnMainThread(Action action)
        {
            if (s_handler == null || s_handler.Looper != Looper.MainLooper)
            {
                s_handler = new Handler(Looper.MainLooper);
            }

            s_handler.Post(action);
        }

        public static IDisposable StartTimer(TimeSpan interval, Action callback)
        {
            var handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() =>
            {
                var t = Interlocked.Exchange(ref handler, null);
                if (t != null)
                {
                    callback();
                }

                t?.Dispose();
            },
            (long)interval.TotalMilliseconds);

            return
                 Disposable.Create(() =>
                 {
                     Interlocked.Exchange(ref handler, null)?.Dispose();
                 });
        }


        public static IDisposable StartInterval(TimeSpan interval, Action callback)
        {
            SerialDisposable disposable = new SerialDisposable();
            StartInterval(interval, callback, disposable);
            return disposable;
        }

        static void StartInterval(TimeSpan interval, Action callback, SerialDisposable disposable)
        {
            var handler = new Handler(Looper.MainLooper);
            handler.PostDelayed(() =>
            {
                var t = Interlocked.Exchange(ref handler, null);
                if (t != null)
                {
                    callback();
                    StartInterval(interval, callback, disposable);
                }

                t?.Dispose();
            }, (long)interval.TotalMilliseconds);

            disposable.Disposable =
                 Disposable.Create(() =>
                 {
                     Interlocked.Exchange(ref handler, null)?.Dispose();
                 });
        }

        public static bool OnMainThread()
        {
            return Looper.MyLooper() == Looper.MainLooper;
        }
    }
}
