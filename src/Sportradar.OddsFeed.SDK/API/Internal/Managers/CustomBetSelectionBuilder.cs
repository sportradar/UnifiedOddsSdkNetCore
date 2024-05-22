// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl.CustomBet;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// The run-time implementation of the <see cref="ICustomBetSelectionBuilder"/> interface
    /// </summary>
    internal class CustomBetSelectionBuilder : ICustomBetSelectionBuilder
    {
        private Urn _eventId;
        private int _marketId;
        private string _outcomeId;
        private string _specifiers;

        public ICustomBetSelectionBuilder SetEventId(Urn eventId)
        {
            _eventId = eventId;
            return this;
        }

        public ICustomBetSelectionBuilder SetMarketId(int marketId)
        {
            _marketId = marketId;
            return this;
        }

        public ICustomBetSelectionBuilder SetOutcomeId(string outcomeId)
        {
            _outcomeId = outcomeId;
            return this;
        }

        public ICustomBetSelectionBuilder SetSpecifiers(string specifiers)
        {
            _specifiers = specifiers;
            return this;
        }

        public ISelection Build()
        {
            var selection = new Selection(_eventId, _marketId, _outcomeId, _specifiers);
            _eventId = null;
            _marketId = 0;
            _outcomeId = null;
            _specifiers = null;
            return selection;
        }

        public ISelection Build(Urn eventId, int marketId, string specifiers, string outcomeId)
        {
            _eventId = eventId;
            _marketId = marketId;
            _outcomeId = outcomeId;
            _specifiers = specifiers;
            return Build();
        }
    }
}
