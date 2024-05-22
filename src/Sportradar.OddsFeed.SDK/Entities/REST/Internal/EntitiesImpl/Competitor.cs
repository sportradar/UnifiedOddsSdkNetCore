// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly Urn _competitorId;
        private CompetitorCacheItem _competitorCacheItem;
        private readonly IProfileCache _profileCache;
        private readonly IReadOnlyCollection<CultureInfo> _cultures;
        private readonly ISportEntityFactory _sportEntityFactory;
        private readonly ExceptionHandlingStrategy _exceptionStrategy;
        private ReferenceIdCacheItem _referenceId;
        private readonly CompetitionCacheItem _competitionCacheItem;
        private readonly object _lock = new object();
        protected string TeamQualifier;
        private DateTime _lastCompetitorFetch;
        private bool? _isVirtual;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's country name in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Countries => new ReadOnlyDictionary<CultureInfo, string>(
            _cultures.Where(c => GetOrLoadCompetitor().GetCountry(c) != null).ToDictionary(c => c, GetOrLoadCompetitor().GetCountry));

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's abbreviations in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Abbreviations => new ReadOnlyDictionary<CultureInfo, string>(
            _cultures.Where(c => GetOrLoadCompetitor().GetAbbreviation(c) != null).ToDictionary(c => c, GetOrLoadCompetitor().GetAbbreviation));

        /// <summary>
        /// Gets a value indicating whether the current instance represents a placeholder team
        /// </summary>
        public bool IsVirtual
        {
            get
            {
                if (_isVirtual == null)
                {
                    FetchEventCompetitorsVirtual();
                }

                return _isVirtual ?? false;
            }
        }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
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
        /// Gets the name for specific locale
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

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode => GetOrLoadCompetitor().CountryCode;

        /// <summary>
        /// Gets the competitor's abbreviation in the specified language or a null reference
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

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IEnumerable<IPlayer> AssociatedPlayers
        {
            get
            {
                try
                {
                    var associatedPlayerIds = GetOrLoadCompetitor()?.AssociatedPlayerIds?.ToList();
                    if (!associatedPlayerIds.IsNullOrEmpty())
                    {
                        return _sportEntityFactory.BuildPlayersAsync(associatedPlayerIds, _cultures, _exceptionStrategy).GetAwaiter().GetResult();
                    }
                }
                catch (Exception e)
                {
                    SdkLoggerFactory.GetLoggerForExecution(typeof(Competitor)).LogError(e, "Getting Competitor associated players");
                }

                return new List<IPlayer>();
            }
        }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        public IEnumerable<IJersey> Jerseys
        {
            get
            {
                if (GetOrLoadCompetitor().Jerseys != null && GetOrLoadCompetitor().Jerseys.Any())
                {
                    return GetOrLoadCompetitor().Jerseys.Select(s => new Jersey(s));
                }

                return new List<IJersey>();
            }
        }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public IManager Manager => GetOrLoadCompetitor().Manager != null ? new Manager(GetOrLoadCompetitor().Manager) : null;

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public IVenue Venue => GetOrLoadCompetitor().Venue != null ? new Venue(GetOrLoadCompetitor().Venue, _cultures) : null;

        //private IReadOnlyDictionary<CultureInfo, string> _names;

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

        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        public string Gender => GetOrLoadCompetitor()?.Gender;

        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public IRaceDriverProfile RaceDriverProfile
        {
            get
            {
                var raceDriverProfileCacheItem = GetOrLoadCompetitor()?.RaceDriverProfile;
                return raceDriverProfileCacheItem == null ? null : new RaceDriverProfile(raceDriverProfileCacheItem);
            }
        }

        /// <summary>
        /// Gets the age group
        /// </summary>
        /// <value>The age group</value>
        public string AgeGroup => GetOrLoadCompetitor()?.AgeGroup;

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <value>The state</value>
        public string State => GetOrLoadCompetitor()?.State;

        /// <summary>
        /// Gets associated sport
        /// </summary>
        /// <returns>The associated sport</returns>
        public async Task<ISport> GetSportAsync()
        {
            var sportId = GetOrLoadCompetitor()?.SportId;
            if (sportId != null)
            {
                return await _sportEntityFactory.BuildSportAsync(sportId, _cultures, _exceptionStrategy).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// Gets associated category
        /// </summary>
        /// <returns>The associated category</returns>
        public async Task<ICategorySummary> GetCategoryAsync()
        {
            var categoryId = GetOrLoadCompetitor()?.CategoryId;
            if (categoryId != null)
            {
                return await _sportEntityFactory.BuildCategoryAsync(categoryId, _cultures).ConfigureAwait(false);
            }

            return null;
        }

        /// <summary>
        /// Gets the short name
        /// </summary>
        /// <value>The short name</value>
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
        /// Initializes a new instance of the <see cref="Competitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayerProfile"/></param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> used in sport entity factory</param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
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
        /// Initializes a new instance of the <see cref="Competitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayerProfile"/></param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> used in sport entity factory</param>
        /// <param name="competitorsReferences">A list of <see cref="ReferenceIdCacheItem"/> for all competitors</param>
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
        /// Initializes a new instance of the <see cref="Competitor"/> class
        /// </summary>
        /// <param name="competitorId">A competitor id used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayerProfile"/></param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> used in sport entity factory</param>
        /// <param name="competitorsReferences">A list of <see cref="ReferenceIdCacheItem"/> for all competitors</param>
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

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance</returns>
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
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance</returns>
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

        protected void FetchEventCompetitorsReferenceIds()
        {
            GetOrLoadCompetitor();
            lock (_lock)
            {
                if (_competitionCacheItem != null || _competitorCacheItem != null)
                {
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

        protected void FetchEventCompetitorsVirtual()
        {
            lock (_lock)
            {
                if (_competitionCacheItem == null)
                {
                    return;
                }

                var competitorsVirtual = _competitionCacheItem.GetCompetitorsVirtualAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                _isVirtual = !competitorsVirtual.IsNullOrEmpty() && competitorsVirtual.Contains(_competitorCacheItem.Id);
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
