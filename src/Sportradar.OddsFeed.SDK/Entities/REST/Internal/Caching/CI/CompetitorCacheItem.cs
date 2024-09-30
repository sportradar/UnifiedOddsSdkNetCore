// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Competitor cache item to be saved within cache
    /// </summary>
    internal class CompetitorCacheItem : SportEntityCacheItem, IExportableBase
    {
        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing competitor names in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Names;

        public Urn AssociatedSportEventId;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor's country name in different languages
        /// </summary>
        private IDictionary<CultureInfo, string> _countryNames;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor abbreviations in different languages
        /// </summary>
        private IDictionary<CultureInfo, string> _abbreviations;

        private List<Urn> _associatedPlayerIds;
        private IDictionary<Urn, int> _associatedPlayerJerseyNumbers;
        private bool? _isVirtual;
        private ReferenceIdCacheItem _referenceId;
        private List<JerseyCacheItem> _jerseys;
        private string _countryCode;
        private ManagerCacheItem _manager;
        private VenueCacheItem _venue;
        private string _gender;
        private string _ageGroup;
        private RaceDriverProfileCacheItem _raceDriverProfile;
        private string _state;
        private Urn _sportId;
        private Urn _categoryId;
        private string _shortName;

        /// <summary>
        /// The list of CultureInfo used to fetch competitor profiles
        /// </summary>
        private IDictionary<CultureInfo, DateTime> CultureCompetitorProfileFetched { get; set; }

        /// <summary>
        /// Gets the name of the competitor in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the competitor in the specified language if it exists, null otherwise</returns>
        public string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (!Names.ContainsKey(culture))
            {
                FetchProfileIfNeeded(culture);
            }

            return Names.TryGetValue(culture, out var name)
                ? name
                : null;
        }

        /// <summary>
        /// Gets the country name of the competitor in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned country name</param>
        /// <returns>The country name of the competitor in the specified language if it exists, null otherwise</returns>
        public string GetCountry(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (!_countryNames.ContainsKey(culture))
            {
                FetchProfileIfNeeded(culture);
            }

            return _countryNames.TryGetValue(culture, out var country)
                ? country
                : null;
        }

        /// <summary>
        /// Gets the abbreviation of the competitor in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned abbreviation</param>
        /// <returns>The abbreviation of the competitor in the specified language if it exists, null otherwise</returns>
        public string GetAbbreviation(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (!_abbreviations.ContainsKey(culture))
            {
                FetchProfileIfNeeded(culture);
            }

            return _abbreviations.TryGetValue(culture, out var abbreviation)
                ? abbreviation
                : null;
        }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode
        {
            get
            {
                if (string.IsNullOrEmpty(_countryCode))
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }

                return _countryCode;
            }
        }

        /// <summary>
        /// Gets a value indicating whether represented competitor is virtual
        /// </summary>
        public bool? IsVirtual
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _isVirtual;
            }
        }

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        public ReferenceIdCacheItem ReferenceId => _referenceId;

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public ICollection<Urn> AssociatedPlayerIds
        {
            get
            {
                if (_associatedPlayerIds.IsNullOrEmpty())
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }
                return _associatedPlayerIds;
            }
        }

        /// <summary>
        /// Get associated player ids if already cached (no competitor profile is fetched)
        /// </summary>
        public ICollection<Urn> GetAssociatedPlayerIds()
        {
            return _associatedPlayerIds;
        }

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IDictionary<Urn, int> AssociatedPlayersJerseyNumbers
        {
            get
            {
                if (_associatedPlayerJerseyNumbers.IsNullOrEmpty())
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }
                return _associatedPlayerJerseyNumbers;
            }
        }

        /// <summary>
        /// Get associated player jersey numbers if already cached (no competitor profile is fetched)
        /// </summary>
        public IDictionary<Urn, int> GetAssociatedPlayersJerseyNumber()
        {
            return _associatedPlayerJerseyNumbers;
        }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        public ICollection<JerseyCacheItem> Jerseys
        {
            get
            {
                if (!_jerseys.Any())
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }
                return _jerseys;
            }
        }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public ManagerCacheItem Manager
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _manager;
            }
        }

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public VenueCacheItem Venue
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _venue;
            }
        }

        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        public string Gender
        {
            get
            {
                if (string.IsNullOrEmpty(_gender))
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }
                return _gender;
            }
        }

        /// <summary>
        /// Gets the age group
        /// </summary>
        /// <value>The age group</value>
        public string AgeGroup
        {
            get
            {
                if (string.IsNullOrEmpty(_ageGroup))
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }
                return _ageGroup;
            }
        }

        /// <summary>
        /// Gets the state
        /// </summary>
        /// <value>The state</value>
        public string State
        {
            get
            {
                if (string.IsNullOrEmpty(_state))
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }

                return _state;
            }
        }

        /// <summary>
        /// Gets associated sport id
        /// </summary>
        /// <value>Sport id</value>
        public Urn SportId
        {
            get
            {
                if (_sportId == null)
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }

                return _sportId;
            }
        }

        /// <summary>
        /// Gets associated category id
        /// </summary>
        /// <value>Category id</value>
        public Urn CategoryId
        {
            get
            {
                if (_categoryId == null)
                {
                    FetchProfileIfNeeded(_primaryCulture);
                }

                return _categoryId;
            }
        }

        /// <summary>
        /// Gets the division
        /// </summary>
        /// <value>The division</value>
        public DivisionCacheItem Division { get; private set; }

        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public RaceDriverProfileCacheItem RaceDriverProfile => _raceDriverProfile;

        /// <summary>
        /// Gets the short name
        /// </summary>
        /// <value>The short name</value>
        public string ShortName => _shortName;

        /// <summary>
        /// Gets the type of the root source
        /// </summary>
        /// <value>The root source</value>
        public string RootSource { get; }

        private readonly IDataRouterManager _dataRouterManager;
        private readonly object _lock = new object();
        private readonly object _lockAdd = new object();
        private CultureInfo _primaryCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCacheItem"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDto"/></param>
        internal CompetitorCacheItem(CompetitorDto competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _primaryCulture = culture;
            CultureCompetitorProfileFetched = new Dictionary<CultureInfo, DateTime>();

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<Urn>();
            _associatedPlayerJerseyNumbers = new Dictionary<Urn, int>();
            _jerseys = new List<JerseyCacheItem>();
            RootSource = "CompetitorDto";
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCacheItem"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorProfileDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorProfileDto"/></param>
        internal CompetitorCacheItem(CompetitorProfileDto competitor, CultureInfo culture, IDataRouterManager dataRouterManager = null)
            : base(competitor.Competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _primaryCulture = culture;
            CultureCompetitorProfileFetched = new Dictionary<CultureInfo, DateTime>();

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<Urn>();
            _associatedPlayerJerseyNumbers = new Dictionary<Urn, int>();
            _jerseys = new List<JerseyCacheItem>();
            RootSource = "CompetitorProfileDto";
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCacheItem"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="SimpleTeamProfileDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="SimpleTeamProfileDto"/></param>
        internal CompetitorCacheItem(SimpleTeamProfileDto competitor, CultureInfo culture, IDataRouterManager dataRouterManager = null)
            : base(competitor.Competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _primaryCulture = culture;
            CultureCompetitorProfileFetched = new Dictionary<CultureInfo, DateTime>();

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<Urn>();
            _associatedPlayerJerseyNumbers = new Dictionary<Urn, int>();
            _jerseys = new List<JerseyCacheItem>();
            RootSource = "SimpleTeamProfileDto";
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCacheItem"/> class
        /// </summary>
        /// <param name="playerCompetitor">A <see cref="PlayerCompetitorDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDto"/></param>
        internal CompetitorCacheItem(PlayerCompetitorDto playerCompetitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(playerCompetitor)
        {
            Guard.Argument(playerCompetitor, nameof(playerCompetitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _primaryCulture = culture;
            CultureCompetitorProfileFetched = new Dictionary<CultureInfo, DateTime>();

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<Urn>();
            _associatedPlayerJerseyNumbers = new Dictionary<Urn, int>();
            _jerseys = new List<JerseyCacheItem>();
            RootSource = "PlayerCompetitorDto";
            Merge(playerCompetitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityCacheItem"/> class
        /// </summary>
        /// <param name="originalCompetitorCacheItem">A <see cref="CompetitorCacheItem"/> containing information about the sport entity</param>
        protected CompetitorCacheItem(CompetitorCacheItem originalCompetitorCacheItem)
            : base(new SportEntityDto(originalCompetitorCacheItem.Id.ToString(), ""))
        {
            Names = originalCompetitorCacheItem.Names;
            _countryNames = originalCompetitorCacheItem._countryNames;
            _abbreviations = originalCompetitorCacheItem._abbreviations;
            _associatedPlayerIds = originalCompetitorCacheItem._associatedPlayerIds.IsNullOrEmpty() ? new List<Urn>() : originalCompetitorCacheItem._associatedPlayerIds;
            _associatedPlayerJerseyNumbers = originalCompetitorCacheItem._associatedPlayerJerseyNumbers.IsNullOrEmpty() ? new Dictionary<Urn, int>() : originalCompetitorCacheItem._associatedPlayerJerseyNumbers;
            _isVirtual = originalCompetitorCacheItem._isVirtual;
            _referenceId = originalCompetitorCacheItem._referenceId;
            _jerseys = originalCompetitorCacheItem._jerseys;
            _countryCode = originalCompetitorCacheItem._countryCode;
            _state = originalCompetitorCacheItem._state;
            _manager = originalCompetitorCacheItem._manager;
            _venue = originalCompetitorCacheItem._venue;
            _gender = originalCompetitorCacheItem._gender;
            _ageGroup = originalCompetitorCacheItem._ageGroup;
            _raceDriverProfile = originalCompetitorCacheItem._raceDriverProfile;
            _shortName = originalCompetitorCacheItem._shortName;
            _dataRouterManager = originalCompetitorCacheItem._dataRouterManager;
            _primaryCulture = originalCompetitorCacheItem._primaryCulture;
            Division = originalCompetitorCacheItem.Division;
            CultureCompetitorProfileFetched = originalCompetitorCacheItem.CultureCompetitorProfileFetched ?? new Dictionary<CultureInfo, DateTime>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCompetitor"/> containing information about the sport entity</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDto"/></param>
        internal CompetitorCacheItem(ExportableCompetitor exportable, IDataRouterManager dataRouterManager)
            : base(exportable)
        {
            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));

            Import(exportable);
        }

        public void UpdateAssociatedSportEvent(Urn associatedSportEventId)
        {
            if (associatedSportEventId == null)
            {
                return;
            }

            if (AssociatedSportEventId == null)
            {
                AssociatedSportEventId = associatedSportEventId;
            }
            else
            {
                if (associatedSportEventId.IsCompetition())
                {
                    AssociatedSportEventId = associatedSportEventId;
                }
                else if (AssociatedSportEventId.IsLongTermEvent() && associatedSportEventId.IsCompetition())
                {
                    AssociatedSportEventId = associatedSportEventId;
                }
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorDto"/> into the current instance
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(CompetitorDto competitor, CultureInfo culture)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();

            lock (_lockAdd)
            {
                MergeInternalCompetitorDto(competitor, culture);
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorProfileDto"/> into the current instance
        /// </summary>
        /// <param name="competitorProfile">A <see cref="CompetitorProfileDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        // ReSharper disable once CognitiveComplexity
        internal void Merge(CompetitorProfileDto competitorProfile, CultureInfo culture)
        {
            Guard.Argument(competitorProfile, nameof(competitorProfile)).NotNull();
            Guard.Argument(competitorProfile.Competitor, nameof(competitorProfile.Competitor)).NotNull();

            lock (_lockAdd)
            {
                MergeInternalCompetitorDto(competitorProfile.Competitor, culture);

                MergeAssociatedPlayers(competitorProfile.Players);
                MergeAssociatedTeamPlayers(competitorProfile.Competitor.Players);

                if (!competitorProfile.Jerseys.IsNullOrEmpty())
                {
                    _jerseys = competitorProfile.Jerseys.Select(s => new JerseyCacheItem(s)).ToList();
                }
                if (competitorProfile.Manager != null)
                {
                    if (_manager == null)
                    {
                        _manager = new ManagerCacheItem(competitorProfile.Manager, culture);
                    }
                    else
                    {
                        _manager.Merge(competitorProfile.Manager, culture);
                    }
                }
                if (competitorProfile.Venue != null)
                {
                    if (_venue == null)
                    {
                        _venue = new VenueCacheItem(competitorProfile.Venue, culture);
                    }
                    else
                    {
                        _venue.Merge(competitorProfile.Venue, culture);
                    }
                }
                if (competitorProfile.RaceDriverProfile != null)
                {
                    _raceDriverProfile = new RaceDriverProfileCacheItem(competitorProfile.RaceDriverProfile);
                }
                AddOrUpdateProfileFetchTime(culture);
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="SimpleTeamProfileDto"/> into the current instance
        /// </summary>
        /// <param name="simpleTeamProfile">A <see cref="SimpleTeamProfileDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(SimpleTeamProfileDto simpleTeamProfile, CultureInfo culture)
        {
            Guard.Argument(simpleTeamProfile, nameof(simpleTeamProfile)).NotNull();
            Guard.Argument(simpleTeamProfile.Competitor, nameof(simpleTeamProfile.Competitor)).NotNull();

            lock (_lockAdd)
            {
                MergeInternalCompetitorDto(simpleTeamProfile.Competitor, culture);

                AddOrUpdateProfileFetchTime(culture);
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorDto"/> into the current instance
        /// </summary>
        /// <param name="playerCompetitor">A <see cref="PlayerCompetitorDto"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(PlayerCompetitorDto playerCompetitor, CultureInfo culture)
        {
            Guard.Argument(playerCompetitor, nameof(playerCompetitor)).NotNull();

            Names[culture] = playerCompetitor.Name;
            _abbreviations[culture] = string.IsNullOrEmpty(playerCompetitor.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(playerCompetitor.Name)
                : playerCompetitor.Abbreviation;
            // NATIONALITY
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorCacheItem"/> into the current instance
        /// </summary>
        /// <param name="item">A <see cref="CompetitorCacheItem"/> containing information about the competitor</param>
        // ReSharper disable once CognitiveComplexity
        internal void Merge(CompetitorCacheItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            lock (_lockAdd)
            {
                MergeNamesFromCompetitorCi(item);
                if (!item._associatedPlayerIds.IsNullOrEmpty())
                {
                    _associatedPlayerIds = item._associatedPlayerIds.ToList();
                }
                if (!item._associatedPlayerJerseyNumbers.IsNullOrEmpty())
                {
                    _associatedPlayerJerseyNumbers = item._associatedPlayerJerseyNumbers;
                }
                _referenceId = item._referenceId ?? _referenceId;
                if (item._jerseys != null && !item._jerseys.IsNullOrEmpty())
                {
                    _jerseys = item._jerseys.ToList();
                }
                _countryCode = item._countryCode ?? _countryCode;
                _state = item._state ?? _state;
                _manager = item._manager ?? _manager;
                _venue = item._venue ?? _venue;
                _gender = item._gender ?? _gender;
                _ageGroup = item._ageGroup ?? _ageGroup;
                _raceDriverProfile = item._raceDriverProfile ?? _raceDriverProfile;
                _referenceId = item._referenceId ?? _referenceId;
                _sportId = item._sportId ?? _sportId;
                _categoryId = item._categoryId ?? _categoryId;
                if (item.Division != null)
                {
                    Division = item.Division;
                }
                if (item.IsVirtual.HasValue)
                {
                    _isVirtual = item.IsVirtual.Value;
                }
                CultureCompetitorProfileFetched = item.CultureCompetitorProfileFetched.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        private void MergeAssociatedTeamPlayers(ICollection<PlayerCompetitorDto> competitorPlayers)
        {
            if (competitorPlayers.IsNullOrEmpty())
            {
                return;
            }
            _associatedPlayerIds = competitorPlayers.Select(s => s.Id).ToList();
        }

        private void MergeAssociatedPlayers(ICollection<PlayerProfileDto> competitorPlayers)
        {
            if (competitorPlayers.IsNullOrEmpty())
            {
                return;
            }
            _associatedPlayerIds = competitorPlayers.Select(s => s.Id).ToList();

            foreach (var player in competitorPlayers)
            {
                if (player.JerseyNumber != null)
                {
                    _associatedPlayerJerseyNumbers[player.Id] = player.JerseyNumber.Value;
                }
            }
        }

        private void MergeNamesFromCompetitorCi(CompetitorCacheItem item)
        {
            if (!item.Names.IsNullOrEmpty())
            {
                foreach (var culture in item.Names.Keys)
                {
                    Names[culture] = item.Names[culture];
                }
            }
            if (!item._countryNames.IsNullOrEmpty())
            {
                foreach (var culture in item._countryNames.Keys)
                {
                    _countryNames[culture] = item._countryNames[culture];
                }
            }
            if (!item._abbreviations.IsNullOrEmpty())
            {
                foreach (var culture in item._abbreviations.Keys)
                {
                    _abbreviations[culture] = item._abbreviations[culture];
                }
            }
            if (!string.IsNullOrEmpty(item.ShortName))
            {
                _shortName = item.ShortName;
            }
        }

        private void MergeInternalCompetitorDto(CompetitorDto competitorDto, CultureInfo culture)
        {
            Names[culture] = competitorDto.Name;
            _countryNames[culture] = competitorDto.CountryName;
            _abbreviations[culture] = string.IsNullOrEmpty(competitorDto.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(competitorDto.Name)
                : competitorDto.Abbreviation;
            _referenceId = UpdateReferenceIds(competitorDto.Id, competitorDto.ReferenceIds);
            if (!string.IsNullOrEmpty(competitorDto.CountryCode))
            {
                _countryCode = competitorDto.CountryCode;
            }
            if (!string.IsNullOrEmpty(competitorDto.State))
            {
                _state = competitorDto.State;
            }
            MergeAssociatedTeamPlayers(competitorDto.Players);
            if (!string.IsNullOrEmpty(competitorDto.Gender))
            {
                _gender = competitorDto.Gender;
            }
            if (!string.IsNullOrEmpty(competitorDto.AgeGroup))
            {
                _ageGroup = competitorDto.AgeGroup;
            }
            if (competitorDto.SportId != null)
            {
                _sportId = competitorDto.SportId;
            }
            if (competitorDto.CategoryId != null)
            {
                _categoryId = competitorDto.CategoryId;
            }
            if (!string.IsNullOrEmpty(competitorDto.ShortName))
            {
                _shortName = competitorDto.ShortName;
            }
            if (competitorDto.Division != null)
            {
                Division = new DivisionCacheItem(competitorDto.Division);
            }
            if (competitorDto.IsVirtual.HasValue)
            {
                _isVirtual = competitorDto.IsVirtual.Value;
            }
        }

        private ReferenceIdCacheItem UpdateReferenceIds(Urn id, IDictionary<string, string> referenceIds)
        {
            if (id.IsSimpleTeam())
            {
                if (referenceIds == null || !referenceIds.Any())
                {
                    referenceIds = new Dictionary<string, string> { { "betradar", id.Id.ToString() } };
                }

                if (!referenceIds.ContainsKey("betradar"))
                {
                    referenceIds = new Dictionary<string, string>(referenceIds) { { "betradar", id.Id.ToString() } };
                }
            }

            if (_referenceId == null)
            {
                return new ReferenceIdCacheItem(referenceIds);
            }

            _referenceId.Merge(referenceIds, true);
            return _referenceId;
        }

        private void FetchProfileIfNeeded(CultureInfo culture)
        {
            if (!IsEligibleForFetch(culture))
            {
                return;
            }

            lock (_lock)
            {
                if (IsEligibleForFetch(culture) && _dataRouterManager != null)
                {
                    _dataRouterManager.GetCompetitorAsync(Id, culture, null).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// Check if culture is eligible for fetching (is not yet fetched)
        /// </summary>
        /// <param name="culture">Check for specified language</param>
        /// <returns>Returns true if it should be fetched, otherwise false</returns>
        private bool IsEligibleForFetch(CultureInfo culture)
        {
            return !CultureCompetitorProfileFetched.ContainsKey(culture);
        }

        /// <summary>
        /// Get the list of cultures which are requested filtered by those already received within last timeout
        /// </summary>
        /// <param name="wantedCultures">The list of cultures which we wanted</param>
        /// <returns>The list of cultures which are requested filtered by those already received within last timeout</returns>
        public ICollection<CultureInfo> GetMissingProfileCultures(IReadOnlyCollection<CultureInfo> wantedCultures)
        {
            return wantedCultures.IsNullOrEmpty() ? new Collection<CultureInfo>() : (ICollection<CultureInfo>)wantedCultures.Where(IsEligibleForFetch).ToList();
        }

        private void AddOrUpdateProfileFetchTime(CultureInfo culture)
        {
            CultureCompetitorProfileFetched[culture] = DateTime.Now;
        }

        /// <summary>
        /// Create exportable CacheItem as an asynchronous operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Task&lt;T&gt;.</returns>
        protected virtual async Task<T> CreateExportableBaseAsync<T>() where T : ExportableCompetitor, new()
        {
            var jerseysList = new List<ExportableJersey>();
            if (!_jerseys.IsNullOrEmpty())
            {
                foreach (var jersey in _jerseys)
                {
                    jerseysList.Add(await jersey.ExportAsync());
                }
            }

            var exportable = new T
            {
                Id = Id.ToString(),
                Names = Names.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(Names),
                CountryNames = _countryNames.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(_countryNames),
                Abbreviations = _abbreviations.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(_abbreviations),
                AssociatedPlayerIds = _associatedPlayerIds.IsNullOrEmpty() ? new List<string>() : new List<string>(_associatedPlayerIds.Select(i => i.ToString()).ToList()),
                AssociatedPlayersJerseyNumbers = _associatedPlayerJerseyNumbers.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
                IsVirtual = _isVirtual ?? false,
                ReferenceIds = _referenceId?.ReferenceIds == null ? new Dictionary<string, string>() : new Dictionary<string, string>((IDictionary<string, string>)_referenceId.ReferenceIds),
                Jerseys = jerseysList.IsNullOrEmpty() ? new List<ExportableJersey>() : new List<ExportableJersey>(jerseysList),
                CountryCode = _countryCode,
                State = _state,
                Manager = _manager != null ? await _manager.ExportAsync().ConfigureAwait(false) : null,
                Venue = _venue != null ? await _venue.ExportAsync().ConfigureAwait(false) : null,
                Gender = _gender,
                AgeGroup = _ageGroup,
                RaceDriverProfile = _raceDriverProfile != null ? await _raceDriverProfile.ExportAsync().ConfigureAwait(false) : null,
                PrimaryCulture = _primaryCulture,
                CultureCompetitorProfileFetched = CultureCompetitorProfileFetched.ToDictionary(pair => pair.Key, pair => pair.Value),
                SportId = _sportId?.ToString(),
                CategoryId = _categoryId?.ToString(),
                ShortName = _shortName,
                Division = Division?.Export()
            };

            return exportable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCacheItem"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCompetitor"/> containing information about the sport entity</param>
        internal void Import(ExportableCompetitor exportable)
        {
            lock (_lockAdd)
            {
                try
                {
                    Names = exportable.Names.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.Names);
                    _countryNames = exportable.CountryNames.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.CountryNames);
                    _abbreviations = exportable.Abbreviations.IsNullOrEmpty()
                                         ? new Dictionary<CultureInfo, string>()
                                         : new Dictionary<CultureInfo, string>(exportable.Abbreviations);
                    _associatedPlayerIds = exportable.AssociatedPlayerIds.IsNullOrEmpty() ? new List<Urn>() : new List<Urn>(exportable.AssociatedPlayerIds.Select(Urn.Parse));
                    _associatedPlayerJerseyNumbers = exportable.AssociatedPlayersJerseyNumbers.IsNullOrEmpty()
                                                         ? new Dictionary<Urn, int>()
                                                         : exportable.AssociatedPlayersJerseyNumbers.ToDictionary(pair => Urn.Parse(pair.Key), pair => pair.Value);
                    _isVirtual = exportable.IsVirtual;
                    _referenceId = exportable.ReferenceIds.IsNullOrEmpty() ? null : new ReferenceIdCacheItem(exportable.ReferenceIds);
                    _jerseys = exportable.Jerseys.IsNullOrEmpty() ? new List<JerseyCacheItem>() : new List<JerseyCacheItem>(exportable.Jerseys.Select(j => new JerseyCacheItem(j)));
                    _countryCode = exportable.CountryCode;
                    _state = exportable.State;
                    _manager = exportable.Manager == null ? null : new ManagerCacheItem(exportable.Manager);
                    _venue = exportable.Venue == null ? null : new VenueCacheItem(exportable.Venue);
                    _gender = exportable.Gender;
                    _ageGroup = exportable.AgeGroup;
                    _primaryCulture = exportable.PrimaryCulture;
                    _raceDriverProfile = exportable.RaceDriverProfile == null ? null : new RaceDriverProfileCacheItem(exportable.RaceDriverProfile);
                    _referenceId = exportable.ReferenceIds.IsNullOrEmpty() ? null : new ReferenceIdCacheItem(exportable.ReferenceIds);
                    CultureCompetitorProfileFetched = new Dictionary<CultureInfo, DateTime>();
                    if (exportable.CultureCompetitorProfileFetched != null)
                    {
                        CultureCompetitorProfileFetched = exportable.CultureCompetitorProfileFetched.ToDictionary(pair => pair.Key, pair => pair.Value);
                    }
                    _sportId = exportable.SportId != null ? Urn.Parse(exportable.SportId) : null;
                    _categoryId = exportable.CategoryId != null ? Urn.Parse(exportable.CategoryId) : null;
                    _shortName = exportable.ShortName;
                    if (exportable.Division != null)
                    {
                        Division = new DivisionCacheItem(exportable.Division);
                    }
                }
                catch (Exception e)
                {
                    SdkLoggerFactory.GetLoggerForExecution(typeof(CompetitorCacheItem)).LogError(e, "Importing CompetitorCacheItem {ExportableId}", exportable.Id);
                }
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public virtual async Task<ExportableBase> ExportAsync()
        {
            return await CreateExportableBaseAsync<ExportableCompetitor>().ConfigureAwait(false);
        }

        /// <summary>Determines whether the specified object is equal to the current object</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false</returns>
        /// <param name="obj">The object to compare with the current object</param>
        public override bool Equals(object obj)
        {
            if (!(obj is CompetitorCacheItem other))
            {
                return false;
            }
            return Id == other.Id;
        }

        /// <summary>Serves as the default hash function</summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}
