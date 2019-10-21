/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a player
    /// </summary>
    /// <seealso cref="SportEntityDTO" />
    public class PlayerDTO : SportEntityDTO
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="player"/> containing information about a player</param>
        internal PlayerDTO(player record)
            :base(record.id, record.name)
        {
            Guard.Argument(record).NotNull();
        }
    }
}
