/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="AvailableSelectionsType" /> instances to <see cref="AvailableSelectionsDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{AvailableSelectionsDTO}" />
    internal class AvailableSelectionsMapper : ISingleTypeMapper<AvailableSelectionsDTO>
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
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="AvailableSelectionsDTO" />
        /// </summary>
        /// <returns>The created <see cref="AvailableSelectionsDTO" /> instance</returns>
        public AvailableSelectionsDTO Map()
        {
            return new AvailableSelectionsDTO(_data);
        }
    }
}
