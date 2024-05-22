// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a player
    /// </summary>
    /// <seealso cref="SportEntityDto" />
    internal class PlayerDto : SportEntityDto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerDto"/> class
        /// </summary>
        /// <param name="record">A <see cref="player"/> containing information about a player</param>
        internal PlayerDto(player record)
            : base(record.id, record.name)
        {
            Guard.Argument(record, nameof(record)).NotNull();
        }
    }
}
