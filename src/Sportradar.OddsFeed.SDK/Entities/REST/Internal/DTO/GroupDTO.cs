/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a group
    /// </summary>
    public class GroupDTO
    {
        internal string Id { get; }

        internal string Name { get; }

        internal IEnumerable<CompetitorDTO> Competitors { get; }

        internal GroupDTO(tournamentGroup group)
        {
            Guard.Argument(group, nameof(group)).NotNull();

            Id = group.id ?? string.Empty;
            Name = group.name ?? string.Empty;
            Competitors = group.competitor == null
                ? null
                : new ReadOnlyCollection<CompetitorDTO>(group.competitor.Select(c => new CompetitorDTO(c)).ToList());
        }
    }
}
