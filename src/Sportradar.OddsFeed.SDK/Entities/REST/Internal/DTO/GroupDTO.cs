/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a group
    /// </summary>
    internal class GroupDto
    {
        internal string Id { get; }

        internal string Name { get; }

        internal IEnumerable<CompetitorDto> Competitors { get; }

        internal GroupDto(tournamentGroup group)
        {
            Guard.Argument(group, nameof(group)).NotNull();

            Id = group.id ?? string.Empty;
            Name = group.name ?? string.Empty;
            Competitors = group.competitor == null
                ? null
                : new ReadOnlyCollection<CompetitorDto>(group.competitor.Select(c => new CompetitorDto(c)).ToList());
        }
    }
}
