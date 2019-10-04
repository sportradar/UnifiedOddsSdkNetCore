/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances used to map <see cref="CalculationResponseType"/> instances to
    /// <see cref="CalculationDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class CalculationMapperFactory : ISingleTypeMapperFactory<CalculationResponseType, CalculationDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="CalculationResponseType"/> instances to
        /// <see cref="CalculationDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="CalculationResponseType" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>The constructed <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<CalculationDTO> CreateMapper(CalculationResponseType data)
        {
            return new CalculationMapper(data);
        }
    }
}