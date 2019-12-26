/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// A <see cref="ITimeProvider"/> giving access to the current time
    /// </summary>
    public class RealTimeProvider : ITimeProvider
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the current time
        /// </summary>
        public DateTime Now => DateTime.Now;
    }
}