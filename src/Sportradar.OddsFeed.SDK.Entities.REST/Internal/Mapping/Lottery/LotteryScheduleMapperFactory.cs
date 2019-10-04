/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{LotteryDTO}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class LotteryScheduleMapperFactory : ISingleTypeMapperFactory<lottery_schedule, LotteryDTO>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{LotteryDTO}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{LotteryDTO}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{LotteryDTO}" /> instance</returns>
        public ISingleTypeMapper<LotteryDTO> CreateMapper(lottery_schedule data)
        {
            return new LotteryScheduleMapper(data);
        }
    }
}
