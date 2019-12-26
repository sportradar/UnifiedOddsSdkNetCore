/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{T}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class LotteriesMapperFactory : ISingleTypeMapperFactory<lotteries, EntityList<LotteryDTO>>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{T}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{T}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{T}" /> instance</returns>
        public ISingleTypeMapper<EntityList<LotteryDTO>> CreateMapper(lotteries data)
        {
            return new LotteriesMapper(data);
        }
    }
}
