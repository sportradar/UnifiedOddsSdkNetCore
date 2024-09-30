// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A data-transfer-object representing player's profile
    /// </summary>
    /// <seealso cref="SportEntityCacheItem" />
    internal class PlayerProfileCacheItem : SportEntityCacheItem, IExportableBase
    {
        private readonly IDataRouterManager _dataRouterManager;
        private string _type;
        private DateTime? _dateOfBirth;
        private int? _height;
        private int? _weight;
        private string _abbreviation;
        private string _gender;
        private Urn _competitorId;
        private readonly object _lockAdd = new object();

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing player name in different languages
        /// </summary>
        public IDictionary<CultureInfo, string> Names;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo, String}"/> containing player nationality in different languages
        /// </summary>
        private IDictionary<CultureInfo, string> _nationalities;

        /// <summary>
        /// Gets a value describing the type(e.g. forward, defense, ...) of the player represented by current instance
        /// </summary>
        public string Type
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _type;
            }
        }

        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the date of birth of the player associated with the current instance
        /// </summary>
        public DateTime? DateOfBirth
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _dateOfBirth;
            }
        }

        /// <summary>
        /// Gets the height in centimeters of the player represented by the current instance or a null reference if height is not known
        /// </summary>
        public int? Height
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _height;
            }
        }

        /// <summary>
        /// Gets the weight in kilograms of the player represented by the current instance or a null reference if weight is not known
        /// </summary>
        public int? Weight
        {
            get
            {
                FetchProfileIfNeeded(_primaryCulture);
                return _weight;
            }
        }

        /// <summary>
        /// The abbreviation
        /// </summary>
        /// <remarks>Available via <see cref="PlayerCompetitorDto"/></remarks>
        public string Abbreviation => _abbreviation;

        /// <summary>
        /// Gets the gender
        /// </summary>
        public string Gender => _gender;

        /// <summary>
        /// Gets the country code
        /// </summary>
        public string CountryCode { get; private set; }

        /// <summary>
        /// Gets the full name of the player
        /// </summary>
        public string FullName { get; private set; }

        /// <summary>
        /// Gets the nickname of the player
        /// </summary>
        public string Nickname { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{CultureInfo}"/> specifying the languages for which the current instance has translations
        /// </summary>
        /// <value>The fetched cultures</value>
        private IList<CultureInfo> _fetchedCultures;

        private readonly object _lock = new object();
        private CultureInfo _primaryCulture;

        /// <summary>
        /// The competitor id this player belongs to
        /// </summary>
        public Urn CompetitorId => _competitorId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileCacheItem"/> class
        /// </summary>
        /// <param name="profile">The <see cref="PlayerProfileDto"/> used to create instance</param>
        /// <param name="competitorId">The competitor id this player belongs to</param>
        /// <param name="culture">The culture of the <see cref="PlayerProfileDto"/> used to create new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="PlayerProfileDto"/></param>
        internal PlayerProfileCacheItem(PlayerProfileDto profile, Urn competitorId, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(profile)
        {
            Guard.Argument(profile, nameof(profile)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _nationalities = new Dictionary<CultureInfo, string>();

            Merge(profile, competitorId, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileCacheItem"/> class
        /// </summary>
        /// <param name="playerCompetitor">The <see cref="PlayerCompetitorDto"/> used to create instance</param>
        /// <param name="competitorId">The competitor id this player belongs to</param>
        /// <param name="culture">The culture of the <see cref="PlayerCompetitorDto"/> used to create new instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="PlayerCompetitorDto"/></param>
        internal PlayerProfileCacheItem(PlayerCompetitorDto playerCompetitor, Urn competitorId, CultureInfo culture, IDataRouterManager dataRouterManager)
            : base(playerCompetitor)
        {
            Guard.Argument(playerCompetitor, nameof(playerCompetitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();
            Guard.Argument(dataRouterManager, nameof(dataRouterManager)).NotNull();

            _fetchedCultures = new List<CultureInfo>();
            _primaryCulture = culture;

            _dataRouterManager = dataRouterManager;

            Names = new Dictionary<CultureInfo, string>();
            _nationalities = new Dictionary<CultureInfo, string>();

            Merge(playerCompetitor, competitorId, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfileCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportablePlayerProfile"/> used to create instance</param>
        /// <param name="dataRouterManager">The <see cref="IDataRouterManager"/> used to fetch <see cref="PlayerProfileDto"/></param>
        internal PlayerProfileCacheItem(ExportablePlayerProfile exportable, IDataRouterManager dataRouterManager)
            : base(exportable)
        {
            _dataRouterManager = dataRouterManager ?? throw new ArgumentNullException(nameof(dataRouterManager));

            Import(exportable);
        }

        internal void Import(ExportablePlayerProfile exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            lock (_lockAdd)
            {
                Names = exportable.Names.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.Names);
                if (!Names.IsNullOrEmpty())
                {
                    _fetchedCultures = new List<CultureInfo>(exportable.Names.Keys);
                    _primaryCulture = exportable.Names.Keys.First();
                }

                _nationalities = exportable.Nationalities.IsNullOrEmpty() ? new Dictionary<CultureInfo, string>() : new Dictionary<CultureInfo, string>(exportable.Nationalities);
                _type = exportable.Type;
                _dateOfBirth = exportable.DateOfBirth;
                _height = exportable.Height;
                _weight = exportable.Weight;
                _abbreviation = exportable.Abbreviation;
                _gender = exportable.Gender;
                _competitorId = string.IsNullOrEmpty(exportable.CompetitorId) ? null : Urn.Parse(exportable.CompetitorId);
                CountryCode = exportable.CountryCode;
                FullName = exportable.FullName;
                Nickname = exportable.Nickname;
            }
        }

        /// <summary>
        /// Merges the specified <see cref="PlayerProfileDto"/> into instance
        /// </summary>
        /// <param name="profile">The <see cref="PlayerProfileDto"/> used to merge into instance</param>
        /// <param name="competitorId">The competitor id this player belongs to</param>
        /// <param name="culture">The culture of the <see cref="PlayerProfileDto"/> used to merge</param>
        internal void Merge(PlayerProfileDto profile, Urn competitorId, CultureInfo culture)
        {
            Guard.Argument(profile, nameof(profile)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            lock (_lockAdd)
            {
                _type = profile.Type;
                _dateOfBirth = profile.DateOfBirth;
                Names[culture] = profile.Name;
                _nationalities[culture] = profile.Nationality;
                _height = profile.Height;
                _weight = profile.Weight;
                _gender = profile.Gender;
                CountryCode = profile.CountryCode;
                FullName = profile.FullName;
                Nickname = profile.Nickname;
                if (string.IsNullOrEmpty(_abbreviation))
                {
                    _abbreviation = SdkInfo.GetAbbreviationFromName(profile.Name);
                }

                if (competitorId != null)
                {
                    _competitorId = competitorId;
                }

                if (_fetchedCultures == null)
                {
                    _fetchedCultures = new List<CultureInfo>();
                }

                if (!_fetchedCultures.Contains(culture))
                {
                    _fetchedCultures.Add(culture);
                }
            }
        }

        /// <summary>
        /// Merges the specified <see cref="PlayerCompetitorDto"/> into instance
        /// </summary>
        /// <param name="playerCompetitor">The <see cref="PlayerCompetitorDto"/> used to merge into instance</param>
        /// <param name="competitorId">The competitor id this player belongs to</param>
        /// <param name="culture">The culture of the <see cref="PlayerCompetitorDto"/> used to merge</param>
        internal void Merge(PlayerCompetitorDto playerCompetitor, Urn competitorId, CultureInfo culture)
        {
            Guard.Argument(playerCompetitor, nameof(playerCompetitor)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            lock (_lockAdd)
            {
                Names[culture] = playerCompetitor.Name;
                _nationalities[culture] = playerCompetitor.Nationality;
                _abbreviation = string.IsNullOrEmpty(playerCompetitor.Abbreviation)
                                    ? SdkInfo.GetAbbreviationFromName(playerCompetitor.Name)
                                    : playerCompetitor.Abbreviation;
                if (competitorId != null)
                {
                    _competitorId = competitorId;
                }
            }
        }

        /// <summary>
        /// Gets the name of the player in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned name</param>
        /// <returns>The name of the player in the specified language if it exists. Null otherwise.</returns>
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
        /// Gets the nationality of the player in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned nationality</param>
        /// <returns>The nationality of the player in the specified language if it exists. Null otherwise.</returns>
        public string GetNationality(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            if (!_nationalities.ContainsKey(culture))
            {
                FetchProfileIfNeeded(culture);
            }

            return _nationalities.TryGetValue(culture, out var nationality)
                       ? nationality
                       : null;
        }

        private void FetchProfileIfNeeded(CultureInfo culture)
        {
            if (_fetchedCultures.Contains(culture))
            {
                return;
            }

            lock (_lock)
            {
                if (!_fetchedCultures.Contains(culture))
                {
                    _dataRouterManager.GetPlayerProfileAsync(Id, culture, null).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableBase> ExportAsync()
        {
            return Task.FromResult<ExportableBase>(new ExportablePlayerProfile
            {
                Id = Id.ToString(),
                Names = new Dictionary<CultureInfo, string>(Names),
                Nationalities = _nationalities.IsNullOrEmpty() ? null : new Dictionary<CultureInfo, string>(_nationalities),
                Type = _type,
                DateOfBirth = _dateOfBirth,
                Height = _height,
                Weight = _weight,
                Abbreviation = _abbreviation,
                Gender = _gender,
                CountryCode = CountryCode,
                FullName = FullName, Nickname = Nickname, CompetitorId = _competitorId?.ToString()
            });
        }
    }
}
