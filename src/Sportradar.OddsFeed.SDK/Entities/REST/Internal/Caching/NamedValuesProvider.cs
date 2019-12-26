/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    /// <summary>
    /// A default <see cref="INamedValuesProvider"/> implementation
    /// </summary>
    internal class NamedValuesProvider : INamedValuesProvider
    {
        /// <summary>
        /// Gets a <see cref="INamedValueCache" /> providing void reason descriptions
        /// </summary>
        public INamedValueCache VoidReasons { get; }

        /// <summary>
        /// Gets a <see cref="INamedValueCache" /> providing bet stop reason descriptions
        /// </summary>
        public INamedValueCache BetStopReasons { get; }

        /// <summary>
        /// Gets a <see cref="INamedValueCache" /> providing betting status descriptions
        /// </summary>
        public INamedValueCache BettingStatuses { get; }

        /// <summary>
        /// Gets a <see cref="ILocalizedNamedValueCache" /> providing localized(translatable) match status descriptions
        /// </summary>
        public ILocalizedNamedValueCache MatchStatuses { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValuesProvider"/> class.
        /// </summary>
        /// <param name="voidReasons">The <see cref="INamedValueCache" /> providing void reason descriptions.</param>
        /// <param name="betStopReasons">The <see cref="INamedValueCache" /> providing bet stop reason descriptions.</param>
        /// <param name="bettingStatuses">The <see cref="INamedValueCache" /> providing betting status descriptions.</param>
        /// <param name="matchStatuses">The <see cref="ILocalizedNamedValueCache" /> providing localized(translatable) match status descriptions.</param>
        public NamedValuesProvider(INamedValueCache voidReasons, INamedValueCache betStopReasons, INamedValueCache bettingStatuses, ILocalizedNamedValueCache matchStatuses)
        {
            Guard.Argument(voidReasons, nameof(voidReasons)).NotNull();
            Guard.Argument(betStopReasons, nameof(betStopReasons)).NotNull();
            Guard.Argument(bettingStatuses, nameof(bettingStatuses)).NotNull();
            Guard.Argument(matchStatuses, nameof(matchStatuses)).NotNull();

            VoidReasons = voidReasons;
            BetStopReasons = betStopReasons;
            BettingStatuses = bettingStatuses;
            MatchStatuses = matchStatuses;
        }
    }
}
