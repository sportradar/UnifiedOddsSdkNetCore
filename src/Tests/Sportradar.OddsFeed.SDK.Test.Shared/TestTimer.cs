/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    public class TestTimer : ITimer
    {
        private readonly bool _raiseEventOnStart;

        public event EventHandler Elapsed;

        public TestTimer(bool raiseEventOnStart)
        {
            _raiseEventOnStart = raiseEventOnStart;
        }

        public void Dispose()
        {

        }

        public void Start()
        {
            if (!_raiseEventOnStart)
            {
                return;
            }

            var handler = Elapsed;
            handler?.Invoke(this, new EventArgs());
        }

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            if (!_raiseEventOnStart)
            {
                return;
            }

            var handler = Elapsed;
            handler?.Invoke(this, new EventArgs());
        }

        public void FireOnce(TimeSpan dueTime)
        {
            var handler = Elapsed;
            handler?.Invoke(this, new EventArgs());
        }

        public void Stop()
        {

        }
    }
}
