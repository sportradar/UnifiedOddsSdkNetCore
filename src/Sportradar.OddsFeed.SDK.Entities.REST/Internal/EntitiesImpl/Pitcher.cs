/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides information about pitcher
    /// </summary>
    /// <seealso cref="IPitcher" />
    internal class Pitcher : EntityPrinter, IPitcher
    {
        /// <summary>
        /// Gets a <see cref="URN"/> used to uniquely identify the current <see cref="IPitcher"/> instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets the name of the pitcher represented by the current <see cref="IPitcher"/> instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the hand with which player pitches
        /// </summary>
        /// <value>The hand with which player pitches</value>
        public PlayerHand Hand { get; }

        /// <summary>
        /// Gets the indicator if the competitor is Home or Away
        /// </summary>
        /// <value>The indicator if the competitor is Home or Away</value>
        public HomeAway Competitor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pitcher"/> class
        /// </summary>
        /// <param name="cacheItem">A <see cref="PitcherCI"/> used to create new instance</param>
        internal Pitcher(PitcherCI cacheItem)
        {
            Id = cacheItem.Id;
            Name = cacheItem.Name;
            Hand = cacheItem.Hand;
            Competitor = cacheItem.Competitor;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"PitchId={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"PitchId={Id}, Name={Name}, Hand={Hand}, Competitor={Competitor}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
