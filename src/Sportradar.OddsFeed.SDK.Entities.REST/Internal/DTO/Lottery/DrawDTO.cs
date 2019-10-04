/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery
{
    /// <summary>
    /// Defines a data-transfer-object for lottery draw
    /// </summary>
    internal class DrawDTO : SportEventSummaryDTO
    {
        /// <summary>
        /// Gets the <see cref="LotteryDTO"/>
        /// </summary>
        public LotteryDTO Lottery { get; }

        /// <summary>
        /// Gets the status of the draw
        /// </summary>
        public DrawStatus Status { get; }

        /// <summary>
        /// Gets a value indicating whether results are in chronological order
        /// </summary>
        /// <value><c>true</c> if [results chronological]; otherwise, <c>false</c>.</value>
        public bool ResultsChronological { get; }

        /// <summary>
        /// Gets the results
        /// </summary>
        /// <value>The results</value>
        public IEnumerable<DrawResultDTO> Results { get; }

        /// <summary>
        /// Gets the display identifier
        /// </summary>
        /// <value>The display identifier</value>
        public int? DisplayId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the associated message was generated (on the server side)
        /// </summary>
        public DateTime? GeneratedAt { get; }

        internal DrawDTO(draw_summary item)
            : base(new sportEvent
            {
                id = item.draw_fixture == null
                         ? "wns:draw:1"
                         : item.draw_fixture.id,
                name = string.Empty,
                scheduledSpecified = item.draw_fixture?.draw_dateSpecified ?? false,
                scheduled = item.draw_fixture?.draw_date ?? DateTime.MinValue,
                tournament = item.draw_fixture?.lottery == null
                    ? null
                    : new tournament
                        {
                            sport =  item.draw_fixture.lottery.sport
                        }
            })
        {
            Contract.Requires(item != null);

            DisplayId = null;

            if (item.draw_fixture != null)
            {
                if (item.draw_fixture.id != null)
                {
                    Lottery = new LotteryDTO(item.draw_fixture.lottery);
                }
                Status = RestMapperHelper.MapDrawStatus(item.draw_fixture.status, item.draw_fixture.statusSpecified);

                DisplayId = item.draw_fixture.display_idSpecified
                                    ? item.draw_fixture.display_id
                                    : (int?) null;
            }

            ResultsChronological = false;

            if (item.draw_result?.draws != null)
            {
                ResultsChronological = item.draw_result.draws.chronologicalSpecified && item.draw_result.draws.chronological;

                if (item.draw_result.draws.draw != null)
                {
                    var res = item.draw_result.draws.draw.Select(draw => new DrawResultDTO(draw)).ToList();
                    Results = res;
                }
            }

            GeneratedAt = item.generated_atSpecified ? item.generated_at : (DateTime?) null;
        }

        internal DrawDTO(draw_fixtures item)
            : base(new sportEvent
            {
                id = item == null ? "wns:draw:1" : item?.draw_fixture?.id,
                name = string.Empty,
                scheduledSpecified = item?.draw_fixture?.draw_dateSpecified ?? false,
                scheduled = item?.draw_fixture?.draw_date ?? DateTime.MinValue,
                tournament = item?.draw_fixture?.lottery == null
                    ? null
                    : new tournament
                    {
                        sport = item.draw_fixture?.lottery.sport
                    }
            })
        {
            Contract.Requires(item?.draw_fixture != null);
            var fixture = item.draw_fixture;

            Debug.Assert(fixture != null, nameof(fixture) + " != null");

            if (fixture.lottery != null)
            {
                Lottery = new LotteryDTO(fixture.lottery);
            }
            Status = RestMapperHelper.MapDrawStatus(fixture.status, fixture.statusSpecified);

            DisplayId = fixture.display_idSpecified
                            ? fixture.display_id
                            : (int?) null;

            GeneratedAt = item.generated_atSpecified ? item.generated_at : (DateTime?) null;
        }

        internal DrawDTO(draw_event item)
            : base(new sportEvent
            {
                id = item == null ? "wns:draw:1" : item.id,
                name = string.Empty,
                scheduledSpecified = item?.scheduledSpecified ?? false,
                scheduled = item?.scheduled ?? DateTime.MinValue
            })
        {
            Contract.Requires(item != null);

            if (item != null)
            {
                Status = RestMapperHelper.MapDrawStatus(item.status, item.statusSpecified);

                DisplayId = item.display_idSpecified
                            ? item.display_id
                            : (int?) null;
            }
        }
    }
}
