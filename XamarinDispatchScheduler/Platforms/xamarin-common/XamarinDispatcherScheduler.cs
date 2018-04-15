
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables; 

namespace Xam.Reactive.Concurrency
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        static Lazy<XamarinDispatcherScheduler> _scheduler;
        static IScheduler _customScheduler;

        static XamarinDispatcherScheduler()
        {
            _scheduler = new Lazy<XamarinDispatcherScheduler>(() => new XamarinDispatcherScheduler(new PlatformImplementation()));
        }

        public IPlatformImplementation _platform;

        public XamarinDispatcherScheduler(IPlatformImplementation platform)
        {
            _platform = platform;
        }

        public static IScheduler Current
        {
            get
            {
                return _customScheduler ?? _scheduler.Value;
            }
            set
            {
                _customScheduler = value;
            }
        }

        public static bool OnMainThread()
        {
            if (_customScheduler != null) return false;

            return _scheduler.Value.Platform.OnMainThread();
        }


        public IPlatformImplementation Platform => _platform;

        public override IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var d = new SingleAssignmentDisposable();
            _scheduler.Value.Platform.BeginInvokeOnMainThread(() =>
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
                _scheduler.Value.Platform.StartTimer(dueTime,
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
            return _scheduler.Value.Platform.StartInterval(period,
            () =>
            {
                state1 = action(state1);
            });
        }
    }
}