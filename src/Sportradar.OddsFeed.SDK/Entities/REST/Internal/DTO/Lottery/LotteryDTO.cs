/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for lottery
    /// </summary>
    internal class LotteryDTO : SportEventSummaryDTO
    {
        /// <summary>
        /// Gets the sport
        /// </summary>
        /// <value>The sport</value>
        public SportDTO Sport { get; }

        /// <summary>
        /// Gets the category
        /// </summary>
        /// <value>The category</value>
        public CategorySummaryDTO Category { get; }

        /// <summary>
        /// Gets the bonus information
        /// </summary>
        /// <value>The bonus information</value>
        public BonusInfoDTO BonusInfo { get; }

        /// <summary>
        /// Gets the draw information
        /// </summary>
        /// <value>The draw information</value>
        public DrawInfoDTO DrawInfo { get; }

        /// <summary>
        /// Gets the draw events
        /// </summary>
        /// <value>The draw events</value>
        public IEnumerable<DrawDTO> DrawEvents { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        internal LotteryDTO(lottery item)
            : base(new sportEvent
            {
                id = item == null ? "wns:lottery:1" : item.id,
                name = item.name,
                scheduledSpecified = false,
                scheduled = DateTime.MinValue,
                tournament = item.sport == null
                    ? null
                    : new tournament
                    {
                        sport = item.sport
                    }
            })
        {
            Guard.Argument(item, nameof(item)).NotNull();

            if (item.sport!=null)
            {
                Sport = new SportDTO(item.sport.id, item.sport.name, (IEnumerable<tournamentExtended>) null);
            }
            if (item.category != null)
            {
                Category = new CategorySummaryDTO(item.category);
            }
            if (item.bonus_info != null)
            {
                BonusInfo = new BonusInfoDTO(item.bonus_info);
            }
            if (item.draw_info != null)
            {
                DrawInfo = new DrawInfoDTO(item.draw_info);
            }
        }

        internal LotteryDTO(lottery_schedule item)
            : this(item.lottery)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            if (item.draw_events != null && item.draw_events.Any())
            {
                DrawEvents = item.draw_events.Select(draw => new DrawDTO(draw)).ToList();
            }

            GeneratedAt = item.generated_atSpecified ? item.generated_at : (DateTime?) null;
        }
    }
}
