/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> implementation used to construct <see cref="EntityList{SportDTO}" /> instances
    /// from <see cref="sportsEndpoint"/> instances
    /// </summary>
    public class SportsMapper : ISingleTypeMapper<EntityList<SportDTO>>
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
            Guard.Argument(data).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="EntityList{SportDTO}"/>
        /// </summary>
        /// <returns>The created <see cref="EntityList{SportDTO}"/> instance</returns>
        public EntityList<SportDTO> Map()
        {
            if (_data.sport == null || !_data.sport.Any())
            {
                throw new InvalidOperationException("The provided sportsEndpoint instance contains no sports");
            }

            var sports = _data.sport.Select(x => new SportDTO(x.id, x.name, (IEnumerable<tournamentExtended>) null));
            return new EntityList<SportDTO>(sports);
        }

        /// <summary>
        /// Constructs and returns a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="sportsEndpoint"/> instances
        /// to <see cref="EntityList{SportDTO}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="sportsEndpoint"/> instance containing tournaments data</param>
        /// <returns>a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="sportsEndpoint"/> instances
        /// to <see cref="EntityList{SportDTO}"/> instances</returns>
        internal static ISingleTypeMapper<EntityList<SportDTO>> Create(sportsEndpoint data)
        {
            return new SportsMapper(data);
        }
    }
}