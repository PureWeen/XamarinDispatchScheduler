using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//https://github.com/xamarin/Xamarin.Forms/blob/d3d59ee4f0b3098457e1debe8d7b03d0d0061a53/Xamarin.Forms.Platform.Android/Forms.cs
namespace Xam.DispatchScheduler
{
    public class PlatformImplementation : IPlatformImplementation
    {
        Handler s_handler;
        Handler getHandler()
        {
            if (s_handler == null || s_handler.Looper != Looper.MainLooper)
            {
                s_handler = new Handler(Looper.MainLooper);
            }
            return s_handler;
        }

        public void BeginInvokeOnMainThread(Action action) =>
            getHandler()
                .Post(action);


        public IDisposable StartTimer(TimeSpan interval, Action callback)
        {
            var handler = getHandler();
            object token = new object();

            handler.PostDelayed(() =>
            {
                var t = Interlocked.Exchange(ref token, null);
                if (t != null)
                {
                    callback();
                }
            },
            (long)interval.TotalMilliseconds);

            return
                 Disposable.Create(() =>
                 {
                     Interlocked.Exchange(ref token, null);
                 });
        }


        public IDisposable StartInterval(TimeSpan interval, Action callback)
        {
            SerialDisposable disposable = new SerialDisposable();
            StartInterval(interval, callback, disposable, new object());
            return disposable;
        }

        public bool OnMainThread()
        {
            return Looper.MyLooper() == Looper.MainLooper;
        }


        void StartInterval(
            TimeSpan interval, 
            Action callback, 
            SerialDisposable disposable,
            object token)
        {
            getHandler().PostDelayed(() =>
            {
                var t = Interlocked.Exchange(ref token, null);
                if (t != null)
                {
                    callback();
                    StartInterval(interval, callback, disposable, t);
                }

            }, (long)interval.TotalMilliseconds);

            disposable.Disposable =
                 Disposable.Create(() =>
                 {
                     Interlocked.Exchange(ref token, null);
                 });
        }

    }
}
