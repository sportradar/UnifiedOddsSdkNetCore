/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a player info within timeline events
    /// </summary>
    /// <seealso cref="SportEntityDTO" />
    internal class EventPlayerDTO : SportEntityDTO
    {
        /// <summary>
        /// Gets the bench value
        /// </summary>
        /// <value>The bench value - in case of yellow or red card event, it is relevant to know if the player who is getting the card is sitting on the bench at that exact moment.</value>
        /// <remarks>The attribute is equal to 1 if the player who gets the card is sitting on the bench. In case the player who gets the card is on the field, then the attribute is not added at all.</remarks>
        public string Bench { get; }

        /// <summary>
        /// Gets the method value
        /// </summary>
        /// <value>The method value</value>
        /// <remarks>The attribute can assume values such as 'penalty' and 'own goal'. In case the attribute is not inserted, then the goal is not own goal neither penalty.</remarks>
        public string Method { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventPlayerDTO"/> class
        /// </summary>
        /// <param name="record">A <see cref="eventPlayer"/> containing information about a player</param>
        internal EventPlayerDTO(eventPlayer record)
            : base(record.id, record.name)
        {
            Guard.Argument(record, nameof(record)).NotNull();

            Bench = record.bench;
            Method = record.method;
        }
    }
}
