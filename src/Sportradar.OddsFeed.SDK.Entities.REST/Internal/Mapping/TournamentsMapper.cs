/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> implementation used to construct <see cref="EntityList{SportDTO}" /> instances
    /// from <see cref="tournamentsEndpoint"/> instances
    /// </summary>
    public class TournamentsMapper : ISingleTypeMapper<EntityList<SportDTO>>
    {

        /// <summary>
        /// A <see cref="IEqualityComparer{tournamentExtended}"/> used to compare different <see cref="tournamentExtended"/> instances
        /// </summary>
        private static readonly IEqualityComparer<tournamentExtended> EqualityComparer = new TournamentBySportIdEqualityComparer();

        /// <summary>
        /// A <see cref="tournamentsEndpoint"/> instance containing tournaments data
        /// </summary>
        private readonly tournamentsEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="TournamentsMapper"/> class.
        /// </summary>
        /// <param name="data">A <see cref="tournamentsEndpoint"/> instance containing tournaments data</param>
        protected TournamentsMapper(tournamentsEndpoint data)
        {
            Contract.Requires(data != null);

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="EntityList{SportDTO}"/>
        /// </summary>
        /// <returns>The created <see cref="EntityList{SportDTO}"/> instance</returns>
        public EntityList<SportDTO> Map()
        {
            if (_data.tournament == null || !_data.tournament.Any())
            {
                throw new InvalidOperationException("The provided tournamentEndpoint instance contains no tournaments");
            }

            var sports = new List<SportDTO>();
            foreach (var distinctRecord in _data.tournament.Distinct(EqualityComparer))
            {
                sports.Add(new SportDTO(
                    distinctRecord.sport.id,
                    distinctRecord.sport.name,
                    _data.tournament.Where(record => record.sport.id == distinctRecord.sport.id)));
            }
            return new EntityList<SportDTO>(new ReadOnlyCollection<SportDTO>(sports));
        }


        /// <summary>
        /// Constructs and returns a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="tournamentsEndpoint"/> instances
        /// to <see cref="EntityList{SportDTO}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="tournamentsEndpoint"/> instance containing tournaments data</param>
        /// <returns>a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="tournamentsEndpoint"/> instances
        /// to <see cref="EntityList{SportDTO}"/> instances</returns>
        internal static ISingleTypeMapper<EntityList<SportDTO>> Create(tournamentsEndpoint data)
        {
            return new TournamentsMapper(data);
        }

        internal class TournamentBySportIdEqualityComparer : IEqualityComparer<tournamentExtended>
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
                return x.sport.id == y.sport.id;
            }

            public int GetHashCode(tournamentExtended obj)
            {
                return obj?.sport.id.GetHashCode() ?? 0;
            }
        }
    }
}
