
using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Windows.Foundation;
using System.Runtime.ExceptionServices;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Xam.DispatchScheduler
{
    public sealed class XamarinDispatcherScheduler : LocalScheduler, ISchedulerPeriodic
    {
        CoreDispatcherScheduler _dispatchScheduler;

        public XamarinDispatcherScheduler(CoreDispatcherScheduler dispatchScheduler) =>
            _dispatchScheduler = dispatchScheduler;

        public XamarinDispatcherScheduler(CoreDispatcher dispatcher) =>
            _dispatchScheduler = new CoreDispatcherScheduler(dispatcher);
        

        public bool HasThreadAccess => 
            _dispatchScheduler.Dispatcher.HasThreadAccess;


        public override IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) =>
            ScheduleOnMainThread(() => _dispatchScheduler.Schedule(state, dueTime, action));
        

        public IDisposable SchedulePeriodic<TState>(TState state, TimeSpan period, Func<TState, TState> action) =>
            ScheduleOnMainThread(() => _dispatchScheduler.SchedulePeriodic(state, period, action));   

        /// <summary>
        /// For UWP things have to be scheduled from the main thread otherwise you'll get a cross thread exception
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        IDisposable ScheduleOnMainThread(Func<IDisposable> func)
        {
            if(HasThreadAccess)
            {
                return func();
            }

            var d = new MultipleAssignmentDisposable();
            d.Disposable = _dispatchScheduler.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                d.Disposable = func();
            })
           .ToObservable()
           .Subscribe();

            return d;
        }


        #region singleton

        static Lazy<XamarinDispatcherScheduler> _scheduler;
        static XamarinDispatcherScheduler()
        {
            _scheduler = new Lazy<XamarinDispatcherScheduler>(() => new XamarinDispatcherScheduler(CoreApplication.MainView.CoreWindow.Dispatcher));
        }
        public static IScheduler Current
        {
            get
            {
                return _scheduler.Value;
            }
        }

        public static bool OnMainThread()
        {
            return
                _scheduler
                    .Value
                    .HasThreadAccess;
        }
        #endregion

    }
}