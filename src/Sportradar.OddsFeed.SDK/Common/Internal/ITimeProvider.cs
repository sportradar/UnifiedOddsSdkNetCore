// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes giving access to the current time
    /// </summary>
    internal interface ITimeProvider
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the current time
        /// </summary>
        DateTime Now { get; }
    }
}
