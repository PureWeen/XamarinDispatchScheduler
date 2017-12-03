
using System;
using System.Reactive.Concurrency; 

namespace Xamarin.DispatchScheduler
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        public static IScheduler Current
        {
            get
            {
                throw new Exception("Please install nuget into platform project");
            }
        }

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            throw new Exception("Please install nuget into platform project");
        }

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            throw new Exception("Please install nuget into platform project");
        }

        public static bool OnMainThread()
        {
            throw new Exception("Please install nuget into platform project");
        }
    }
}