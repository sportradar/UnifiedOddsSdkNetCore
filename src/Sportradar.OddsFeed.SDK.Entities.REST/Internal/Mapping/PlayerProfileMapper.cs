/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}"/> implementation used to map <see cref="playerProfileEndpoint"/> instances to <see cref="PlayerProfileDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{PlayerProfileDTO}" />
    internal class PlayerProfileMapper : ISingleTypeMapper<PlayerProfileDTO>
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
            Contract.Requires(data != null);

            _data = data;
        }

        /// <summary>
        /// Maps it's data to <see cref="PlayerProfileDTO"/> instance
        /// </summary>
        /// <returns>The created <see cref="PlayerProfileDTO"/> instance </returns>
        public PlayerProfileDTO Map()
        {
            return new PlayerProfileDTO(_data.player, _data.generated_atSpecified ? _data.generated_at : (DateTime?) null);
        }
    }
}