/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide access to named values (e.g bet stop reasons, void reasons, ...)
    /// </summary>
    internal interface INamedValuesProvider
    {
        /// <summary>
        /// Gets a <see cref="INamedValueCache"/> providing void reason descriptions
        /// </summary>
        INamedValueCache VoidReasons { get; }

        /// <summary>
        /// Gets a <see cref="INamedValueCache"/> providing bet stop reason descriptions
        /// </summary>
        INamedValueCache BetStopReasons { get; }

        /// <summary>
        /// Gets a <see cref="INamedValueCache"/> providing betting status descriptions
        /// </summary>
        INamedValueCache BettingStatuses { get; }

        /// <summary>
        /// Gets a <see cref="ILocalizedNamedValueCache"/> providing localized (translatable) match status descriptions
        /// </summary>
        ILocalizedNamedValueCache MatchStatuses { get; }
    }
}