using System;

namespace Xamarin.DispatchScheduler
{
    public interface IPlatformImplementation
    {
        void BeginInvokeOnMainThread(Action action);
        bool OnMainThread();
        IDisposable StartInterval(TimeSpan interval, Action callback);
        IDisposable StartTimer(TimeSpan interval, Action callback);
    }
}