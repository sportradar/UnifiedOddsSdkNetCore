/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for round
    /// </summary>
    internal class RoundDto
    {
        internal string Type { get; }

        internal int? Number { get; }

        internal string Name { get; }

        internal string Group { get; }

        internal Urn GroupId { get; }

        internal string OtherMatchId { get; }

        internal int? CupRoundMatches { get; }

        internal int? CupRoundMatchNumber { get; }

        internal int? BetradarId { get; }

        internal string PhaseOrGroupLongName { get; }

        internal string Phase { get; }

        internal string BetradarName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundDto"/> class
        /// </summary>
        /// <param name="round">The <see cref="matchRound"/> used for creating new instance</param>
        internal RoundDto(matchRound round)
        {
            Guard.Argument(round, nameof(round)).NotNull();

            Type = round.type;
            Number = round.numberSpecified
                ? (int?)round.number
                : null;
            Name = !string.IsNullOrEmpty(round.name) ? round.name : null;
            PhaseOrGroupLongName = !string.IsNullOrEmpty(round.group_long_name) ? round.group_long_name : null;
            Group = round.group;
            Phase = round.phase;
            if (string.IsNullOrEmpty(round.group_id))
            {
                GroupId = null;
            }
            else
            {
                GroupId = Urn.TryParse(round.group_id, out var groupId) ? groupId : null;
            }
            OtherMatchId = round.other_match_id;
            CupRoundMatches = round.cup_round_matchesSpecified
                ? (int?)round.cup_round_matches
                : null;
            CupRoundMatchNumber = round.cup_round_match_numberSpecified
                ? (int?)round.cup_round_match_number
                : null;
            BetradarId = round.betradar_idSpecified
                ? (int?)round.betradar_id
                : null;
            BetradarName = round.betradar_name;
        }
    }
}
