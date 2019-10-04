/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    /// <summary>
    /// A <see cref="ITimeProvider"/> giving access to fake time useful for unit testing
    /// </summary>
    public class FakeTimeProvider : ITimeProvider
    {
        /// <summary>
        /// A <see cref="DateTime"/> served by the current instance
        /// </summary>
        private DateTime _time;

        /// <summary>
        /// An <see cref="object"/> used to ensure thread safety
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTimeProvider"/> class
        /// </summary>
        public FakeTimeProvider()
            : this(DateTime.Now)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTimeProvider"/> class
        /// <param name="time">The <see cref="DateTime"/> to be served by the constructed instance</param>
        /// </summary>
        public FakeTimeProvider(DateTime time)
        {
            _time = time;
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the current time
        /// </summary>
        public DateTime Now
        {
            get
            {
                lock (_lock)
                {
                    return _time;
                }
            }
            set
            {
                lock (_lock)
                {
                    _time = value;
                }
            }
        }

        public void AddSeconds(int seconds)
        {
            Now = Now.AddSeconds(seconds);
        }

        public void AddMilliSeconds(int milliSeconds)
        {
            Now = Now.AddMilliseconds(milliSeconds);
        }
    }
}
