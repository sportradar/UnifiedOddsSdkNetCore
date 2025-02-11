// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet
{
    /// <summary>
    /// Implements methods used to provides an requested selection
    /// </summary>
    internal class Selection : ISelection
    {
        public Urn EventId { get; }

        public int MarketId { get; }

        public string OutcomeId { get; }

        public string Specifiers { get; }

        public double? Odds { get; }

        internal Selection(Urn eventId, int marketId, string outcomeId, string specifiers, double? odds = null)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            MarketId = marketId > 0 ? marketId : throw new ArgumentException("Missing marketId", nameof(marketId));
            OutcomeId = outcomeId ?? throw new ArgumentNullException(nameof(outcomeId));
            Specifiers = specifiers;
            Odds = odds;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var specifiers = Specifiers.IsNullOrEmpty() ? string.Empty : $",Specifiers={Specifiers}";
            return $"Event={EventId},Market={MarketId},Outcome={OutcomeId}{specifiers},Odds={Odds}";
        }
    }
}
