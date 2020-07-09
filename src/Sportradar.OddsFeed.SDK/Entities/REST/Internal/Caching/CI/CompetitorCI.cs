/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
        public readonly IDictionary<CultureInfo, string> Names;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor's country name in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _countryNames;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing competitor abbreviations in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _abbreviations;

        private readonly List<URN> _associatedPlayerIds;

        private bool _isVirtual;
        private ReferenceIdCI _referenceId;
        private readonly List<JerseyCI> _jerseys;
        private string _countryCode;
        private ManagerCI _manager;
        private VenueCI _venue;
        private string _gender;
        private string _ageGroup;
        private RaceDriverProfileCI _raceDriverProfile;
        private DateTime _lastTimeCompetitorProfileFetched;
        private List<CultureInfo> _cultureCompetitorProfileFetched;
        private string _state;
        private URN _sportId;
        private URN _categoryId;

        /// <summary>
        /// Last time (if any) competitor profile was fetched
        /// </summary>
        public DateTime LastTimeCompetitorProfileFetched => _lastTimeCompetitorProfileFetched;

        /// <summary>
        /// The list of CultureInfo used to fetch competitor profiles
        /// </summary>
        public List<CultureInfo> CultureCompetitorProfileFetched => _cultureCompetitorProfileFetched;

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
        public ReferenceIdCI ReferenceId
        {
            get
            {
                //DEBUG
                //FetchProfileIfNeeded(_primaryCulture);
                return _referenceId;
            }
        }

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IEnumerable<URN> AssociatedPlayerIds
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _associatedPlayerIds;
            }
        }

        /// <summary>
        /// Gets the jerseys of known competitors
        /// </summary>
        /// <value>The jerseys</value>
        public IEnumerable<JerseyCI> Jerseys
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
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
        /// Gets the <see cref="IEnumerable{CultureInfo}"/> specifying the languages for which the current instance has translations
        /// </summary>
        /// <value>The fetched cultures</value>
        private readonly IEnumerable<CultureInfo> _fetchedCultures;
        private readonly IDataRouterManager _dataRouterManager;
        private readonly object _lock = new object();
        private readonly CultureInfo _primaryCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDTO"/></param>
        internal CompetitorCI(CompetitorDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
            _lastTimeCompetitorProfileFetched = DateTime.MinValue;
            _cultureCompetitorProfileFetched = new List<CultureInfo>();
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorProfileDTO"/></param>
        internal CompetitorCI(CompetitorProfileDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager = null)
            : base(competitor.Competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
_lastTimeCompetitorProfileFetched = DateTime.MinValue;
            _cultureCompetitorProfileFetched = new List<CultureInfo>();
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="competitor">A <see cref="SimpleTeamProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="SimpleTeamProfileDTO"/></param>
        internal CompetitorCI(SimpleTeamProfileDTO competitor, CultureInfo culture, IDataRouterManager dataRouterManager = null)
            : base(competitor.Competitor)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
_lastTimeCompetitorProfileFetched = DateTime.MinValue;
            _cultureCompetitorProfileFetched = new List<CultureInfo>();
            Merge(competitor, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="playerCompetitor">A <see cref="PlayerCompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDTO"/></param>
        internal CompetitorCI(PlayerCompetitorDTO playerCompetitor, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(playerCompetitor)
        {
            Guard.Argument(playerCompetitor, nameof(playerCompetitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _countryNames = new Dictionary<CultureInfo, string>();
            _abbreviations = new Dictionary<CultureInfo, string>();
            _associatedPlayerIds = new List<URN>();
            _jerseys = new List<JerseyCI>();
            _lastTimeCompetitorProfileFetched = DateTime.MinValue;
            _cultureCompetitorProfileFetched = new List<CultureInfo>();
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
            _associatedPlayerIds = originalCompetitorCI._associatedPlayerIds;
            _isVirtual = originalCompetitorCI._isVirtual;
            _referenceId = originalCompetitorCI._referenceId;
            _jerseys = originalCompetitorCI._jerseys;
            _countryCode = originalCompetitorCI._countryCode;
            _state = originalCompetitorCI._state;
            _manager = originalCompetitorCI._manager;
            _venue = originalCompetitorCI._venue;
            _gender = originalCompetitorCI._gender;
            _ageGroup = originalCompetitorCI._ageGroup;
            _fetchedCultures = originalCompetitorCI._fetchedCultures;
            _dataRouterManager = originalCompetitorCI._dataRouterManager;
            _primaryCulture = originalCompetitorCI._primaryCulture;
            _raceDriverProfile = originalCompetitorCI._raceDriverProfile;
            _lastTimeCompetitorProfileFetched = originalCompetitorCI._lastTimeCompetitorProfileFetched;
            _cultureCompetitorProfileFetched = originalCompetitorCI._cultureCompetitorProfileFetched;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompetitorCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCompetitorCI"/> containing information about the sport entity</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="CompetitorDTO"/></param>
        internal CompetitorCI(ExportableCompetitorCI exportable, IDataRouterManager dataRouterManager)
            : base(exportable)
        {
            Names = new Dictionary<CultureInfo, string>(exportable.Name);
            _countryNames = new Dictionary<CultureInfo, string>(exportable.CountryNames);
            _abbreviations = new Dictionary<CultureInfo, string>(exportable.Abbreviations);
            _associatedPlayerIds = new List<URN>(exportable.AssociatedPlayerIds.Select(URN.Parse));
            _isVirtual = exportable.IsVirtual;
            _referenceId = new ReferenceIdCI(exportable.ReferenceIds);
            _jerseys = new List<JerseyCI>(exportable.Jerseys.Select(j => new JerseyCI(j)));
            _countryCode = exportable.CountryCode;
            _state = exportable.State;
            _manager = exportable.Manager != null ? new ManagerCI(exportable.Manager) : null;
            _venue = exportable.Venue != null ? new VenueCI(exportable.Venue) : null;
            _gender = exportable.Gender;
            _ageGroup = exportable.AgeGroup;
            _fetchedCultures = new List<CultureInfo>(exportable.FetchedCultures);
            _dataRouterManager = dataRouterManager;
            _primaryCulture = exportable.PrimaryCulture;
            _raceDriverProfile = exportable.RaceDriverProfile != null ? new RaceDriverProfileCI(exportable.RaceDriverProfile) : null;
            _referenceId = new ReferenceIdCI(exportable.ReferenceIds);
            _lastTimeCompetitorProfileFetched = DateTime.MinValue;
            if (exportable.LastTimeCompetitorProfileIsFetched.HasValue)
            {
                _lastTimeCompetitorProfileFetched = exportable.LastTimeCompetitorProfileIsFetched.Value;
            }
            if (exportable.CultureCompetitorProfileFetched != null)
            {
                _cultureCompetitorProfileFetched = exportable.CultureCompetitorProfileFetched.ToList();
            }
            _sportId = exportable.SportId != null ? URN.Parse(exportable.SportId) : null;
            _categoryId = exportable.CategoryId != null ? URN.Parse(exportable.CategoryId) : null;
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorDTO"/> into the current instance
        /// </summary>
        /// <param name="competitor">A <see cref="CompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        internal void Merge(CompetitorDTO competitor, CultureInfo culture)
        {
            Guard.Argument(competitor, nameof(competitor)).NotNull();

            _isVirtual = competitor.IsVirtual;
            Names[culture] = competitor.Name;
            _countryNames[culture] = competitor.CountryName;
            _abbreviations[culture] = string.IsNullOrEmpty(competitor.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(competitor.Name)
                : competitor.Abbreviation;
            _referenceId = UpdateReferenceIds(competitor.Id, competitor.ReferenceIds);
            _countryCode = competitor.CountryCode;
            _state = competitor.State;
            if (competitor.Players != null && competitor.Players.Any())
            {
                _associatedPlayerIds.Clear();
                _associatedPlayerIds.AddRange(competitor.Players.Select(s => s.Id));
            }
            if (!string.IsNullOrEmpty(competitor.Gender))
            {
                _gender = competitor.Gender;
            }
            if (!string.IsNullOrEmpty(competitor.AgeGroup))
            {
                _ageGroup = competitor.AgeGroup;
            }
            //((List<CultureInfo>)_fetchedCultures).Add(culture);
            if (competitor.SportId != null)
            {
                _sportId = competitor.SportId;
            }

            if (competitor.CategoryId != null)
            {
                _categoryId = competitor.CategoryId;
            }
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorProfileDTO"/> into the current instance
        /// </summary>
        /// <param name="competitorProfile">A <see cref="CompetitorProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        internal void Merge(CompetitorProfileDTO competitorProfile, CultureInfo culture)
        {
            Guard.Argument(competitorProfile, nameof(competitorProfile)).NotNull();
            Guard.Argument(competitorProfile.Competitor, nameof(competitorProfile.Competitor)).NotNull();

            _isVirtual = competitorProfile.Competitor.IsVirtual;
            Names[culture] = competitorProfile.Competitor.Name;
            _countryNames[culture] = competitorProfile.Competitor.CountryName;
            _abbreviations[culture] = string.IsNullOrEmpty(competitorProfile.Competitor.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(competitorProfile.Competitor.Name)
                : competitorProfile.Competitor.Abbreviation;
            _referenceId =
                UpdateReferenceIds(competitorProfile.Competitor.Id, competitorProfile.Competitor.ReferenceIds);
            _countryCode = competitorProfile.Competitor.CountryCode;
            _state = competitorProfile.Competitor.State;

            if (competitorProfile.Players != null && competitorProfile.Players.Any())
            {
                _associatedPlayerIds.Clear();
                _associatedPlayerIds.AddRange(competitorProfile.Players.Select(s => s.Id));
            }

            if (competitorProfile.Jerseys != null && competitorProfile.Jerseys.Any())
            {
                _jerseys.Clear();
                _jerseys.AddRange(competitorProfile.Jerseys.Select(s => new JerseyCI(s)));
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
            
            if (competitorProfile.Players != null && competitorProfile.Players.Any())
            {
                _lastTimeCompetitorProfileFetched = DateTime.Now;
                _cultureCompetitorProfileFetched.Add(culture);
            }
            if (competitorProfile.Competitor.SportId != null)
            {
                _sportId = competitorProfile.Competitor.SportId;
            }

            if (competitorProfile.Competitor.CategoryId != null)
            {
                _categoryId = competitorProfile.Competitor.CategoryId;
            }

            ((List<CultureInfo>) _fetchedCultures).Add(culture);
        }

        /// <summary>
        /// Merges the information from the provided <see cref="SimpleTeamProfileDTO"/> into the current instance
        /// </summary>
        /// <param name="simpleTeamProfile">A <see cref="SimpleTeamProfileDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
        internal void Merge(SimpleTeamProfileDTO simpleTeamProfile, CultureInfo culture)
        {
            Guard.Argument(simpleTeamProfile, nameof(simpleTeamProfile)).NotNull();
            Guard.Argument(simpleTeamProfile.Competitor, nameof(simpleTeamProfile.Competitor)).NotNull();

            _isVirtual = simpleTeamProfile.Competitor.IsVirtual;
            Names[culture] = simpleTeamProfile.Competitor.Name;
            _countryNames[culture] = simpleTeamProfile.Competitor.CountryName;
            _abbreviations[culture] = string.IsNullOrEmpty(simpleTeamProfile.Competitor.Abbreviation)
                ? SdkInfo.GetAbbreviationFromName(simpleTeamProfile.Competitor.Name)
                : simpleTeamProfile.Competitor.Abbreviation;
            _referenceId =
                UpdateReferenceIds(simpleTeamProfile.Competitor.Id, simpleTeamProfile.Competitor.ReferenceIds);
            _countryCode = simpleTeamProfile.Competitor.CountryCode;
            _state = simpleTeamProfile.Competitor.State;
            if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.Gender))
            {
                _gender = simpleTeamProfile.Competitor.Gender;
            }
            if (!string.IsNullOrEmpty(simpleTeamProfile.Competitor.AgeGroup))
            {
                _ageGroup = simpleTeamProfile.Competitor.AgeGroup;
            }

            if (simpleTeamProfile.Competitor.Players != null && simpleTeamProfile.Competitor.Players.Any())
            {
                _lastTimeCompetitorProfileFetched = DateTime.Now;
                _cultureCompetitorProfileFetched.Add(culture);
            }
            ((List<CultureInfo>) _fetchedCultures).Add(culture);
        }

        /// <summary>
        /// Merges the information from the provided <see cref="CompetitorDTO"/> into the current instance
        /// </summary>
        /// <param name="playerCompetitor">A <see cref="PlayerCompetitorDTO"/> containing information about the competitor</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the passed <code>dto</code></param>
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

            foreach (var k in item.Names.Keys)
            {
                Names[k] = item.Names[k];
            }
            foreach (var k in item._countryNames.Keys)
            {
                _countryNames[k] = item._countryNames[k];
            }
            foreach (var k in item._abbreviations.Keys)
            {
                _abbreviations[k] = item._abbreviations[k];
            }
            _associatedPlayerIds.Clear();
            _associatedPlayerIds.AddRange(item._associatedPlayerIds);
            _isVirtual = item.IsVirtual;
            _referenceId = item._referenceId ?? _referenceId;
            _jerseys.Clear();
            _jerseys.AddRange(item._jerseys);
            _countryCode = item._countryCode ?? _countryCode;
            _state = item._state ?? _state;
            _manager = item._manager ?? _manager;
            _venue = item._venue ?? _venue;
            _gender = item._gender ?? _gender;
            _ageGroup = item._ageGroup ?? _ageGroup;
            _raceDriverProfile = item._raceDriverProfile ?? _raceDriverProfile;
            _referenceId = item._referenceId ?? _referenceId;
            _lastTimeCompetitorProfileFetched = item._lastTimeCompetitorProfileFetched;
            _cultureCompetitorProfileFetched = item._cultureCompetitorProfileFetched?.ToList();
            _sportId = item._sportId ?? _sportId;
            _categoryId = item._categoryId ?? _categoryId;
        }

        private ReferenceIdCI UpdateReferenceIds(URN id, IDictionary<string, string> referenceIds)
        {
            if (id.Type.Equals(SdkInfo.SimpleTeamIdentifier, StringComparison.InvariantCultureIgnoreCase))
            {
                if (referenceIds == null || !referenceIds.Any())
                {
                    referenceIds = new Dictionary<string, string> {{"betradar", id.Id.ToString()}};
                }

                if (!referenceIds.ContainsKey("betradar"))
                {
                    referenceIds = new Dictionary<string, string>(referenceIds) {{"betradar", id.Id.ToString()}};
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
            if (_fetchedCultures.Contains(culture))
            {
                return;
            }

            lock (_lock)
            {
                if (!_fetchedCultures.Contains(culture) && _dataRouterManager != null)
                {
                    var task = Task.Run(async () =>
                    {
                        await _dataRouterManager.GetCompetitorAsync(Id, culture, null).ConfigureAwait(false);
                    });
                    task.Wait();
                }
            }
        }

        /// <summary>
        /// Create exportable ci as an asynchronous operation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Task&lt;T&gt;.</returns>
        protected virtual async Task<T> CreateExportableCIAsync<T>() where T : ExportableCompetitorCI, new()
        {
            var jerseysList = new List<ExportableJerseyCI>();
            foreach (var jersey in _jerseys)
            {
                jerseysList.Add(await jersey.ExportAsync());
            }

            var exportable = new T
            {
                Id = Id.ToString(),
                Name = new ReadOnlyDictionary<CultureInfo, string>(Names),
                CountryNames = new ReadOnlyDictionary<CultureInfo, string>(_countryNames),
                Abbreviations = new ReadOnlyDictionary<CultureInfo, string>(_abbreviations),
                AssociatedPlayerIds = new ReadOnlyCollection<string>(_associatedPlayerIds.Select(i => i.ToString()).ToList()),
                IsVirtual = _isVirtual,
                ReferenceIds = _referenceId.ReferenceIds != null ? new ReadOnlyDictionary<string, string>(_referenceId.ReferenceIds as IDictionary<string, string>) : null,
                Jerseys = new ReadOnlyCollection<ExportableJerseyCI>(jerseysList),
                CountryCode = _countryCode,
                State = _state,
                Manager = _manager!= null ? await _manager.ExportAsync().ConfigureAwait(false) : null,
                Venue = _venue != null ? await _venue.ExportAsync().ConfigureAwait(false) : null,
                Gender = _gender,
                AgeGroup = _ageGroup,
                RaceDriverProfile = _raceDriverProfile != null ? await _raceDriverProfile.ExportAsync().ConfigureAwait(false) : null,
                FetchedCultures = new ReadOnlyCollection<CultureInfo>(_fetchedCultures.ToList()),
                PrimaryCulture = _primaryCulture,
                LastTimeCompetitorProfileIsFetched = _lastTimeCompetitorProfileFetched > DateTime.MinValue
                                                     ? _lastTimeCompetitorProfileFetched
                                                     : (DateTime?) null,
                CultureCompetitorProfileFetched = _cultureCompetitorProfileFetched,
                SportId = _sportId?.ToString(),
                CategoryId = _categoryId?.ToString()
            };

            return exportable;
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableCI"/> instance containing all relevant properties</returns>
        public virtual async Task<ExportableCI> ExportAsync() => await CreateExportableCIAsync<ExportableCompetitorCI>().ConfigureAwait(false);
    }
}