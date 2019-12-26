/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Enums;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a score of a sport event period
    /// </summary>
    /// <seealso cref="IPeriodScore" />
    internal class PeriodScore : EntityPrinter, IPeriodScore
    {
        /// <summary>
        /// The match statuses cache
        /// </summary>
        private readonly ILocalizedNamedValueCache _matchStatusesCache;

        /// <summary>
        /// The <see cref="HomeScore"/> property backing field
        /// </summary>
        private readonly decimal _homeScore;

        /// <summary>
        /// The <see cref="AwayScore"/> property backing field
        /// </summary>
        private readonly decimal _awayScore;

        /// <summary>
        /// The <see cref="Type"/> property backing field
        /// </summary>
        private readonly PeriodType? _type;

        /// <summary>
        /// The <see cref="Number"/> property backing field
        /// </summary>
        private readonly int? _number;

        private readonly int? _matchStatusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodScore"/> class.
        /// </summary>
        /// <param name="dto">The data-transfer-object for period score</param>
        /// <param name="matchStatusesCache">The match statuses cache</param>
        public PeriodScore(PeriodScoreDTO dto, ILocalizedNamedValueCache matchStatusesCache)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            _homeScore = dto.HomeScore;
            _awayScore = dto.AwayScore;
            _type = dto.Type;
            _number = dto.PeriodNumber;
            _matchStatusCode = dto.MatchStatusCode;
            _matchStatusesCache = matchStatusesCache;
        }

        /// <summary>
        /// Gets the score of the home team in the period represented by the current <see cref="IPeriodScore" /> instance
        /// </summary>
        public decimal HomeScore => _homeScore;

        /// <summary>
        /// Gets the score of the away team in the period represented by the current <see cref="IPeriodScore" /> instance
        /// </summary>
        public decimal AwayScore => _awayScore;

        /// <summary>
        /// Gets the type value of the current <see cref="IPeriodScore"/> instance
        /// </summary>
        public PeriodType? Type => _type;

        /// <summary>
        /// Gets the sequence number of the period represented by the current <see cref="IPeriodScore" /> instance
        /// </summary>
        public int? Number => _number;

        /// <summary>
        /// Gets the match status code
        /// </summary>
        /// <value>The match status code</value>
        public int? MatchStatusCode => _matchStatusCode;

        /// <summary>
        /// Asynchronously gets the match status
        /// </summary>
        /// <param name="culture">The culture used to get match status id and description</param>
        /// <returns>Returns the match status id and description in selected culture</returns>
        public async Task<ILocalizedNamedValue> GetMatchStatusAsync(CultureInfo culture)
        {
            return _matchStatusCode == null || _matchStatusesCache == null
                ? null
                : await _matchStatusesCache.GetAsync(_matchStatusCode.Value, new List<CultureInfo> { culture }).ConfigureAwait(false);
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"HomeScore={_homeScore}, AwayScore={_awayScore}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"HomeScore={_homeScore}, AwayScore={_awayScore}, Type={_type}, Number={_number}, MatchStatusCode={_matchStatusCode}";
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
