// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    internal interface ICircuitTimeTracker
    {
        int IncrementFailedRequestsCount();

        void ResetFailedRequestsCount();

        TimeSpan NextOpenDuration();
    }
}
