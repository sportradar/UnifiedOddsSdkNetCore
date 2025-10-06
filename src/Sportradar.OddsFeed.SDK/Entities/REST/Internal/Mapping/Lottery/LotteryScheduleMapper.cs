// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="lottery_schedule" /> instances to <see cref="LotteryDto" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{LotteryDto}" />
    internal class LotteryScheduleMapper : ISingleTypeMapper<LotteryDto>
    {
        /// <summary>
        /// A <see cref="lottery_schedule"/> containing rest data
        /// </summary>
        private readonly lottery_schedule _lotterySchedule;

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteryScheduleMapper"/> class
        /// </summary>
        /// <param name="lotterySchedule">A <see cref="lottery"/> containing lottery schedule data (single lottery with schedule)</param>
        internal LotteryScheduleMapper(lottery_schedule lotterySchedule)
        {
            _lotterySchedule = lotterySchedule;
        }

        public LotteryDto Map()
        {
            if (_lotterySchedule == null)
            {
                return null;
            }
            return new LotteryDto(_lotterySchedule);
        }
    }
}
