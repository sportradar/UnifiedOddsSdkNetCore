/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to execute period tasks
    /// </summary>
    public interface ITimer : IDisposable {
        /// <summary>
        /// Raised when the timer elapses
        /// </summary>
        event EventHandler Elapsed;

        /// <summary>
        /// Starts (or restarts, if already running) the current instance so it will start raising the <see cref="Elapsed"/> event.
        /// The dueTime and period are specified by values passed to the constructor or by those passed
        /// to the last call of <see cref="ITimer.Start(TimeSpan,TimeSpan)"/> method
        /// </summary>
        void Start();

        /// <summary>
        /// Starts (or restarts, if already running) the current instance so it will start raising the <see cref="Elapsed"/> event.
        /// Note that the <code>dueTime</code> and <code>period</code> arguments will override those passed to the
        /// constructor and any subsequent calls to <see cref="Start()"/> will use the new values
        /// </summary>
        /// <param name="dueTime">A <see cref="TimeSpan"/> specifying a time period before the <see cref="Elapsed"/> event will be raised for the first time.</param>
        /// <param name="period">A <see cref="TimeSpan"/> specifying a period between subsequent raises of the <see cref="Elapsed"/> event.</param>
        void Start(TimeSpan dueTime, TimeSpan period);

        /// <summary>
        /// Starts (or restarts, if already running) the current instance so the <see cref="Elapsed"/> event will be raised once.
        /// Note the <code>dueTime</code> will not override values used by the <see cref="Start()"/> method
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        void FireOnce(TimeSpan dueTime);

        /// <summary>
        /// Stops the current instance (if running) so it will no longer raise the <see cref="Elapsed"/> event. If the
        /// current instance was already stopped the call has no effect.
        /// </summary>
        void Stop();
    }
}