using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Xam.DispatchScheduler
{
    //https://github.com/xamarin/Xamarin.Forms/blob/d3d59ee4f0b3098457e1debe8d7b03d0d0061a53/Xamarin.Forms.Platform.iOS/Forms.cs
    public class PlatformImplementation : IPlatformImplementation
    {
        public IDisposable StartInterval(TimeSpan interval, Action callback)
        {
            NSTimer timer = NSTimer.CreateRepeatingTimer(interval, t =>
            {
                callback();
            });

            NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
            return Disposable.Create(() =>
            {
                timer.Invalidate();
                timer.Dispose();
            });
        }

        public IDisposable StartTimer(TimeSpan interval, Action callback)
        {
            NSTimer timer = NSTimer.CreateTimer(interval, t =>
            {
                if (t.IsValid)
                {
                    callback();
                }
            });

            NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
            return Disposable.Create(() =>
            {
                timer.Invalidate();
                timer.Dispose();
            });
        }

        public void BeginInvokeOnMainThread(Action action)
        {
            NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
        }

        public bool OnMainThread()
        {
            return NSThread.Current.IsMainThread;
        }
    }
}
