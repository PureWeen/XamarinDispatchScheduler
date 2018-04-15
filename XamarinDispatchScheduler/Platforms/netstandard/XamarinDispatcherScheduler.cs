
using System;
using System.Reactive.Concurrency;

namespace Xam.Reactive.Concurrency
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        static IScheduler _scheduler;
        public static IScheduler Current
        {
            get
            {
                return _scheduler ?? throw new Exception("Please install nuget into platform project");
            }
            set
            {
                _scheduler = value;  
            }
        }

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            _ = _scheduler ?? throw new Exception("Please install nuget into platform project");
            return _scheduler.Schedule(state, dueTime, action);
        }

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            _ = _scheduler ?? throw new Exception("Please install nuget into platform project");
            return _scheduler.SchedulePeriodic(state, period, action);
        }

        public static bool OnMainThread()
        {
            _ = _scheduler ?? throw new Exception("Please install nuget into platform project");
            return false;
        }
    }
}