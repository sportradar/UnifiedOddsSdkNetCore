/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a player in a sport event timeline event
    /// </summary>
    /// <seealso cref="IPlayer" />
    /// <seealso cref="IEventPlayer" />
    internal class EventPlayer : BaseEntity, IEventPlayer
    {
        /// <summary>
        /// Gets the bench value
        /// </summary>
        /// <value>The bench value - in case of yellow or red card event, it is relevant to know if the player who is getting the card is sitting on the bench at that exact moment.</value>
        /// <remarks>The attribute is equal to 1 if the player who gets the card is sitting on the bench. In case the player who gets the card is on the field, then the attribute is not added at all.</remarks>
        public string Bench { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IEventPlayer"/> class
        /// </summary>
        /// <param name="data">The <see cref="EventPlayerCI"/> data</param>
        public EventPlayer(EventPlayerCI data)
            : base(data.Id, data.Name as IReadOnlyDictionary<CultureInfo, string>)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            Bench = data.Bench;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"{base.PrintC()}, Bench={Bench}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return $"{base.PrintF()}, Bench={Bench}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
