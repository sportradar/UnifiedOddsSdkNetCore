/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides basic tournament round information
    /// </summary>
    /// <seealso cref="IRound" />
    internal class Round : EntityPrinter, IRound
    {
        /// <summary>
        /// Gets the type of the round
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets a value specifying the round number or a null reference if round number is not defined
        /// </summary>
        public int? Number { get; }

        /// <summary>
        /// Gets the group associated with the current round
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the name of the current <see cref="IRound" />
        /// </summary>
        public IDictionary<CultureInfo, string> Name { get; }

        /// <summary>
        /// Gets the group name of the current <see cref="IRound"/> per language
        /// </summary>
        [Obsolete("GroupName was removed from API")]
        public IDictionary<CultureInfo, string> GroupName { get; }

        /// <summary>
        /// Gets the phase or group long name of the current <see cref="IRound"/> per language
        /// </summary>
        public IDictionary<CultureInfo, string> PhaseOrGroupLongName { get; }

        /// <summary>
        /// Gets the id of the group associated with the current round
        /// </summary>
        public URN GroupId { get; }

        /// <summary>
        /// Gets the id of the other match
        /// </summary>
        public string OtherMatchId { get; }

        /// <summary>
        /// Gets a value specifying the number of matches in the current cup round or a null reference
        /// if number of matches is not applicable to current <see cref="IRound" /> instance
        /// </summary>
        public int? CupRoundMatches { get; }

        /// <summary>
        /// Gets a value specifying the number of the match in the current cup round or a null reference
        /// if match number is not applicable to current <see cref="IRound" /> instance
        /// </summary>
        public int? CupRoundMatchNumber { get; }

        /// <summary>
        /// Gets the betradar identifier
        /// </summary>
        public int BetradarId { get; }

        /// <summary>
        /// Gets the phase of the associated round
        /// </summary>
        public string Phase { get; }

        /// <summary>
        /// A betradar name
        /// </summary>
        public string BetradarName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Round"/> class
        /// </summary>
        /// <param name="ci">A <see cref="RoundCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="RoundCI"/></param>
        public Round(RoundCI ci, IEnumerable<CultureInfo> cultures)
        {
            Guard.Argument(ci, nameof(ci)).NotNull();

            Type = ci.Type;
            Number = ci.Number;
            Group = ci.Group;
            GroupId = ci.GroupId;
            OtherMatchId = ci.OtherMatchId;
            CupRoundMatches = ci.CupRoundMatches;
            CupRoundMatchNumber = ci.CupRoundMatchNumber;
            Name = new Dictionary<CultureInfo, string>();
            GroupName = new Dictionary<CultureInfo, string>();
            PhaseOrGroupLongName = new Dictionary<CultureInfo, string>();
            foreach (var c in cultures)
            {
                Name.Add(c, ci.GetName(c));
                GroupName.Add(c, ci.GetGroupName(c));
                PhaseOrGroupLongName.Add(c, ci.GetPhaseOrGroupLongName(c));
            }
            BetradarId = ci.BetradarId ?? 0;
            Phase = ci.Phase;
            BetradarName = ci.BetradarName;
        }

        /// <summary>
        /// Gets the name for specific language
        /// </summary>
        /// <param name="culture">The cultures</param>
        /// <returns>Return the Name if exists, or null</returns>
        public string GetName(CultureInfo culture)
        {
            return Name.ContainsKey(culture) ? Name[culture] : null;
        }

        /// <summary>
        /// Gets the group name for specific language
        /// </summary>
        /// <param name="culture">The cultures</param>
        /// <returns>Return the Name if exists, or null</returns>
        [Obsolete("GroupName was removed from API")]
        public string GetGroupName(CultureInfo culture)
        {
            return GroupName.ContainsKey(culture) ? GroupName[culture] : null;
        }

        /// <summary>
        /// Gets the phase or group long name for specific language
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>Return the phase or group long name if exists, or null</returns>
        public string GetPhaseOrGroupLongName(CultureInfo culture)
        {
            return PhaseOrGroupLongName.ContainsKey(culture) ? PhaseOrGroupLongName[culture] : null;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Name={Name.FirstOrDefault()}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            var names = string.Join(", ", Name.Keys.Select(k => $"{k}={GetName(k)}"));
            var groupNames = string.Join(", ", GroupName.Keys.Select(k => $"{k}={GetGroupName(k)}"));
            var phase = string.Join(", ", PhaseOrGroupLongName.Keys.Select(k => $"{k}={GetPhaseOrGroupLongName(k)}"));
            return $"Name=[{names}], GroupName=[{groupNames}], PhaseOrGroupLongName={phase}, Type={Type}, Number={Number}, CupRoundMatches={CupRoundMatches}, CupRoundMatchNumber={CupRoundMatchNumber}, BetradarId={BetradarId}, BetradarName={BetradarName}, Phase={Phase}";
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
