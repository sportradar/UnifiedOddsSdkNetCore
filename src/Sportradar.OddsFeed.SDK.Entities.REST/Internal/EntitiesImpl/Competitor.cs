/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a player or a team competing in a sport event
    /// </summary>
    /// <seealso cref="Player" />
    /// <seealso cref="ICompetitor" />
    [DataContract]
    internal class Competitor : Player, ICompetitorV2
    {
        private readonly CompetitorCI _competitorCI;
        private readonly IProfileCache _profileCache;
        private readonly List<CultureInfo> _cultures;
        private readonly ISportEntityFactory _sportEntityFactory;
        private ReferenceIdCI _referenceId;
        private readonly CompetitionCI _competitionCI;
        private readonly object _lock = new object();
        protected string TeamQualifier;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's country name in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Countries => new ReadOnlyDictionary<CultureInfo, string>(
            _cultures.Where(c => GetCompetitor().GetCountry(c) != null).ToDictionary(c => c, GetCompetitor().GetCountry));

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing competitor's abbreviations in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Abbreviations => new ReadOnlyDictionary<CultureInfo, string>(
            _cultures.Where(c => GetCompetitor().GetAbbreviation(c) != null).ToDictionary(c => c, GetCompetitor().GetAbbreviation));

        /// <summary>
        /// Gets a value indicating whether the current <see cref="ICompetitor" /> is virtual - i.e. competes in a virtual sport
        /// </summary>
        public bool IsVirtual => GetCompetitor().IsVirtual;

        /// <summary>
        /// Gets the reference ids
        /// </summary>
        public IReference References
        {
            get
            {
                FetchEventCompetitorsReferenceIds();
                return _referenceId != null
                           ? new Reference(_referenceId)
                           : null;
            }
        }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode => GetCompetitor().CountryCode;

        /// <summary>
        /// Gets the list of associated player ids
        /// </summary>
        /// <value>The associated player ids</value>
        public IEnumerable<IPlayer> AssociatedPlayers {
            get
            {
                if (GetCompetitor().AssociatedPlayerIds != null && GetCompetitor().AssociatedPlayerIds.Any())
                {
                    return _sportEntityFactory.BuildPlayersAsync(GetCompetitor().AssociatedPlayerIds, _cultures).Result;
                }

                return null;
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
                if (GetCompetitor().Jerseys != null && GetCompetitor().Jerseys.Any())
                {
                    return GetCompetitor().Jerseys.Select(s => new Jersey(s));
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the manager
        /// </summary>
        /// <value>The manager</value>
        public IManager Manager => GetCompetitor().Manager != null ? new Manager(GetCompetitor().Manager) : null;

        /// <summary>
        /// Gets the venue
        /// </summary>
        /// <value>The venue</value>
        public IVenue Venue => GetCompetitor().Venue != null ? new Venue(GetCompetitor().Venue, _cultures) : null;

        /// <summary>
        /// Initializes a new instance of the <see cref="Competitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayerProfile"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        public Competitor(CompetitorCI ci,
                          IProfileCache profileCache,
                          IEnumerable<CultureInfo> cultures,
                          ISportEntityFactory sportEntityFactory,
                          ICompetitionCI rootCompetitionCI)
            : base(ci.Id, new Dictionary<CultureInfo, string>())
        {
            //Contract.Requires(ci != null);
            Contract.Requires(cultures != null && cultures.Any());
            Contract.Requires(sportEntityFactory != null);

            if (ci == null)
            {
                // above contract requirement throws even when ci in fact not null
                throw new ArgumentNullException(nameof(ci));
            }

            _competitorCI = ci;
            _profileCache = profileCache;
            _cultures = cultures.ToList();
            _sportEntityFactory = sportEntityFactory;
            _competitionCI = (CompetitionCI) rootCompetitionCI;
            _referenceId = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Competitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayerProfile"/></param>
        /// <param name="competitorsReferences">A list of <see cref="ReferenceIdCI"/> for all competitors</param>
        public Competitor(CompetitorCI ci,
                          IProfileCache profileCache,
                          IEnumerable<CultureInfo> cultures,
                          ISportEntityFactory sportEntityFactory,
                          IDictionary<URN, ReferenceIdCI> competitorsReferences)
            : base(ci.Id, new Dictionary<CultureInfo, string>())
        {
            //Contract.Requires(ci != null);
            Contract.Requires(cultures != null && cultures.Any());
            Contract.Requires(sportEntityFactory != null);

            if (ci == null)
            {
                // above contract requirement throws even when ci in fact not null
                throw new ArgumentNullException(nameof(ci));
            }

            _competitorCI = ci;
            _profileCache = profileCache;
            _cultures = cultures.ToList();
            _sportEntityFactory = sportEntityFactory;
            _competitionCI = null;
            _referenceId = null;

            if (competitorsReferences != null && competitorsReferences.Any())
            {
                ReferenceIdCI q;
                if (competitorsReferences.TryGetValue(ci.Id, out q))
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
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Country if exists, or null</returns>
        public string GetCountry(CultureInfo culture)
        {
            return Countries.ContainsKey(culture)
                ? Countries[culture]
                : null;
        }

        /// <summary>
        /// Gets the competitor's abbreviation in the specified language or a null reference
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language of the abbreviation</param>
        /// <returns>The competitor's abbreviation in the specified language or a null reference</returns>
        public string GetAbbreviation(CultureInfo culture)
        {
            return Abbreviations.ContainsKey(culture)
                ? Abbreviations[culture]
                : null;
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance</returns>
        protected override string PrintC()
        {
            var abbreviations = string.Join(", ", Abbreviations.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            var associatedPlayers = string.Empty;
            if (AssociatedPlayers != null && AssociatedPlayers.Any())
            {
                associatedPlayers = string.Join(", ", AssociatedPlayers.Select(s => s.Id + ": " + s.GetName(_cultures.First())));
                associatedPlayers = $", AssociatedPlayers=[{associatedPlayers}]";
            }
            var reference = _referenceId?.ReferenceIds == null || !_referenceId.ReferenceIds.Any()
                                ? string.Empty
                                : _referenceId.ReferenceIds.Aggregate(string.Empty, (current, item) => current = $"{current}, {item.Key}={item.Value}").Substring(2);
            return $"{base.PrintC()}, Gender={Gender}, Reference={reference}, Abbreviations=[{abbreviations}]{associatedPlayers}";
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
            var reference = References == null
                                ? string.Empty
                                : References.ToString("f");
            return $"{base.PrintF()}, Countries=[{countryNames}], Reference={reference}, Abbreviations=[{abbreviations}], IsVirtual={IsVirtual}{associatedPlayers}";
        }

        protected void FetchEventCompetitorsReferenceIds()
        {
            lock (_lock)
            {
                if (_competitionCI != null || _competitorCI != null)
                {
                    var task = Task.Run(async () =>
                                        {
                                            var competitorsReferences = _competitionCI != null
                                                                            ? await _competitionCI.GetCompetitorsReferencesAsync().ConfigureAwait(false)
                                                                            : null;

                                            if (competitorsReferences != null && competitorsReferences.Any())
                                            {
                                                ReferenceIdCI q;
                                                if (competitorsReferences.TryGetValue(_competitorCI.Id, out q))
                                                {
                                                    _referenceId = q;
                                                }
                                            }
                                            else
                                            {
                                                if (GetCompetitor().ReferenceId != null)
                                                {
                                                    _referenceId = GetCompetitor().ReferenceId;
                                                }
                                            }
                                        });
                    task.Wait();
                }
            }
        }

        protected void FetchEventCompetitorsQualifiers()
        {
            lock (_lock)
            {
                if (_competitionCI != null)
                {
                    var task = Task.Run(async () =>
                                        {
                                            var competitorsQualifiers = await _competitionCI.GetCompetitorsQualifiersAsync().ConfigureAwait(false);

                                            if (competitorsQualifiers != null && competitorsQualifiers.Any())
                                            {
                                                string qualifier;
                                                if (competitorsQualifiers.TryGetValue(_competitorCI.Id, out qualifier))
                                                {
                                                    TeamQualifier = qualifier;
                                                }
                                            }
                                        });
                    task.Wait();
                }
            }
        }

        private CompetitorCI GetCompetitor()
        {
            return _profileCache != null
                ? _profileCache.GetCompetitorProfileAsync(_competitorCI.Id, _cultures).Result
                : _competitorCI;
        }

        private IReadOnlyDictionary<CultureInfo, string> _names;

        public override IReadOnlyDictionary<CultureInfo, string> Names
        {
            get
            {
                if (_names != null)
                {
                    return _names;
                }
                lock (_lock)
                {
                    _names = _cultures.Where(c => _competitorCI.GetName(c) != null).ToDictionary(c => c, _competitorCI.GetName);
                    return _names;
                }
            }
        }

        public override string GetName(CultureInfo culture)
        {
            return _names != null
                       ? base.GetName(culture)
                       : _competitorCI.GetName(culture);
        }

        /// <summary>
        /// Gets the gender
        /// </summary>
        /// <value>The gender</value>
        public string Gender => GetCompetitor()?.Gender;

        /// <summary>
        /// Gets the race driver profile
        /// </summary>
        /// <value>The race driver profile</value>
        public IRaceDriverProfile RaceDriverProfile
        {
            get
            {
                var raceDriverProfileCI = GetCompetitor()?.RaceDriverProfile;
                return raceDriverProfileCI == null ? null : new RaceDriverProfile(raceDriverProfileCI);
            }
        }
    }
}
