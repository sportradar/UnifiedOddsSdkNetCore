// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for bonus info
    /// </summary>
    internal class BonusInfoDto
    {
        /// <summary>
        /// Gets the bonus balls
        /// </summary>
        /// <value>The bonus balls</value>
        public int? BonusBalls { get; }

        /// <summary>
        /// Gets the type of the bonus drum
        /// </summary>
        /// <value>The type of the bonus drum</value>
        public BonusDrumType? BonusDrumType { get; }

        /// <summary>
        /// Gets the bonus range
        /// </summary>
        /// <value>The bonus range</value>
        public string BonusRange { get; }

        internal BonusInfoDto(lotteryBonus_info info)
        {
            Guard.Argument(info, nameof(info)).NotNull();

            BonusBalls = info.bonus_ballsSpecified
                             ? info.bonus_balls
                             : (int?)null;

            BonusDrumType = RestMapperHelper.MapBonusDrumType(info.bonus_drum, info.bonus_drumSpecified);

            BonusRange = info.bonus_range;
        }
    }
}
