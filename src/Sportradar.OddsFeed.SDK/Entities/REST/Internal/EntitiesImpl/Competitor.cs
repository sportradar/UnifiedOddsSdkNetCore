// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a player or a team competing in a sport event
    /// </summary>
    /// <seealso cref="Player" />
    /// <seealso cref="ICompetitor" />
    [DataContract]
    internal class Competitor : Player, ICompetitor
    {
        private readonly CompetitionCacheItem _competitionCacheItem;
        private readonly Urn _competitorId;
        private readonly IReadOnlyCollection<CultureInfo> _cultures;
        private readonly ExceptionHandlingStrategy _exceptionStrategy;
        private readonly object _lock = new object();
        private readonly IProfileCache _profileCache;
        private readonly ISportEntityFactory _sportEntityFactory;
        private CompetitorCacheItem _competitorCacheItem;
        private DateTime _lastCompetitorFetch;
        private ReferenceIdCacheItem _referenceId;
        protected string TeamQualifier;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Competitor" /> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCacheItem" /> used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache" /> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem" /></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory" /> used to retrieve <see cref="IPlayerProfile" />
        /// </param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy" /> used in sport entity factory</param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem" /> to which this competitor belongs to</param>
        [SuppressMessage("CodeQuality", "IDE0058:Expression value is never used", Justification = "Allowed for Guard statements")]
        public Competitor(CompetitorCacheItem ci,
            IProfileCache profileCache,
            IReadOnlyCollection<CultureInfo> cultures,
            ISportEntityFactory sportEntityFactory,
            ExceptionHandlingStrategy exceptionStrategy,
            ICompetitionCacheItem rootCompetitionCacheItem)
            : base(ci.Id, new Dictionary<CultureInfo, string>())
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull();
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();

            _competitorId = ci.Id;
            _competitorCacheItem = ci;
            _profileCache = profileCache;
            _cultures = cultures;
            _sportEntityFactory = sportEntityFactory;
            _exceptionStrategy = exceptionStrategy;

            if (rootCompetitionCacheItem is CompetitionCacheItem competitionCi)
            {
                _competitionCacheItem = competitionCi;
            }

            _referenceId = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Competitor" /> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCacheItem" /> used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache" /> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem" /></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory" /> used to retrieve <see cref="IPlayerProfile" />
        /// </param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy" /> used in sport entity factory</param>
        /// <param name="competitorsReferences">A list of <see cref="ReferenceIdCacheItem" /> for all competitors</param>
        [SuppressMessage("CodeQuality", "IDE0058:Expression value is never used", Justification = "Allowed for Guard statements")]
        public Competitor(CompetitorCacheItem ci,
            IProfileCache profileCache,
            IReadOnlyCollection<CultureInfo> cultures,
            ISportEntityFactory sportEntityFactory,
            ExceptionHandlingStrategy exceptionStrategy,
            IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences)
            : base(ci.Id, new Dictionary<CultureInfo, string>())
        {
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();

            _competitorId = ci.Id;
            _competitorCacheItem = ci;
            _profileCache = profileCache;
            _cultures = cultures;
            _sportEntityFactory = sportEntityFactory;
            _exceptionStrategy = exceptionStrategy;
            _competitionCacheItem = null;
            _referenceId = null;

            if (competitorsReferences != null && competitorsReferences.Any())
            {
                if (competitorsReferences.TryGetValue(ci.Id, out var q))
                {
                    _referenceId = q;
                }
            }
            else
            {
                if (ci.ReferenceId != null)
                {
                    _referenceId = ci.ReferenceId;
                }
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Competitor" /> class
        /// </summary>
        /// <param name="competitorId">A competitor id used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache" /> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem" /></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory" /> used to retrieve <see cref="IPlayerProfile" />
        /// </param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy" /> used in sport entity factory</param>
        /// <param name="competitorsReferences">A list of <see cref="ReferenceIdCacheItem" /> for all competitors</param>
        [SuppressMessage("CodeQuality", "IDE0058:Expression value is never used", Justification = "Allowed for Guard statements")]
        public Competitor(Urn competitorId,
            IProfileCache profileCache,
            IReadOnlyCollection<CultureInfo> cultures,
            ISportEntityFactory sportEntityFactory,
            ExceptionHandlingStrategy exceptionStrategy,
            IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences)
            : base(competitorId, new Dictionary<CultureInfo, string>())
        {
            Guard.Argument(competitorId, nameof(competitorId)).NotNull();
            Guard.Argument(profileCache, nameof(profileCache)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull();
            Guard.Argument(sportEntityFactory, nameof(sportEntityFactory)).NotNull();

            _competitorId = competitorId;
            _competitorCacheItem = null;
            _profileCache = profileCache;
            _cultures = cultures;
            _sportEntityFactory = sportEntityFactory;
            _exceptionStrategy = exceptionStrategy;
            _competitionCacheItem = null;
            _referenceId = null;

            if (competitorsReferences != null && competitorsReferences.Any() && competitorsReferences.TryGetValue(competitorId, out var q))
            {
                _referenceId = q;
            }
        }

        public IReadOnlyDictionary<CultureInfo, string> Countries => new ReadOnlyDictionary<CultureInfo, string>(_cultures.Where(c => GetOrLoadCompetitor().GetCountry(c) != null).ToDictionary(c => c, GetOrLoadCompetitor().GetCountry));

        public IReadOnlyDictionary<CultureInfo, string> Abbreviations => new ReadOnlyDictionary<CultureInfo, string>(_cultures.Where(c => GetOrLoadCompetitor().GetAbbreviation(c) != null).ToDictionary(c => c, c => GetOrLoadCompetitor().GetAbbreviation(c)));

        public bool IsVirtual => GetOrLoadCompetitor()?.IsVirtual ?? false;

        public IReference References
        {
            get
            {
                if (_referenceId == null)
                {
                    FetchEventCompetitorsReferenceIds();
                }

                return _referenceId != null
                           ? new Reference(_referenceId)
                           : null;
            }
        }

        /// <summary>
        ///     Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Country if exists, or null</returns>
        public string GetCountry(CultureInfo culture)
        {
            // no need to call GetOrLoadCompetitor() since already called before populating dictionary
            return Countries.ContainsKey(culture)
                       ? Countries[culture]
                       : null;
        }

        public string CountryCode => GetOrLoadCompetitor().CountryCode;

        /// <summary>
        ///     Gets the competitor's abbreviation in the specified language or a null reference
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the abbreviation</param>
        /// <returns>The competitor's abbreviation in the specified language or a null reference</returns>
        public string GetAbbreviation(CultureInfo culture)
        {
            // no need to call GetOrLoadCompetitor() since already called before populating dictionary
            return Abbreviations.ContainsKey(culture)
                       ? Abbreviations[culture]
                       : null;
        }

        public IEnumerable<IPlayer> AssociatedPlayers
        {
            get
            {
                try
                {
                    var associatedPlayerIds = GetOrLoadCompetitor()?.AssociatedPlayerIds?.ToList();
                    var associatedPlayersJerseyNumbers = GetOrLoadCompetitor()?.AssociatedPlayersJerseyNumbers;
                    if (!associatedPlayerIds.IsNullOrEmpty())
                    {
                        return _sportEntityFactory.BuildPlayersAsync(associatedPlayerIds, _cultures, _exceptionStrategy, associatedPlayersJerseyNumbers).GetAwaiter().GetResult();
                    }
                }
                catch (Exception e)
                {
                    SdkLoggerFactory.GetLoggerForExecution(typeof(Competitor)).LogError(e, "Error getting Competitor associated players");
                }

                return new List<IPlayer>();
            }
        }

        public IEnumerable<IJersey> Jerseys => GetOrLoadCompetitor().Jerseys != null && GetOrLoadCompetitor().Jerseys.Any()
            ? (IEnumerable<IJersey>)GetOrLoadCompetitor().Jerseys.Select(s => new Jersey(s))
            : new List<IJersey>();

        public IManager Manager => GetOrLoadCompetitor().Manager != null ? new Manager(GetOrLoadCompetitor().Manager) : null;

        public IVenue Venue => GetOrLoadCompetitor().Venue != null ? new Venue(GetOrLoadCompetitor().Venue, _cultures) : null;

        public override IReadOnlyDictionary<CultureInfo, string> Names
        {
            get
            {
                var names = _profileCache.GetCompetitorNamesAsync(Id, _cultures, true).GetAwaiter().GetResult();
                return names;
            }
        }

        public override string GetName(CultureInfo culture)
        {
            return _profileCache.GetCompetitorNameAsync(Id, culture, true).GetAwaiter().GetResult();
        }

        public string Gender => GetOrLoadCompetitor()?.Gender;

        public IRaceDriverProfile RaceDriverProfile
        {
            get
            {
                var raceDriverProfileCacheItem = GetOrLoadCompetitor()?.RaceDriverProfile;
                return raceDriverProfileCacheItem == null ? null : new RaceDriverProfile(raceDriverProfileCacheItem);
            }
        }

        public string AgeGroup => GetOrLoadCompetitor()?.AgeGroup;

        public string State => GetOrLoadCompetitor()?.State;

        public async Task<ISport> GetSportAsync()
        {
            var sportId = GetOrLoadCompetitor()?.SportId;
            return sportId != null
                ? await _sportEntityFactory.BuildSportAsync(sportId, _cultures, _exceptionStrategy).ConfigureAwait(false)
                : null;

        }

        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var categoryId = GetOrLoadCompetitor()?.CategoryId;
            return categoryId != null
                ? await _sportEntityFactory.BuildCategoryAsync(categoryId, _cultures).ConfigureAwait(false)
                : null;

        }

        public string ShortName => GetOrLoadCompetitor()?.ShortName;

        public IDivision Division
        {
            get
            {
                var divisionCacheItem = GetOrLoadCompetitor()?.Division;
                return divisionCacheItem == null ? null : new Division(divisionCacheItem);
            }
        }

        /// <summary>
        ///     Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintC()
        {
            var abbreviations = string.Join(", ", Abbreviations.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            var associatedPlayersStr = string.Empty;
            var associatedPlayerIds = _competitorCacheItem?.GetAssociatedPlayerIds()?.ToList();
            if (!associatedPlayerIds.IsNullOrEmpty())
            {
                associatedPlayersStr = string.Join(", ", associatedPlayerIds);
                associatedPlayersStr = $", AssociatedPlayers=[{associatedPlayersStr}]";
            }

            var reference = string.Empty;
            if (_referenceId != null && !_referenceId.ReferenceIds.IsNullOrEmpty())
            {
                var stringBuilder = new StringBuilder();
                foreach (var referenceIdReferenceId in _referenceId.ReferenceIds)
                {
                    stringBuilder.Append(", ").Append($"{referenceIdReferenceId.Key}={referenceIdReferenceId.Value}");
                }

                reference = stringBuilder.ToString().Substring(2);
            }

            return $"{base.PrintC()}, Gender={Gender}, Reference={reference}, Abbreviations=[{abbreviations}]{associatedPlayersStr}";
        }

        /// <summary>
        ///     Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            var countryNames = string.Join(", ", Countries.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            var abbreviations = string.Join(", ", Abbreviations.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            var associatedPlayers = string.Empty;
            if (AssociatedPlayers != null && AssociatedPlayers.Any())
            {
                associatedPlayers = string.Join(", ", AssociatedPlayers.Select(s => s.ToString("f")));
                associatedPlayers = $", AssociatedPlayers=[{associatedPlayers}]";
            }

            var division = Division == null ? string.Empty : $", Division=[{Division.Id}-{Division.Name}]";
            var reference = References == null
                                ? string.Empty
                                : References.ToString("f");
            return $"{base.PrintF()}, Countries=[{countryNames}], Reference={reference}, Abbreviations=[{abbreviations}], IsVirtual={IsVirtual}{associatedPlayers}{division}";
        }

        private void FetchEventCompetitorsReferenceIds()
        {
            GetOrLoadCompetitor();
            lock (_lock)
            {
                if (_competitionCacheItem == null && _competitorCacheItem == null)
                {
                    return;
                }
                var competitorsReferences = _competitionCacheItem?.GetCompetitorsReferencesAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                if (competitorsReferences != null && competitorsReferences.Any())
                {
                    if (competitorsReferences.TryGetValue(_competitorCacheItem.Id, out var q))
                    {
                        _referenceId = q;
                    }
                }
                else
                {
                    if (_competitorCacheItem.ReferenceId != null)
                    {
                        _referenceId = _competitorCacheItem.ReferenceId;
                    }
                }
            }
        }

        protected void FetchEventCompetitorsQualifiers()
        {
            lock (_lock)
            {
                var competitorsQualifiers = _competitionCacheItem?.GetCompetitorsQualifiersAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                if (competitorsQualifiers != null && competitorsQualifiers.Any() && competitorsQualifiers.TryGetValue(_competitorCacheItem.Id, out var qualifier))
                {
                    TeamQualifier = qualifier;
                }
            }
        }

        private CompetitorCacheItem GetOrLoadCompetitor()
        {
            if (_competitorId != null && _competitorCacheItem == null && _profileCache != null)
            {
                LoadCompetitorProfileInCache();
                _lastCompetitorFetch = DateTime.Now;
            }

            if (_competitorCacheItem != null && _profileCache != null && _lastCompetitorFetch < DateTime.Now.AddSeconds(-30))
            {
                LoadCompetitorProfileInCache();
                _lastCompetitorFetch = DateTime.Now;
            }

            return _competitorCacheItem;
        }

        private void LoadCompetitorProfileInCache()
        {
            _competitorCacheItem = _profileCache.GetCompetitorProfileAsync(_competitorId, _cultures, true).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
