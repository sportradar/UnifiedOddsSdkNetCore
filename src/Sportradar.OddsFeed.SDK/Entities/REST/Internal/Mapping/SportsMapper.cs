/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> implementation used to construct <see cref="EntityList{SportDto}" /> instances
    /// from <see cref="sportsEndpoint"/> instances
    /// </summary>
    internal class SportsMapper : ISingleTypeMapper<EntityList<SportDto>>
    {
        /// <summary>
        /// A <see cref="sportsEndpoint"/> instance containing data about available sports
        /// </summary>
        private readonly sportsEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportsMapper"/> class.
        /// </summary>
        /// <param name="data">The <see cref="sportsEndpoint"/> instance containing data about available sports.</param>
        protected SportsMapper(sportsEndpoint data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="EntityList{SportDto}"/>
        /// </summary>
        /// <returns>The created <see cref="EntityList{SportDto}"/> instance</returns>
        public EntityList<SportDto> Map()
        {
            if (_data.sport == null || !_data.sport.Any())
            {
                throw new InvalidOperationException("The provided sportsEndpoint instance contains no sports");
            }

            var sports = _data.sport.Select(x => new SportDto(x.id, x.name, (IEnumerable<tournamentExtended>)null));
            return new EntityList<SportDto>(sports);
        }

        /// <summary>
        /// Constructs and returns a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="sportsEndpoint"/> instances
        /// to <see cref="EntityList{SportDto}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="sportsEndpoint"/> instance containing tournaments data</param>
        /// <returns>a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="sportsEndpoint"/> instances
        /// to <see cref="EntityList{SportDto}"/> instances</returns>
        internal static ISingleTypeMapper<EntityList<SportDto>> Create(sportsEndpoint data)
        {
            return new SportsMapper(data);
        }
    }
}
