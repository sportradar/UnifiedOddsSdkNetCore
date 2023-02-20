/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes representing goal scorer in a sport event
    /// </summary>
    public interface IGoalScorer : IEntityPrinter
    {
        /// <summary>
        /// Gets the <see cref="URN"/> uniquely identifying the current <see cref="IPlayer" /> instance
        /// </summary>
        [DataMember]
        URN Id { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing player names in different languages
        /// </summary>
        [DataMember]
        IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name of the player in the specified language or a null reference
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language or a null reference.</returns>
        string GetName(CultureInfo culture);

        /// <summary>
        /// Gets the method value
        /// </summary>
        /// <value>The method value</value>
        /// <remarks>The attribute can assume values such as 'penalty' and 'own goal'. In case the attribute is not inserted, then the goal is not own goal neither penalty.</remarks>
        string Method => null;
    }
}
