// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery
{
    /// <summary>
    /// A factory used to create <see cref="ISingleTypeMapper{LotteryDto}"/> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapperFactory{TOut,TIn}" />
    internal class LotteryScheduleMapperFactory : ISingleTypeMapperFactory<lottery_schedule, LotteryDto>
    {
        /// <summary>
        /// Creates and returns a <see cref="ISingleTypeMapper{LotteryDto}" /> instance
        /// </summary>
        /// <param name="data">A <see cref="RestMessage" /> instance which the created <see cref="ISingleTypeMapper{LotteryDto}" /> will map</param>
        /// <returns>New <see cref="ISingleTypeMapper{LotteryDto}" /> instance</returns>
        public ISingleTypeMapper<LotteryDto> CreateMapper(lottery_schedule data)
        {
            return new LotteryScheduleMapper(data);
        }
    }
}
