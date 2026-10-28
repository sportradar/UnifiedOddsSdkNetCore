// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    internal sealed class CircuitTimeTracker : ICircuitTimeTracker
    {
        private int _failedRequestsCount;

        private readonly TimeSpan _shortOpenDuration;
        private readonly TimeSpan _longOpenDuration;
        private readonly int _longThreshold;

        public CircuitTimeTracker(
            TimeSpan shortOpenDuration,
            TimeSpan longOpenDuration,
            int longThreshold)
        {
            _shortOpenDuration = shortOpenDuration;
            _longOpenDuration = longOpenDuration;
            _longThreshold = longThreshold;
        }

        public int IncrementFailedRequestsCount()
        {
            return ++_failedRequestsCount;
        }

        public void ResetFailedRequestsCount()
        {
            _failedRequestsCount = 0;
        }

        public TimeSpan NextOpenDuration()
        {
            var opens = _failedRequestsCount;
            return opens >= _longThreshold ? _longOpenDuration : _shortOpenDuration;
        }
    }
}
