/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="AvailableSelectionsType" /> instances to <see cref="AvailableSelectionsDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{AvailableSelectionsDTO}" />
    internal class AvailableSelectionsMapper : ISingleTypeMapper<AvailableSelectionsDto>
    {
        /// <summary>
        /// A <see cref="AvailableSelectionsType"/> containing sport event data
        /// </summary>
        private readonly AvailableSelectionsType _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableSelectionsMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="AvailableSelectionsType"/> containing available selections</param>
        internal AvailableSelectionsMapper(AvailableSelectionsType data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="AvailableSelectionsDto" />
        /// </summary>
        /// <returns>The created <see cref="AvailableSelectionsDto" /> instance</returns>
        public AvailableSelectionsDto Map()
        {
            return new AvailableSelectionsDto(_data);
        }
    }
}
