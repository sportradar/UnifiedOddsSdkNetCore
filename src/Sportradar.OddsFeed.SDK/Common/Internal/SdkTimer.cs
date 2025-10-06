// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Timer = System.Threading.Timer;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// A timer used for invocation of period tasks
    /// </summary>
    internal sealed class SdkTimer : ISdkTimer
    {
        public string TimerName { get; }

        /// <summary>
        /// Internally used <see cref="Timer"/> instance
        /// </summary>
        private readonly Timer _timer;

        /// <summary>
        /// Value specifying the time between the start of the timer and the first firing of the <see cref="Elapsed"/> event
        /// </summary>
        public TimeSpan DueTime { get; private set; }

        /// <summary>
        /// Value specifying the time window between the first and subsequent firing of the <see cref="Elapsed"/> event
        /// </summary>
        public TimeSpan Period { get; private set; }

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
        /// <param name="name">The name of the timer instance</param>
        /// <param name="dueTime">A <see cref="TimeSpan"/> specifying a time period before the <see cref="Elapsed"/> event will be raised for the first time.</param>
        /// <param name="period">A <see cref="TimeSpan"/> specifying a period between subsequent raises of the <see cref="Elapsed"/> event.</param>
        public SdkTimer(string name, TimeSpan dueTime, TimeSpan period)
        {
            Guard.Argument(name, nameof(name)).NotNull();
            Guard.Argument(dueTime, nameof(dueTime)).Require(dueTime >= TimeSpan.Zero);
            //Guard.Argument(period, nameof(period)).Require(period > TimeSpan.Zero);
            // Create the timer which is stopped - pass -1 for dueTime and period

            TimerName = name;
            DueTime = dueTime;
            Period = period;
            _timer = new Timer(OnTick, null, -1, -1);
        }

        /// <summary>
        /// Disposes unmanaged resources associated with the current instance
        /// </summary>
        ~SdkTimer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SdkTimer"/> class.
        /// </summary>
        /// <param name="name">The name of the timer instance</param>
        /// <param name="dueTime">A <see cref="TimeSpan"/> specifying a time period before the <see cref="Elapsed"/> event will be raised for the first time.</param>
        /// <param name="period">A <see cref="TimeSpan"/> specifying a period between subsequent raises of the <see cref="Elapsed"/> event.</param>
        /// <returns>New timer instance</returns>
        public static SdkTimer Create(string name, TimeSpan dueTime, TimeSpan period)
        {
            return new SdkTimer(name, dueTime, period);
        }

        /// <summary>
        /// Invoked when the internally used <see cref="Timer"/> elapses
        /// </summary>
        /// <param name="state">A <see cref="object"/> instance passed to the timer when it was constructed</param>
        private void OnTick(object state)
        {
            try
            {
                var handler = Elapsed;
                handler?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                SdkLoggerFactory.GetLoggerForExecution(typeof(SdkTimer)).LogError(e, "Error invoking SdkTimer.OnTick");
            }
        }

        public bool IsDisposed()
        {
            return _isDisposed;
        }

        /// <summary>
        /// Disposes resources associated with the current instance
        /// </summary>
        /// <param name="disposing">Value indicating whether the managed resources should also be disposed</param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (disposing)
            {
                _timer.Dispose();
            }
        }

        /// <summary>
        /// Starts (or restarts if already running) the current instance so it will start raising the <see cref="Elapsed"/> event.
        /// The dueTime and period are specified by values passed to the constructor or by those passed
        /// to the last call of <see cref="Start(TimeSpan, TimeSpan)"/> method
        /// </summary>
        public void Start()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("SdkTimer is disposed");
            }
            _timer.Change(DueTime, Period);
        }

        /// <summary>
        /// Starts (or restarts if already running) the current instance so it will start raising the <see cref="Elapsed"/> event.
        /// Note that the <c>dueTime</c> and <c>period</c> arguments will override those passed to the
        /// constructor and any subsequent calls to <see cref="Start()"/> will use new values
        /// </summary>
        /// <param name="dueTime">A <see cref="TimeSpan"/> specifying a time period before the <see cref="Elapsed"/> event will be raised for the first time.</param>
        /// <param name="period">A <see cref="TimeSpan"/> specifying a period between subsequent raises of the <see cref="Elapsed"/> event.</param>
        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            Guard.Argument(dueTime, nameof(dueTime)).Require(dueTime >= TimeSpan.Zero);
            Guard.Argument(period, nameof(period)).Require(period > TimeSpan.Zero);

            if (_isDisposed)
            {
                throw new ObjectDisposedException("SdkTimer is disposed");
            }

            DueTime = dueTime;
            Period = period;

            _timer?.Change(DueTime, Period);
        }

        /// <summary>
        /// Starts (or restarts if already running) the current instance so the <see cref="Elapsed"/> event will be raised once.
        /// Note the <paramref name="dueTime"/> will not override values used by the <see cref="Start()"/> method
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        public void FireOnce(TimeSpan dueTime)
        {
            Guard.Argument(dueTime, nameof(dueTime)).Require(dueTime >= TimeSpan.Zero);

            _timer?.Change(dueTime, TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Stops the current instance (if running) so it will no longer raise the <see cref="Elapsed"/> event. If the
        /// current instance was already stopped the call has no effect.
        /// </summary>
        public void Stop()
        {
            if (!_isDisposed)
            {
                _timer?.Change(-1, -1);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
