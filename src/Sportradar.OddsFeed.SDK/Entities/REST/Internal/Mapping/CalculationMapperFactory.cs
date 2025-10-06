// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances used to map <see cref="CalculationResponseType"/> instances to
    /// <see cref="CalculationDto"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class CalculationMapperFactory : ISingleTypeMapperFactory<CalculationResponseType, CalculationDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="CalculationResponseType"/> instances to
        /// <see cref="CalculationDto"/> instances
        /// </summary>
        /// <param name="data">A <see cref="CalculationResponseType" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>The constructed <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<CalculationDto> CreateMapper(CalculationResponseType data)
        {
            return new CalculationMapper(data);
        }
    }
}
