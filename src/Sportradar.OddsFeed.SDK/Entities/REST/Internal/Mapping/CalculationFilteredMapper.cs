// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="FilteredCalculationResponseType" /> instances to <see cref="FilteredCalculationDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{FilteredCalculationDto}" />
    internal class CalculationFilteredMapper : ISingleTypeMapper<FilteredCalculationDto>
    {
        /// <summary>
        /// A <see cref="FilteredCalculationResponseType"/> containing sport event data
        /// </summary>
        private readonly FilteredCalculationResponseType _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="FilteredCalculationResponseType"/> containing available selections</param>
        internal CalculationFilteredMapper(FilteredCalculationResponseType data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="FilteredCalculationDto" />
        /// </summary>
        /// <returns>The created <see cref="FilteredCalculationDto" /> instance</returns>
        public FilteredCalculationDto Map()
        {
            return new FilteredCalculationDto(_data);
        }
    }
}
