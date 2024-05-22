// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Globalization;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about pitcher (cache item)
    /// </summary>
    internal class PitcherCacheItem : SportEntityCacheItem
    {
        /// <summary>
        /// Gets the name of the pitcher
        /// </summary>
        /// <value>The name of the pitcher</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the hand with which player pitches
        /// </summary>
        /// <value>The hand with which player pitches</value>
        public PlayerHand Hand { get; private set; }

        /// <summary>
        /// Gets the indicator if the competitor is Home or Away
        /// </summary>
        /// <value>The indicator if the competitor is Home or Away</value>
        public HomeAway Competitor { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitcherCacheItem"/> class
        /// </summary>
        /// <param name="pitcher">A <see cref="PitcherDto"/> containing information about the pitcher</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the pitcher</param>
        internal PitcherCacheItem(PitcherDto pitcher, CultureInfo culture)
            : base(pitcher)
        {
            Guard.Argument(pitcher, nameof(pitcher)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Merge(pitcher, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitcherCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportablePitcher"/> containing information about the pitcher</param>
        internal PitcherCacheItem(ExportablePitcher exportable)
            : base(Urn.Parse(exportable.Id))
        {
            Name = exportable.Name;
            Hand = exportable.Hand;
            Competitor = exportable.Competitor;
        }

        /// <summary>
        /// Merges the provided information about the current pitcher
        /// </summary>
        /// <param name="pitcher">A <see cref="PitcherDto"/> containing pitcher info</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the pitcher info</param>
        internal void Merge(PitcherDto pitcher, CultureInfo culture)
        {
            Guard.Argument(pitcher, nameof(pitcher)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Name = pitcher.Name;
            Hand = pitcher.Hand;
            Competitor = pitcher.Competitor;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportablePitcher> ExportAsync()
        {
            return Task.FromResult(new ExportablePitcher
            {
                Id = Id.ToString(),
                Name = Name,
                Hand = Hand,
                Competitor = Competitor
            });
        }
    }
}
