/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Provides information about pitcher (cache item)
    /// </summary>
    public class PitcherCI : SportEntityCI
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
        /// Initializes a new instance of the <see cref="PitcherCI"/> class
        /// </summary>
        /// <param name="pitcher">A <see cref="PitcherDTO"/> containing information about the pitcher</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the pitcher</param>
        internal PitcherCI(PitcherDTO pitcher, CultureInfo culture)
            : base(pitcher)
        {
            Guard.Argument(pitcher, nameof(pitcher)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            Merge(pitcher, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitcherCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportablePitcherCI"/> containing information about the pitcher</param>
        internal PitcherCI(ExportablePitcherCI exportable)
            : base(URN.Parse(exportable.Id))
        {
            Name = exportable.Name;
            Hand = exportable.Hand;
            Competitor = exportable.Competitor;
        }

        /// <summary>
        /// Merges the provided information about the current pitcher
        /// </summary>
        /// <param name="pitcher">A <see cref="PitcherDTO"/> containing pitcher info</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the pitcher info</param>
        internal void Merge(PitcherDTO pitcher, CultureInfo culture)
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
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public Task<ExportablePitcherCI> ExportAsync()
        {
            return Task.FromResult(new ExportablePitcherCI
            {
                Id = Id.ToString(),
                Name = Name,
                Hand = Hand,
                Competitor = Competitor
            });
        }
    }
}
