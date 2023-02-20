/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing players or racers in a sport event timeline events
    /// </summary>
    public interface IEventPlayer : IEntityPrinter
    {
        /// <summary>
        /// Gets the <see cref="URN"/> uniquely identifying the current <see cref="IPlayer" /> instance
        /// </summary>
        URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing player names in different languages
        /// </summary>
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name of the player in the specified language or a null reference
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language or a null reference.</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the bench value
        /// </summary>
        /// <value>The bench value - in case of yellow or red card event, it is relevant to know if the player who is getting the card is sitting on the bench at that exact moment.</value>
        /// <remarks>The attribute is equal to 1 if the player who gets the card is sitting on the bench. In case the player who gets the card is on the field, then the attribute is not added at all.</remarks>
        string Bench { get; }
    }
}
