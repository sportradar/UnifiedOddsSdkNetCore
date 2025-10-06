// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// Defines a cache item for round
    /// </summary>
    internal class RoundCacheItem
    {
        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing round names in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _names;

        /// <summary>
        /// A <see cref="IDictionary{CultureInfo,String}"/> containing phase or group long name in different languages
        /// </summary>
        private readonly IDictionary<CultureInfo, string> _phaseOrGroupLongName;

        /// <summary>
        /// Type of the round
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the name of the group associated with the current round.
        /// </summary>
        public string Group { get; private set; }

        /// <summary>
        /// Gets the id of the group associated with the current round.
        /// </summary>
        public Urn GroupId { get; private set; }

        /// <summary>
        /// Gets the id of the other match
        /// </summary>
        public string OtherMatchId { get; private set; }

        /// <summary>
        /// Gets a value specifying the round number or a null reference if round number is not defined
        /// </summary>
        public int? Number { get; private set; }

        /// <summary>
        /// Gets a value specifying the number of matches in the current cup round or a null reference
        /// if number of matches is not applicable to current <see cref="IRound" /> instance
        /// </summary>
        public int? CupRoundMatches { get; private set; }

        /// <summary>
        /// Gets a value specifying the number of the match in the current cup round or a null reference
        /// if match number is not applicable to current <see cref="IRound" /> instance
        /// </summary>
        public int? CupRoundMatchNumber { get; private set; }

        /// <summary>
        /// Gets the betradar identifier
        /// </summary>
        public int? BetradarId { get; private set; }

        /// <summary>
        /// Gets the phase
        /// </summary>
        /// <value>The phase</value>
        public string Phase { get; private set; }

        /// <summary>
        /// Gets the betradar name
        /// </summary>
        /// <value>The betradar name</value>
        public string BetradarName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundCacheItem"/> class
        /// </summary>
        /// <param name="dto">The <see cref="RoundDto"/> used to create new instance</param>
        /// <param name="culture">The culture of the input <see cref="RoundDto"/></param>
        internal RoundCacheItem(RoundDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();
            Guard.Argument(culture, nameof(culture)).NotNull();

            _names = new Dictionary<CultureInfo, string>();
            _phaseOrGroupLongName = new Dictionary<CultureInfo, string>();
            Merge(dto, culture);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundCacheItem"/> class
        /// </summary>
        /// <param name="exportable">The <see cref="ExportableRound"/> used to create new instance</param>
        internal RoundCacheItem(ExportableRound exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            _names = new Dictionary<CultureInfo, string>(exportable.Names);
            _phaseOrGroupLongName = new Dictionary<CultureInfo, string>(exportable.PhaseOrGroupLongName);
            Type = exportable.Type;
            Group = exportable.Group;
            GroupId = exportable.GroupId != null ? Urn.Parse(exportable.GroupId) : null;
            OtherMatchId = exportable.OtherMatchId;
            Number = exportable.Number;
            CupRoundMatches = exportable.CupRoundMatches;
            CupRoundMatchNumber = exportable.CupRoundMatchNumber;
            BetradarId = exportable.BetradarId;
            Phase = exportable.Phase;
            BetradarName = exportable.BetradarName;
        }

        /// <summary>
        /// Merges the specified <see cref="RoundDto"/> into instance
        /// </summary>
        /// <param name="dto">The <see cref="RoundDto"/> used fro merging</param>
        /// <param name="culture">The culture of the input <see cref="RoundDto"/></param>
        internal void Merge(RoundDto dto, CultureInfo culture)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Type = dto.Type;
            Group = dto.Group;
            GroupId = dto.GroupId;
            OtherMatchId = dto.OtherMatchId;
            Number = dto.Number;
            CupRoundMatches = dto.CupRoundMatches;
            CupRoundMatchNumber = dto.CupRoundMatchNumber;
            BetradarId = dto.BetradarId;
            _names[culture] = dto.Name;
            _phaseOrGroupLongName[culture] = dto.PhaseOrGroupLongName;
            Phase = dto.Phase;
            BetradarName = dto.BetradarName;
        }

        /// <summary>
        /// Gets the name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the Name if exists, or null</returns>
        public string GetName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _names == null || !_names.ContainsKey(culture)
                       ? null
                       : _names[culture];
        }

        /// <summary>
        /// Gets the phase or group long name for specific locale
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the phase or group long name if exists, or null</returns>
        public string GetPhaseOrGroupLongName(CultureInfo culture)
        {
            Guard.Argument(culture, nameof(culture)).NotNull();

            return _phaseOrGroupLongName == null || !_phaseOrGroupLongName.ContainsKey(culture)
                       ? null
                       : _phaseOrGroupLongName[culture];
        }

        /// <summary>
        /// Determines whether the current instance has translations for the specified languages
        /// </summary>
        /// <param name="cultures">A <see cref="IEnumerable{CultureInfo}"/> specifying the required languages</param>
        /// <returns>True if the current instance contains data in the required locals. Otherwise false.</returns>
        public virtual bool HasTranslationsFor(IEnumerable<CultureInfo> cultures)
        {
            return cultures.All(c => _names.ContainsKey(c));
        }

        /// <summary>
        /// Asynchronous export item's properties
        /// </summary>
        /// <returns>An <see cref="ExportableBase"/> instance containing all relevant properties</returns>
        public Task<ExportableRound> ExportAsync()
        {
            return Task.FromResult(new ExportableRound
            {
                Names = new Dictionary<CultureInfo, string>(_names),
                PhaseOrGroupLongName = new Dictionary<CultureInfo, string>(_phaseOrGroupLongName),
                Type = Type,
                GroupId = GroupId?.ToString(),
                Group = Group,
                OtherMatchId = OtherMatchId,
                Number = Number,
                BetradarId = BetradarId,
                Phase = Phase,
                CupRoundMatchNumber = CupRoundMatchNumber,
                CupRoundMatches = CupRoundMatches,
                BetradarName = BetradarName
            });
        }
    }
}
