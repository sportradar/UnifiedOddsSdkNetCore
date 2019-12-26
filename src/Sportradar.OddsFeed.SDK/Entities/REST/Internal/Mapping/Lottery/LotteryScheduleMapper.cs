/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="lottery_schedule" /> instances to <see cref="LotteryDTO" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{LotteryDTO}" />
    internal class LotteryScheduleMapper : ISingleTypeMapper<LotteryDTO>
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
            Guard.Argument(lotterySchedule, nameof(lotterySchedule)).NotNull();

            _lotterySchedule = lotterySchedule;
        }

        public LotteryDTO Map()
        {
            return new LotteryDTO(_lotterySchedule);
        }
    }
}
