/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IDataProvider{ITournament}"/> used to retrieve list of sport available tournament
    /// </summary>
    /// <seealso cref="DataProvider{sportTournamentsEndpoint, EntityList}" />
    /// <seealso cref="IDataProvider{EntityList}" />
    public class ListSportAvailableTournamentProvider : DataProvider<sportTournamentsEndpoint, EntityList<TournamentInfoDTO>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListSportAvailableTournamentProvider"/> class
        /// </summary>
        /// <param name="baseUriFormat">An address format used to retrieve list of sport events</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{sportTournamentsEndpoint}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{sportTournamentsEndpoint, EntityList}" /> used to construct instances of <see cref="ISingleTypeMapper{ITournament}" /></param>
        public ListSportAvailableTournamentProvider(string baseUriFormat,
                                                    IDataFetcher fetcher,
                                                    IDeserializer<sportTournamentsEndpoint> deserializer,
                                                    ISingleTypeMapperFactory<sportTournamentsEndpoint, EntityList<TournamentInfoDTO>> mapperFactory)
            : base(baseUriFormat, fetcher, deserializer, mapperFactory)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(baseUriFormat));
            Contract.Requires(fetcher != null);
            Contract.Requires(deserializer != null);
            Contract.Requires(mapperFactory != null);
        }
    }
}
