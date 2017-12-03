
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading; 

namespace System.Reactive.Concurrency
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        public static XamarinDispatcherScheduler Current
        {
            get
            {
                throw new Exception("Please install nuget into platform project");
            }
        }

        public static void Init()
        {
            throw new Exception("Please install nuget into platform project");
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