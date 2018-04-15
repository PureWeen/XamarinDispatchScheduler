
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Threading;

namespace Xam.Reactive.Concurrency
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        DispatcherScheduler _dispatchScheduler;

        public XamarinDispatcherScheduler(DispatcherScheduler dispatchScheduler) =>
            _dispatchScheduler = dispatchScheduler;

        public XamarinDispatcherScheduler(Dispatcher dispatcher) =>
        _dispatchScheduler = new DispatcherScheduler(dispatcher);

        public bool HasThreadAccess =>
            _dispatchScheduler.Dispatcher.CheckAccess();


        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) =>
            _dispatchScheduler.Schedule(state, dueTime, action);
        

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action) =>
            _dispatchScheduler.SchedulePeriodic(state, period, action);   


        #region singleton

        static Lazy<XamarinDispatcherScheduler> _scheduler;
        static IScheduler _customScheduler;

        static XamarinDispatcherScheduler()
        {
            _scheduler = new Lazy<XamarinDispatcherScheduler>(() =>
            {
                return new XamarinDispatcherScheduler(System.Windows.Application.Current.Dispatcher);
            });
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

            return
                _scheduler
                    .Value
                    .HasThreadAccess;
        }

        #endregion

    }
}