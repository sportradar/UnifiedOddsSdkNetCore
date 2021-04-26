/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl.CustomBet;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// The run-time implementation of the <see cref="ICustomBetSelectionBuilder"/> interface
    /// </summary>
    internal class CustomBetSelectionBuilder : ICustomBetSelectionBuilder
    {
        private URN _eventId;
        private int _marketId;
        private string _outcomeId;
        private string _specifiers;

        public ICustomBetSelectionBuilder SetEventId(URN eventId)
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

        public ISelection Build(URN eventId, int marketId, string specifiers, string outcomeId)
        {
            _eventId = eventId;
            _marketId = marketId;
            _outcomeId = outcomeId;
            _specifiers = specifiers;
            return Build();
        }
    }
}
