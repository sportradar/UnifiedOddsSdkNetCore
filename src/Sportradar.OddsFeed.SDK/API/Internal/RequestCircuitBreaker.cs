// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    internal sealed class RequestCircuitBreaker : IRequestCircuitBreaker
    {
        private readonly TimeProvider _timeProvider;
        private long _openUntilTicks;

        public RequestCircuitBreaker(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public bool IsOpen()
        {
            return _openUntilTicks > _timeProvider.GetUtcNow().UtcDateTime.Ticks;
        }

        public void Open(TimeSpan duration)
        {
            var until = _timeProvider.GetUtcNow().UtcDateTime.Add(duration).Ticks;
            var current = _openUntilTicks;
            if (until > current)
            {
                _openUntilTicks = until;
            }
        }

        public void Close()
        {
            _openUntilTicks = 0L;
        }
    }
}
