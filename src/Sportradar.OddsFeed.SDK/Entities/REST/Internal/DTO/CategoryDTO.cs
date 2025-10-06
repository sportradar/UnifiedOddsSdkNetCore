// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a sport category
    /// </summary>
    internal class CategoryDto : CategorySummaryDto
    {
        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> used to compare different <see cref="tournamentExtended"/> instances
        /// </summary>
        private static readonly IEqualityComparer<tournamentExtended> EqualityComparer = new TournamentByTournamentIdEqualityComparer();

        /// <summary>
        /// Gets a <see cref="IEnumerable{TournamentDto}"/> representing tournaments which belong to the associated category
        /// </summary>
        public readonly IEnumerable<TournamentDto> Tournaments;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDto"/> class
        /// </summary>
        /// <param name="id">The id of the category</param>
        /// <param name="name">The name of the category</param>
        /// <param name="countryCode">A country code</param>
        /// <param name="tournaments">A <see cref="ICollection{TournamentDto}"/> containing tournaments belonging to category</param>
        internal CategoryDto(string id, string name, string countryCode, ICollection<tournamentExtended> tournaments)
            : base(id, name, countryCode)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();
            Guard.Argument(tournaments, nameof(tournaments)).NotNull();

            var recordList = tournaments as List<tournamentExtended> ?? tournaments.ToList();
            Tournaments = new ReadOnlyCollection<TournamentDto>(recordList
                                                               .Distinct(EqualityComparer)
                                                               .Select(t => new TournamentDto(t))
                                                               .ToList());
        }

        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> implementation used for equality comparison of <see cref="tournamentExtended"/> instances
        /// </summary>
        /// <seealso cref="IEqualityComparer{tournamentExtended}" />
        private sealed class TournamentByTournamentIdEqualityComparer : IEqualityComparer<tournamentExtended>
        {
            public bool Equals(tournamentExtended x, tournamentExtended y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }
                return x.id == y.id;
            }

            public int GetHashCode(tournamentExtended obj)
            {
                return obj.id.GetHashCode();
            }
        }
    }
}
