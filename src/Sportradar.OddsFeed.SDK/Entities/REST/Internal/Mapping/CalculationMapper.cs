/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;
using System;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="CalculationResponseType" /> instances to <see cref="CalculationDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{CalculationDto}" />
    internal class CalculationMapper : ISingleTypeMapper<CalculationDto>
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
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="CalculationDto" />
        /// </summary>
        /// <returns>The created <see cref="CalculationDto" /> instance</returns>
        public CalculationDto Map()
        {
            return new CalculationDto(_data);
        }
    }
}
