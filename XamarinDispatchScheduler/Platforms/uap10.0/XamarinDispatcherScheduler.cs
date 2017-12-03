
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading;
using Windows.UI.Core;

namespace Xamarin.DispatchScheduler
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        static XamarinDispatcherScheduler _scheduler;
        CoreDispatcherScheduler _dispatchScheduler;


        public static IScheduler Current
        {
            get
            {
                if (_scheduler == null)
                {
                    Init();
                }

                return _scheduler;
            }
        }

        public XamarinDispatcherScheduler(CoreDispatcherScheduler dispatchScheduler)
        {
            _dispatchScheduler = dispatchScheduler;
        }

        public XamarinDispatcherScheduler(CoreDispatcher dispatcher)
        {
            _dispatchScheduler = new CoreDispatcherScheduler(dispatcher);
        }


        public static void Init()
        {
            try
            {
                _scheduler = _scheduler ?? new XamarinDispatcherScheduler(CoreDispatcherScheduler.Current);
            }
            catch (InvalidOperationException exc)
            {
                throw new InvalidOperationException("First use of scheduler on UWP has to be off main thread so the dispatch scheduler can initialize", exc);
            }
        }

        public static void Init(CoreDispatcherScheduler coreDispatcherScheduler)
        {
            _scheduler = new XamarinDispatcherScheduler(coreDispatcherScheduler);
        }

        public static void Init(CoreDispatcher coreDispatcher)
        {
            _scheduler = new XamarinDispatcherScheduler(new CoreDispatcherScheduler(coreDispatcher));
        }

        public static bool OnMainThread()
        {
            return 
                _scheduler
                    ._dispatchScheduler
                    .Dispatcher
                    .HasThreadAccess;
        }



        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return _dispatchScheduler.Schedule(state, dueTime, action);
        }

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            return _dispatchScheduler.SchedulePeriodic(state, period, action);
        } 
    }
}