/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="CalculationResponseType" /> instances to <see cref="CalculationDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{CalculationDTO}" />
    internal class CalculationMapper : ISingleTypeMapper<CalculationDTO>
    {
        /// <summary>
        /// A <see cref="CalculationResponseType"/> containing sport event data
        /// </summary>
        private readonly CalculationResponseType _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="CalculationResponseType"/> containing available selections</param>
        internal CalculationMapper(CalculationResponseType data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="CalculationDTO" />
        /// </summary>
        /// <returns>The created <see cref="CalculationDTO" /> instance</returns>
        public CalculationDTO Map()
        {
            return new CalculationDTO(_data);
        }
    }
}
