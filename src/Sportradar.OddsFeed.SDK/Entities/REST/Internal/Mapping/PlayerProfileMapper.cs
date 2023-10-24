/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="playerProfileEndpoint"/> instances to <see cref="PlayerProfileDto"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{PlayerProfileDto}" />
    internal class PlayerProfileMapper : ISingleTypeMapper<PlayerProfileDto>
    {
        /// <summary>
        /// A <see cref="playerProfileEndpoint"/> instance containing player profile data
        /// </summary>
        private readonly playerProfileEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="playerProfileEndpoint"/> instance containing player profile data</param>
        internal PlayerProfileMapper(playerProfileEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="PlayerProfileDto"/> instance
        /// </summary>
        /// <returns>The created <see cref="PlayerProfileDto"/> instance </returns>
        public PlayerProfileDto Map()
        {
            return new PlayerProfileDto(_data.player, _data.generated_atSpecified ? _data.generated_at : (DateTime?)null);
        }
    }
}
