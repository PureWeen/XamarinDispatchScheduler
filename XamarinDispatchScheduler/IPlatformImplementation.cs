using System;

namespace Xam.Reactive.DispatchScheduler
{
    public interface IPlatformImplementation
    {
        void BeginInvokeOnMainThread(Action action);
        bool OnMainThread();
        IDisposable StartInterval(TimeSpan interval, Action callback);
        IDisposable StartTimer(TimeSpan interval, Action callback);
    }
}