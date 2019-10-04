/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Timer = System.Threading.Timer;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// A timer used for invocation of period tasks
    /// </summary>
    public class SdkTimer : ITimer
    {
        /// <summary>
        /// Internally used <see cref="Timer"/> instance
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Value specifying the time between tha start of the timer and the first firing of the <see cref="Elapsed"/> event
        /// </summary>
        private TimeSpan _dueTime;

        /// <summary>
        /// Value specifying the time window between the first and subsequent firing of the <see cref="Elapsed"/> event
        /// </summary>
        private TimeSpan _period;

        /// <summary>
        /// Raised when the timer elapses
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// Value indicating whether the current instance was/is already disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdkTimer"/> class.
        /// </summary>
        /// <param name="dueTime">A <see cref="TimeSpan"/> specifying a time period before the <see cref="Elapsed"/> event will be raised for the first time.</param>
        /// <param name="period">A <see cref="TimeSpan"/> specifying a period between subsequent raises of the <see cref="Elapsed"/> event.</param>
        public SdkTimer(TimeSpan dueTime, TimeSpan period)
        {
//            Contract.Requires(dueTime >= TimeSpan.Zero);
//            Contract.Requires(period > TimeSpan.Zero);
            Contract.Ensures(_timer != null);
            // Create the timer which is stopped - pass -1 for dueTime and period

            _dueTime = dueTime;
            _period = period;
            _timer = new Timer(OnTick, null, -1, -1);
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_timer != null);
            Contract.Invariant(_dueTime >= TimeSpan.Zero);
            Contract.Invariant(_period > TimeSpan.Zero);
        }

        /// <summary>
        /// Disposes un-managed resources associated with the current instance
        /// </summary>
        ~SdkTimer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Invoked when the internally used <see cref="Timer"/> elapses
        /// </summary>
        /// <param name="state">A <see cref="object"/> instance passed to the timer when it was constructed</param>
        private void OnTick(object state)
        {
            var handler = Elapsed;
            handler?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Disposes resources associated with the current instance
        /// </summary>
        /// <param name="disposing">Value indicating whether the managed resources should also be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _timer.Dispose();
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Starts (or restarts if already running) the current instance so it will start raising the <see cref="Elapsed"/> event.
        /// The dueTime and period are specified by values passed to the constructor or by those passed
        /// to the last call of <see cref="Start(TimeSpan, TimeSpan)"/> method
        /// </summary>
        public void Start()
        {
            _timer.Change(_dueTime, _period);
        }

        /// <summary>
        /// Starts (or restarts if already running) the current instance so it will start raising the <see cref="Elapsed"/> event.
        /// Note that the <code>dueTime</code> and <code>period</code> arguments will override those passed to the
        /// constructor and any subsequent calls to <see cref="Start()"/> will use new values
        /// </summary>
        /// <param name="dueTime">A <see cref="TimeSpan"/> specifying a time period before the <see cref="Elapsed"/> event will be raised for the first time.</param>
        /// <param name="period">A <see cref="TimeSpan"/> specifying a period between subsequent raises of the <see cref="Elapsed"/> event.</param>
        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            _dueTime = dueTime;
            _period = period;

            _timer.Change(_dueTime, _period);
        }

        /// <summary>
        /// Starts (or restarts if already running) the current instance so the <see cref="Elapsed"/> event will be raised once.
        /// Note the <paramref name="dueTime"/> will not override values used by the <see cref="Start()"/> method
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        public void FireOnce(TimeSpan dueTime)
        {
            _timer.Change(dueTime, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Stops the current instance (if running) so it will no longer raise the <see cref="Elapsed"/> event. If the
        /// current instance was already stopped the call has no effect.
        /// </summary>
        public void Stop()
        {
            _timer.Change(-1, -1);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
