/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing TV channels
    /// </summary>
    public interface ITvChannel
    {
        /// <summary>
        /// Gets a name of the channel represented by the current <see cref="ITvChannel"/> instance
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying when the coverage on the channel represented by the
        /// current <see cref="ITvChannel"/> starts, or a null reference if the time is not known.
        /// </summary>
        DateTime? StartTime { get; }
    }
}
