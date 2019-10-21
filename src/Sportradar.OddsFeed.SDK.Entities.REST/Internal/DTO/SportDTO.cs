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
    /// A data-transfer-object representing a sport
    /// </summary>
    public class SportDTO : SportEntityDTO
    {
        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> used to compare different <see cref="tournamentExtended"/> instances
        /// </summary>
        private static readonly IEqualityComparer<tournamentExtended> EqualityComparer = new TournamentByCategoryIdEqualityComparer();

        /// <summary>
        /// Gets a <see cref="IEnumerable{CategoryDTO}"/> representing categories which belong to the sport
        /// </summary>
        public readonly IEnumerable<CategoryDTO> Categories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDTO"/> from the provided tournaments
        /// </summary>
        /// <param name="id">The id of the sport</param>
        /// <param name="name">The name of the sport</param>
        /// <param name="tournaments">A <see cref="IEnumerable{tournamentExtended}"/> representing tournament which belong to the sport</param>
        internal SportDTO(string id, string name, IEnumerable<tournamentExtended> tournaments)
            :base(id, name)
        {
            Guard.Argument(!string.IsNullOrEmpty(id));
            Guard.Argument(!string.IsNullOrEmpty(name));

            if (tournaments == null)
            {
                return;
            }

            var recordList = tournaments as List<tournamentExtended> ?? tournaments.ToList();

            var categories = new List<CategoryDTO>();
            foreach (var distinctRecord in recordList.Distinct(EqualityComparer))
            {
                categories.Add(new CategoryDTO(
                    distinctRecord.category.id,
                    distinctRecord.category.name,
                    distinctRecord.category.country_code,
                    recordList.Where(record => record.category.id == distinctRecord.category.id)));
            }
            Categories = new ReadOnlyCollection<CategoryDTO>(categories);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDTO"/> from the provided tournaments
        /// </summary>
        /// <param name="id">The id of the sport</param>
        /// <param name="name">The name of the sport</param>
        /// <param name="categories">A <see cref="IEnumerable{CategoryDTO}"/> representing categories which belong to the sport</param>
        internal SportDTO(string id, string name, IEnumerable<CategoryDTO> categories)
            : base(id, name)
        {
            Guard.Argument(!string.IsNullOrEmpty(id));
            Guard.Argument(!string.IsNullOrEmpty(name));

            Categories = new ReadOnlyCollection<CategoryDTO>(categories.ToList());
        }

        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> used for equality comparison of <see cref="tournamentExtended"/> instances. Instances are
        /// considered equal if they belong to same category
        /// </summary>
        /// <seealso cref="IEqualityComparer{tournamentExtended}" />
        internal class TournamentByCategoryIdEqualityComparer : IEqualityComparer<tournamentExtended>
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
                return x.category.id == y.category.id;
            }

            public int GetHashCode(tournamentExtended obj)
            {
                return obj?.category.id.GetHashCode() ?? 0;
            }
        }
    }
}