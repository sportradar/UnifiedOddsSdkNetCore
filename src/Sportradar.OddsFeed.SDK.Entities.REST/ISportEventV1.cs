/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing the target(tournament, match, race) of feed messages
    /// </summary>
    public interface ISportEventV1 : ISportEvent
    {
        /// <summary>
        /// Asynchronously gets a <see cref="Nullable{bool}"/> specifying if the start time to be determined is set for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="Nullable{bool}"/> specifying if the start time to be determined is set for the associated sport event.</returns>
        Task<bool?> GetStartTimeTbdAsync();

        /// <summary>
        /// Asynchronously gets a <see cref="URN"/> specifying the replacement sport event for the associated sport event.
        /// </summary>
        /// <returns>A <see cref="URN"/> specifying the replacement sport event for the associated sport event.</returns>
        Task<URN> GetReplacedByAsync();
    }
}
