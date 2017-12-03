
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading;
using XamarinDispatchScheduler;

namespace System.Reactive.Concurrency
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        static Lazy<XamarinDispatcherScheduler> _scheduler;
        static XamarinDispatcherScheduler()
        {
            _scheduler = new Lazy<XamarinDispatcherScheduler>(() => new XamarinDispatcherScheduler());
        }


        //this is really only valid for UWP currently
        public static void Init()
        {
            
        }


        public static XamarinDispatcherScheduler Current
        {
            get
            {
                return _scheduler.Value;
            }
        }

        public static bool OnMainThread()
        {
            return PlatformScheduler.OnMainThread();
        }


        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var d = new SingleAssignmentDisposable();
            PlatformScheduler.BeginInvokeOnMainThread(() =>
            {
                if (!d.IsDisposed)
                {
                    d.Disposable = action(this, state);
                }
            });

            return d;
        }

        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var dt = Scheduler.Normalize(dueTime);
            if (dt.Ticks == 0)
            {
                return Schedule(state, action);
            }

            return ScheduleSlow(state, dt, action);
        }

        private IDisposable ScheduleSlow<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            var d = new MultipleAssignmentDisposable();
            IDisposable timer = null;

            timer =
                PlatformScheduler.StartTimer(dueTime,
                    () =>
                    {
                        if (!d.IsDisposed)
                        {
                            try
                            {
                                d.Disposable = action(this, state);
                            }
                            finally
                            {
                                action = null;
                                timer.Dispose();
                            }
                        }
                    });

            d.Disposable = new CompositeDisposable(
                d,
                timer,
                Disposable.Create(() =>
                {
                    action = (_, __) => Disposable.Empty;
                })
            );

            return d;
        }


        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action)
        {
            if (period < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(period));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var state1 = state;
            return PlatformScheduler.StartInterval(period,
            () =>
            {
                state1 = action(state1);
            });
        }
    }
}