// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a sport
    /// </summary>
    internal class SportDto : SportEntityDto
    {
        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> used to compare different <see cref="tournamentExtended"/> instances
        /// </summary>
        private static readonly IEqualityComparer<tournamentExtended> EqualityComparer = new TournamentByCategoryIdEqualityComparer();

        /// <summary>
        /// Gets a <see cref="IEnumerable{CategoryDto}"/> representing categories which belong to the sport
        /// </summary>
        public readonly IEnumerable<CategoryDto> Categories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDto"/> from the provided tournaments
        /// </summary>
        /// <param name="id">The id of the sport</param>
        /// <param name="name">The name of the sport</param>
        /// <param name="tournaments">A <see cref="IEnumerable{tournamentExtended}"/> representing tournament which belong to the sport</param>
        internal SportDto(string id, string name, IEnumerable<tournamentExtended> tournaments)
            : base(id, name)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();

            if (tournaments == null)
            {
                return;
            }

            var recordList = tournaments as List<tournamentExtended> ?? tournaments.ToList();

            var categories = new List<CategoryDto>();
            foreach (var distinctRecord in recordList.Distinct(EqualityComparer))
            {
                categories.Add(new CategoryDto(
                    distinctRecord.category.id,
                    distinctRecord.category.name,
                    distinctRecord.category.country_code,
                    recordList.Where(record => record.category.id == distinctRecord.category.id).ToList()));
            }
            Categories = new ReadOnlyCollection<CategoryDto>(categories);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportDto"/> from the provided tournaments
        /// </summary>
        /// <param name="id">The id of the sport</param>
        /// <param name="name">The name of the sport</param>
        /// <param name="categories">A <see cref="IEnumerable{CategoryDto}"/> representing categories which belong to the sport</param>
        internal SportDto(string id, string name, IEnumerable<CategoryDto> categories)
            : base(id, name)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            Guard.Argument(name, nameof(name)).NotNull().NotEmpty();

            Categories = new ReadOnlyCollection<CategoryDto>(categories.ToList());
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
                return obj.category.id.GetHashCode();
            }
        }
    }
}
