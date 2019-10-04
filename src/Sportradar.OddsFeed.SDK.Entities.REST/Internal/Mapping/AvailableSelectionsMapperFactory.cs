/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances used to map <see cref="AvailableSelectionsType"/> instances to
    /// <see cref="AvailableSelectionsDTO"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    public class AvailableSelectionsMapperFactory : ISingleTypeMapperFactory<AvailableSelectionsType, AvailableSelectionsDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance used to map <see cref="AvailableSelectionsType"/> instances to
        /// <see cref="AvailableSelectionsDTO"/> instances
        /// </summary>
        /// <param name="data">A <see cref="AvailableSelectionsType" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>The constructed <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<AvailableSelectionsDTO> CreateMapper(AvailableSelectionsType data)
        {
            return new AvailableSelectionsMapper(data);
        }
    }
}