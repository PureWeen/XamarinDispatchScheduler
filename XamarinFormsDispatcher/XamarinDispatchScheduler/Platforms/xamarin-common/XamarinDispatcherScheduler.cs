
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
         

        public static IScheduler Current
        {
            get
            {
                return _scheduler.Value;
            }
        }


        /// <summary>
        /// Schedules an action to be executed on the dispatcher.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
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

        /// <summary>
        /// Schedules an action to be executed after <paramref name="dueTime"/> on the dispatcher, using a <see cref="Windows.UI.Xaml.DispatcherTimer"/> object.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">State passed to the action to be executed.</param>
        /// <param name="action">Action to be executed.</param>
        /// <param name="dueTime">Relative time after which to execute the action.</param>
        /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
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
            SingleAssignmentDisposable disposable = new SingleAssignmentDisposable();
            IDisposable timer = null;

            timer =
                PlatformScheduler.StartTimer(dueTime,
                    () =>
                    {
                        if (!disposable.IsDisposed)
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

            return new CompositeDisposable(
                d,
                timer,
                Disposable.Create(() =>
                {
                    action = (_, __) => Disposable.Empty;
                })
            );
        }

        /// <summary>
        /// Schedules a periodic piece of work on the dispatcher, using a <see cref="Windows.UI.Xaml.DispatcherTimer"/> object.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
        /// <param name="state">Initial state passed to the action upon the first iteration.</param>
        /// <param name="period">Period for running the work periodically.</param>
        /// <param name="action">Action to be executed, potentially updating the state.</param>
        /// <returns>The disposable object used to cancel the scheduled recurring action (best effort).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> is less than <see cref="TimeSpan.Zero"/>.</exception>
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