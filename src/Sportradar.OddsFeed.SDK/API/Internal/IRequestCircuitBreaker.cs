// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    internal interface IRequestCircuitBreaker
    {
        bool IsOpen();
        void Open(TimeSpan duration);
        void Close();
    }
}
