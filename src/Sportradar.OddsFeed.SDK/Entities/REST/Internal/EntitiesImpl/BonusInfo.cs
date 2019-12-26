/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class BonusInfo
    /// </summary>
    /// <seealso cref="EntityPrinter" />
    /// <seealso cref="IBonusInfo" />
    internal class BonusInfo : EntityPrinter, IBonusInfo
    {
        /// <summary>
        /// Gets the bonus balls info
        /// </summary>
        /// <value>The bonus balls info or null if not known</value>
        public int? BonusBalls { get; }
        /// <summary>
        /// Gets the type of the bonus drum
        /// </summary>
        /// <value>The type of the bonus drum or null if not known</value>
        public BonusDrumType? BonusDrumType { get; }
        /// <summary>
        /// Gets the bonus range
        /// </summary>
        /// <value>The bonus range</value>
        public string BonusRange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BonusInfo"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public BonusInfo(BonusInfoCI item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            BonusBalls = item.BonusBalls;
            BonusDrumType = item.BonusDrumType;
            BonusRange = item.BonusRange;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"BonusDrumType={BonusDrumType}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            return $"BonusDrumType={BonusDrumType}, BonusBalls={BonusBalls}, BonusRange={BonusRange}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
