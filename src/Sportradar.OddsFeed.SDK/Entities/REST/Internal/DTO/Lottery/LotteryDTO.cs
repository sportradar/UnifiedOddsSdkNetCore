// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for lottery
    /// </summary>
    internal class LotteryDto : SportEventSummaryDto
    {
        /// <summary>
        /// Gets the sport
        /// </summary>
        /// <value>The sport</value>
        public SportDto Sport { get; }

        /// <summary>
        /// Gets the category
        /// </summary>
        /// <value>The category</value>
        public CategorySummaryDto Category { get; }

        /// <summary>
        /// Gets the bonus information
        /// </summary>
        /// <value>The bonus information</value>
        public BonusInfoDto BonusInfo { get; }

        /// <summary>
        /// Gets the draw information
        /// </summary>
        /// <value>The draw information</value>
        public DrawInfoDto DrawInfo { get; }

        /// <summary>
        /// Gets the draw events
        /// </summary>
        /// <value>The draw events</value>
        public IEnumerable<DrawDto> DrawEvents { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        internal LotteryDto(lottery item)
            : base(new sportEvent
            {
                id = item == null ? "wns:lottery:1" : item.id,
                name = item?.name,
                scheduledSpecified = false,
                scheduled = DateTime.MinValue,
                tournament = item?.sport == null
                                            ? null
                                            : new tournament
                                            {
                                                sport = item.sport
                                            }
            })
        {
            if (item == null)
            {
                return;
            }

            if (item.sport != null)
            {
                Sport = new SportDto(item.sport.id, item.sport.name, (IEnumerable<tournamentExtended>)null);
            }
            if (item.category != null)
            {
                Category = new CategorySummaryDto(item.category);
            }
            if (item.bonus_info != null)
            {
                BonusInfo = new BonusInfoDto(item.bonus_info);
            }
            if (item.draw_info != null)
            {
                DrawInfo = new DrawInfoDto(item.draw_info);
            }
        }

        internal LotteryDto(lottery_schedule item)
            : this(item.lottery)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            if (item.draw_events != null && item.draw_events.Any())
            {
                DrawEvents = item.draw_events.Select(draw => new DrawDto(draw)).ToList();
            }

            GeneratedAt = item.generated_atSpecified ? item.generated_at : (DateTime?)null;
        }
    }
}
