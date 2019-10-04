/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representing a sport category
    /// </summary>
    public class CategoryDTO : CategorySummaryDTO
    {
        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> used to compare different <see cref="tournamentExtended"/> instances
        /// </summary>
        private static readonly IEqualityComparer<tournamentExtended> EqualityComparer = new TournamentByTournamentIdEqualityComparer();

        /// <summary>
        /// Gets a <see cref="IEnumerable{TournamentDTO}"/> representing tournaments which belong to the associated category
        /// </summary>
        public readonly IEnumerable<TournamentDTO> Tournaments;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryDTO"/> class
        /// </summary>
        /// <param name="id">The id of the category</param>
        /// <param name="name">The name of the category</param>
        /// <param name="countryCode">A country code</param>
        /// <param name="tournaments">A <see cref="IEnumerable{TournamentDTO}"/> containing tournaments belonging to category</param>
        internal CategoryDTO(string id, string name, string countryCode, IEnumerable<tournamentExtended> tournaments)
            :base(id, name, countryCode)
        {
            Contract.Requires(!string.IsNullOrEmpty(id));
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Requires(tournaments != null);

            var recordList = tournaments as List<tournamentExtended> ?? tournaments.ToList();
            Tournaments = new ReadOnlyCollection<TournamentDTO>(recordList
                .Distinct(EqualityComparer)
                .Select(t => new TournamentDTO(t)).ToList());
        }

        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> implementation used for equality comparison of <see cref="tournamentExtended"/> instances
        /// </summary>
        /// <seealso cref="IEqualityComparer{tournamentExtended}" />
        private class TournamentByTournamentIdEqualityComparer : IEqualityComparer<tournamentExtended>
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
