/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for round
    /// </summary>
    public class RoundDTO
    {
        internal string Type { get; }

        internal int? Number { get; }

        internal string Name { get; }

        internal string Group { get; }

        internal URN GroupId { get; }

        internal string OtherMatchId { get; }

        internal int? CupRoundMatches { get; }

        internal int? CupRoundMatchNumber { get; }

        internal int? BetradarId { get; }

        internal string PhaseOrGroupLongName { get; }

        internal string Phase { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundDTO"/> class
        /// </summary>
        /// <param name="round">The <see cref="matchRound"/> used for creating new instance</param>
        internal RoundDTO(matchRound round)
        {
            Contract.Requires(round != null);
            Type = round.type;
            Number = round.numberSpecified
                ? (int?)round.number
                : null;
            Name = round.name;
            Group = round.group;
            URN groupId;
            GroupId = URN.TryParse(round.group_id ?? " ", out groupId) ? groupId : null;
            OtherMatchId = round.other_match_id;
            CupRoundMatches = round.cup_round_matchesSpecified
                ? (int?)round.cup_round_matches
                : null;
            CupRoundMatchNumber = round.cup_round_match_numberSpecified
                ? (int?) round.cup_round_match_number
                : null;
            BetradarId = round.betradar_idSpecified
                ? (int?) round.betradar_id
                : null;
            PhaseOrGroupLongName = round.group_long_name;
            Phase = round.phase;
        }
    }
}
