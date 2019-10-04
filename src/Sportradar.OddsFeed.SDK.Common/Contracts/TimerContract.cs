/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Contracts
{
    [ContractClassFor(typeof(ITimer))]
    internal abstract class TimerContract : ITimer
    {
        public void Dispose()
        {
        }

        public event EventHandler Elapsed;

        public void Start()
        {
        }

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            Contract.Requires(dueTime >= TimeSpan.Zero);
            Contract.Requires(period > TimeSpan.Zero);
        }

        public void FireOnce(TimeSpan dueTime)
        {
            Contract.Requires(dueTime >= TimeSpan.Zero);
        }

        public void Stop()
        {
        }
    }
}

