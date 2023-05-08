/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// Competitor cache item to be saved within cache
    /// </summary>
    internal class CompetitorCI : SportEntityCI, IExportableCI
    {
        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor names in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Names;

        public URN AssociatedSportEventId;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor's country name in different languages
        /// </summary>
        private IDictionary<CultureInfo, string> _countryNames;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor abbreviations in different languages
        /// </summary>
        private IDictionary<CultureInfo, string> _abbreviations;

        private List<URN> _associatedPlayerIds;
        private bool _isVirtual;
        private ReferenceIdCI _referenceId;
        private List<JerseyCI> _jerseys;
        private string _countryCode;
        private ManagerCI _manager;
        private VenueCI _venue;
        private string _gender;
        private string _ageGroup;
        private RaceDriverProfileCI _raceDriverProfile;
        private string _state;
        private URN _sportId;
        private URN _categoryId;
        private string _shortName;

        /// <summary>
        /// The list of CultureInfo used to fetch competitor profiles
        /// </summary>
        public IDictionary<CultureInfo, DateTime> CultureCompetitorProfileFetched { get; private set; }

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

            return Names.ContainsKey(culture)
                ? Names[culture]
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

            return _countryNames.ContainsKey(culture)
                ? _countryNames[culture]
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

            return _abbreviations.ContainsKey(culture)
                ? _abbreviations[culture]
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
        public bool IsVirtual
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
        public ReferenceIdCI ReferenceId => _referenceId;

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IEnumerable<URN> AssociatedPlayerIds
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
        public IEnumerable<URN> GetAssociatedPlayerIds()
        {
            return _associatedPlayerIds;
        }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        public IEnumerable<JerseyCI> Jerseys
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
        public ManagerCI Manager
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
        public VenueCI Venue
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
        public URN SportId
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
        public URN CategoryId
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
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public RaceDriverProfileCI RaceDriverProfile => _raceDriverProfile;

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
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDTO"/></param>
        internal CompetitorCI(CompetitorDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
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
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
            RootSource = "CompetitorDTO";
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorProfileDTO"/></param>
        internal CompetitorCI(CompetitorProfileDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager = null)
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
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
            RootSource = "CompetitorProfileDTO";
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="SimpleTeamProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="SimpleTeamProfileDTO"/></param>
        internal CompetitorCI(SimpleTeamProfileDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager = null)
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
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
            RootSource = "SimpleTeamProfileDTO";
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="playerCompetitor">A <see cref="PlayerCompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDTO"/></param>
        internal CompetitorCI(PlayerCompetitorDTO playerCompetitor, CultureInfo culture, IDataRouterManager dataRouterManager)
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
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
            RootSource = "PlayerCompetitorDTO";
            Merge(playerCompetitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityCI"/> class
        /// </summary>
        /// <param name="originalCompetitorCI">A <see cref="CompetitorCI"/> containing information about the sport entity</param>
        protected CompetitorCI(CompetitorCI originalCompetitorCI)
            : base(new SportEntityDTO(originalCompetitorCI.Id.ToString(), ""))
        {
            Names = originalCompetitorCI.Names;
            _countryNames = originalCompetitorCI._countryNames;
            _abbreviations = originalCompetitorCI._abbreviations;
            _associatedPlayerIds = originalCompetitorCI._associatedPlayerIds.IsNullOrEmpty() ? new List<URN>() : originalCompetitorCI._associatedPlayerIds;
            _isVirtual = originalCompetitorCI._isVirtual;
            _referenceId = originalCompetitorCI._referenceId;
            _jerseys = originalCompetitorCI._jerseys;
            _countryCode = originalCompetitorCI._countryCode;
            _state = originalCompetitorCI._state;
            _manager = originalCompetitorCI._manager;
            _venue = originalCompetitorCI._venue;
            _gender = originalCompetitorCI._gender;
            _ageGroup = originalCompetitorCI._ageGroup;
            _raceDriverProfile = originalCompetitorCI._raceDriverProfile;
            _shortName = originalCompetitorCI._shortName;
            _dataRouterManager = originalCompetitorCI._dataRouterManager;
            _primaryCulture = originalCompetitorCI._primaryCulture;
            CultureCompetitorProfileFetched = originalCompetitorCI.CultureCompetitorProfileFetched ?? new Dictionary<CultureInfo, DateTime>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCompetitorCI"/> containing information about the sport entity</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDTO"/></param>
        internal CompetitorCI(ExportableCompetitorCI exportable, IDataRouterManager dataRouterManager)
            : base(exportable)
        {
            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));

            Import(exportable);
        }

        public void UpdateAssociatedSportEvent(URN associatedSportEventId)
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
        /// Merges the information from the provided <see cref="CompetitorDTO"/> into the current instance
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(CompetitorDTO competitor, CultureInfo culture)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();

            lock (_lockAdd)
            {
                _isVirtual = competitor.IsVirtual;
                Names[culture] = competitor.Name;
                _countryNames[culture] = competitor.CountryName;
                _abbreviations[culture] = string.IsNullOrEmpty(competitor.Abbreviation)
                                              ? SdkInfo.GetAbbreviationFromName(competitor.Name)
                                              : competitor.Abbreviation;
                _referenceId = UpdateReferenceIds(competitor.Id, competitor.ReferenceIds);
                if (!string.IsNullOrEmpty(competitor.CountryCode))
                {
                    _countryCode = competitor.CountryCode;
                }
                if (!string.IsNullOrEmpty(competitor.State))
                {
                    _state = competitor.State;
                }
                if (competitor.Players != null && !competitor.Players.IsNullOrEmpty())
                {
                    _associatedPlayerIds = competitor.Players.Select(s => s.Id).ToList();
                }
                if (!string.IsNullOrEmpty(competitor.Gender))
                {
                    _gender = competitor.Gender;
                }
                if (!string.IsNullOrEmpty(competitor.AgeGroup))
                {
                    _ageGroup = competitor.AgeGroup;
                }
                if (competitor.SportId != null)
                {
                    _sportId = competitor.SportId;
                }
                if (competitor.CategoryId != null)
                {
                    _categoryId = competitor.CategoryId;
                }
                if (!string.IsNullOrEmpty(competitor.ShortName))
                {
                    _shortName = competitor.ShortName;
                }
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorProfileDTO"/> into the current instance
        /// </summary>
        /// <param name="competitorProfile">A <see cref="CompetitorProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(CompetitorProfileDTO competitorProfile, CultureInfo culture)
        {
            Guard.Argument(competitorProfile, nameof(competitorProfile)).NotNull();
            Guard.Argument(competitorProfile.Competitor, nameof(competitorProfile.Competitor)).NotNull();

            lock (_lockAdd)
            {
                _isVirtual = competitorProfile.Competitor.IsVirtual;
                Names[culture] = competitorProfile.Competitor.Name;
                _countryNames[culture] = competitorProfile.Competitor.CountryName;
                _abbreviations[culture] = string.IsNullOrEmpty(competitorProfile.Competitor.Abbreviation)
                                              ? SdkInfo.GetAbbreviationFromName(competitorProfile.Competitor.Name)
                                              : competitorProfile.Competitor.Abbreviation;
                _referenceId = UpdateReferenceIds(competitorProfile.Competitor.Id, competitorProfile.Competitor.ReferenceIds);
                if (!string.IsNullOrEmpty(competitorProfile.Competitor.CountryCode))
                {
                    _countryCode = competitorProfile.Competitor.CountryCode;
                }
                if (!string.IsNullOrEmpty(competitorProfile.Competitor.State))
                {
                    _state = competitorProfile.Competitor.State;
                }
                if (!competitorProfile.Players.IsNullOrEmpty())
                {
                    _associatedPlayerIds = competitorProfile.Players.Select(s => s.Id).ToList();
                }
                if (!competitorProfile.Jerseys.IsNullOrEmpty())
                {
                    _jerseys = competitorProfile.Jerseys.Select(s => new JerseyCI(s)).ToList();
                }
                if (competitorProfile.Manager != null)
                {
                    if (_manager == null)
                    {
                        _manager = new ManagerCI(competitorProfile.Manager, culture);
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
                        _venue = new VenueCI(competitorProfile.Venue, culture);
                    }
                    else
                    {
                        _venue.Merge(competitorProfile.Venue, culture);
                    }
                }
                if (!string.IsNullOrEmpty(competitorProfile.Competitor.Gender))
                {
                    _gender = competitorProfile.Competitor.Gender;
                }
                if (!string.IsNullOrEmpty(competitorProfile.Competitor.AgeGroup))
                {
                    _ageGroup = competitorProfile.Competitor.AgeGroup;
                }
                if (competitorProfile.RaceDriverProfile != null)
                {
                    _raceDriverProfile = new RaceDriverProfileCI(competitorProfile.RaceDriverProfile);
                }
                if (competitorProfile.Competitor.SportId != null)
                {
                    _sportId = competitorProfile.Competitor.SportId;
                }
                if (competitorProfile.Competitor.CategoryId != null)
                {
                    _categoryId = competitorProfile.Competitor.CategoryId;
                }
                if (!string.IsNullOrEmpty(competitorProfile.Competitor.ShortName))
                {
                    _shortName = competitorProfile.Competitor.ShortName;
                }
                AddOrUpdateProfileFetchTime(culture);
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="SimpleTeamProfileDTO"/> into the current instance
        /// </summary>
        /// <param name="simpleTeamProfile">A <see cref="SimpleTeamProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(SimpleTeamProfileDTO simpleTeamProfile, CultureInfo culture)
        {
            Guard.Argument(simpleTeamProfile, nameof(simpleTeamProfile)).NotNull();
            Guard.Argument(simpleTeamProfile.Competitor, nameof(simpleTeamProfile.Competitor)).NotNull();

            lock (_lockAdd)
            {
                _isVirtual = simpleTeamProfile.Competitor.IsVirtual;
                Names[culture] = simpleTeamProfile.Competitor.Name;
                _countryNames[culture] = simpleTeamProfile.Competitor.CountryName;
                _abbreviations[culture] = string.IsNullOrEmpty(simpleTeamProfile.Competitor.Abbreviation)
                                              ? SdkInfo.GetAbbreviationFromName(simpleTeamProfile.Competitor.Name)
                                              : simpleTeamProfile.Competitor.Abbreviation;
                _referenceId = UpdateReferenceIds(simpleTeamProfile.Competitor.Id, simpleTeamProfile.Competitor.ReferenceIds);
                if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.CountryCode))
                {
                    _countryCode = simpleTeamProfile.Competitor.CountryCode;
                }
                if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.State))
                {
                    _state = simpleTeamProfile.Competitor.State;
                }
                if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.Gender))
                {
                    _gender = simpleTeamProfile.Competitor.Gender;
                }
                if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.AgeGroup))
                {
                    _ageGroup = simpleTeamProfile.Competitor.AgeGroup;
                }
                if (!simpleTeamProfile.Competitor.Players.IsNullOrEmpty())
                {
                    _associatedPlayerIds = simpleTeamProfile.Competitor.Players.Select(s => s.Id).ToList();
                }
                if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.ShortName))
                {
                    _shortName = simpleTeamProfile.Competitor.ShortName;
                }
                AddOrUpdateProfileFetchTime(culture);
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorDTO"/> into the current instance
        /// </summary>
        /// <param name="playerCompetitor">A <see cref="PlayerCompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <c>dto</c></param>
        internal void Merge(PlayerCompetitorDTO playerCompetitor, CultureInfo culture)
        {
            Guard.Argument(playerCompetitor, nameof(playerCompetitor)).NotNull();

            Names[culture] = playerCompetitor.Name;
            _abbreviations[culture] = string.IsNullOrEmpty(playerCompetitor.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(playerCompetitor.Name)
                : playerCompetitor.Abbreviation;
            // NATIONALITY
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorCI"/> into the current instance
        /// </summary>
        /// <param name="item">A <see cref="CompetitorCI"/> containing information about the competitor</param>
        internal void Merge(CompetitorCI item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            lock (_lockAdd)
            {
                if (!item.Names.IsNullOrEmpty())
                {
                    foreach (var k in item.Names.Keys)
                    {
                        Names[k] = item.Names[k];
                    }
                }
                if (!item._countryNames.IsNullOrEmpty())
                {
                    foreach (var k in item._countryNames.Keys)
                    {
                        _countryNames[k] = item._countryNames[k];
                    }
                }
                if (!item._abbreviations.IsNullOrEmpty())
                {
                    foreach (var k in item._abbreviations.Keys)
                    {
                        _abbreviations[k] = item._abbreviations[k];
                    }
                }
                if (!item._associatedPlayerIds.IsNullOrEmpty())
                {
                    _associatedPlayerIds = item._associatedPlayerIds.ToList();
                }
                _isVirtual = item.IsVirtual;
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
                if (!string.IsNullOrEmpty(item.ShortName))
                {
                    _shortName = item.ShortName;
                }
                CultureCompetitorProfileFetched = item.CultureCompetitorProfileFetched.ToDictionary(pair => pair.Key, pair => pair.Value);
            }
        }

        private ReferenceIdCI UpdateReferenceIds(URN id, IDictionary<string, string> referenceIds)
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
                return new ReferenceIdCI(referenceIds);
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
        public bool IsEligibleForFetch(CultureInfo culture)
        {
            return !CultureCompetitorProfileFetched.ContainsKey(culture);
        }

        /// <summary>
        /// Check if culture is eligible for fetching (is not yet fetched or it was fetched long ago)
        /// </summary>
        /// <param name="culture">Check for specified language</param>
        /// <returns>Returns true if it should be fetched, otherwise false</returns>
        public bool IsEligibleForFetchForce(CultureInfo culture)
        {
            return !CultureCompetitorProfileFetched.ContainsKey(culture) || CultureCompetitorProfileFetched[culture] < DateTime.Now.AddSeconds(-30);
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
            if (CultureCompetitorProfileFetched.ContainsKey(culture))
            {
                CultureCompetitorProfileFetched[culture] = DateTime.Now;
            }
            else
            {
                CultureCompetitorProfileFetched.Add(culture, DateTime.Now);
            }
        }

        /// <summary>
        /// Create exportable CI as an asynchronous operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Task&lt;T&gt;.</returns>
        protected virtual async Task<T> CreateExportableCIAsync<T>() where T : ExportableCompetitorCI, new()
        {
            var jerseysList = new List<ExportableJerseyCI>();
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
                Name = Names.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(Names),
                CountryNames = _countryNames.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(_countryNames),
                Abbreviations = _abbreviations.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(_abbreviations),
                AssociatedPlayerIds = _associatedPlayerIds.IsNullOrEmpty() ? new List<string>() : new List<string>(_associatedPlayerIds.Select(i => i.ToString()).ToList()),
                IsVirtual = _isVirtual,
                ReferenceIds = _referenceId?.ReferenceIds == null ? new Dictionary<string, string>() : new Dictionary<string, string>(_referenceId.ReferenceIds),
                Jerseys = jerseysList.IsNullOrEmpty() ? new List<ExportableJerseyCI>() : new List<ExportableJerseyCI>(jerseysList),
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
                ShortName = _shortName
            };

            return exportable;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCompetitorCI"/> containing information about the sport entity</param>
        internal void Import(ExportableCompetitorCI exportable)
        {
            lock (_lockAdd)
            {
                try
                {
                    Names = exportable.Name.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.Name);
                    _countryNames = exportable.CountryNames.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.CountryNames);
                    _abbreviations = exportable.Abbreviations.IsNullOrEmpty()
                                         ? new Dictionary<CultureInfo, string>()
                                         : new Dictionary<CultureInfo, string>(exportable.Abbreviations);
                    _associatedPlayerIds = exportable.AssociatedPlayerIds.IsNullOrEmpty() ? new List<URN>() : new List<URN>(exportable.AssociatedPlayerIds.Select(URN.Parse));
                    _isVirtual = exportable.IsVirtual;
                    _referenceId = exportable.ReferenceIds.IsNullOrEmpty() ? null : new ReferenceIdCI(exportable.ReferenceIds);
                    _jerseys = exportable.Jerseys.IsNullOrEmpty() ? new List<JerseyCI>() : new List<JerseyCI>(exportable.Jerseys.Select(j => new JerseyCI(j)));
                    _countryCode = exportable.CountryCode;
                    _state = exportable.State;
                    _manager = exportable.Manager == null ? null : new ManagerCI(exportable.Manager);
                    _venue = exportable.Venue == null ? null : new VenueCI(exportable.Venue);
                    _gender = exportable.Gender;
                    _ageGroup = exportable.AgeGroup;
                    _primaryCulture = exportable.PrimaryCulture;
                    _raceDriverProfile = exportable.RaceDriverProfile == null ? null : new RaceDriverProfileCI(exportable.RaceDriverProfile);
                    _referenceId = exportable.ReferenceIds.IsNullOrEmpty() ? null : new ReferenceIdCI(exportable.ReferenceIds);
                    CultureCompetitorProfileFetched = new Dictionary<CultureInfo, DateTime>();
                    if (exportable.CultureCompetitorProfileFetched != null)
                    {
                        CultureCompetitorProfileFetched = exportable.CultureCompetitorProfileFetched.ToDictionary(pair => pair.Key, pair => pair.Value);
                    }
                    _sportId = exportable.SportId != null ? URN.Parse(exportable.SportId) : null;
                    _categoryId = exportable.CategoryId != null ? URN.Parse(exportable.CategoryId) : null;
                    _shortName = exportable.ShortName;
                }
                catch (Exception e)
                {
                    SdkLoggerFactory.GetLoggerForExecution(typeof(CompetitorCI)).LogError(e, $"Importing CompetitorCI {exportable.Id}");
                }
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public virtual async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableCompetitorCI>().ConfigureAwait(false);

        /// <summary>Determines whether the specified object is equal to the current object</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false</returns>
        /// <param name="obj">The object to compare with the current object</param>
        public override bool Equals(object obj)
        {
            if (!(obj is CompetitorCI other))
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
