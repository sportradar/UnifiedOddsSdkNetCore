// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to build <see cref="ITournament"/> instances
    /// </summary>
    internal interface ISportEntityFactory
    {
        /// <summary>
        /// Builds and returns a <see cref="IReadOnlyCollection{ISport}"/> representing all sports supported by the feed
        /// </summary>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<ISport>> BuildSportsAsync(IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds and returns a new instance of the <see cref="ISport"/> representing the sport specified by its id
        /// </summary>
        /// <param name="sportId">A <see cref="Urn"/> specifying the id of the sport which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{ITournament}"/> representing the asynchronous operation</returns>
        Task<ISport> BuildSportAsync(Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player or competitor profile specified by its id
        /// </summary>
        /// <param name="playerId">A <see cref="Urn"/> specifying the id of the player or competitor which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{IPlayer}"/> representing the asynchronous operation</returns>
        Task<IPlayer> BuildPlayerAsync(Urn playerId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player and/or competitor profiles specified by ids
        /// </summary>
        /// <param name="playersIds">A list of <see cref="Urn"/> specifying the ids of the players or competitors which will be represented by the constructed instances</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <param name="playersJerseyNumbers">The list of player id and associated jersey numbers</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<IPlayer>> BuildPlayersAsync(IReadOnlyCollection<Urn> playersIds, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy, IDictionary<Urn, int> playersJerseyNumbers);

        /// <summary>
        /// Builds the <see cref="ISportEvent"/> derived class based on specified id
        /// </summary>
        /// <typeparam name="T">A <see cref="ISportEvent"/></typeparam>
        /// <param name="eventId">The identifier</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">The cultures used for returned instance</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ISportEvent"/> derived instance</returns>
        T BuildSportEvent<T>(Urn eventId, Urn sportId, IReadOnlyCollection<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent;

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorCacheItem">A <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="CompetitorCacheItem"/> id used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        ICompetitor BuildCompetitor(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorCacheItem">A <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        ICompetitor BuildCompetitor(CompetitorCacheItem competitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorCacheItem">A <see cref="TeamCompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="TeamCompetitorCacheItem"/></param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCacheItem teamCompetitorCacheItem, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> of the <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="Urn"/> of the <see cref="CompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCacheItem"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        Task<ICompetitor> BuildCompetitorAsync(Urn competitorId, IReadOnlyCollection<CultureInfo> cultures, IDictionary<Urn, ReferenceIdCacheItem> competitorsReferences, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="Urn"/> of the <see cref="TeamCompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="TeamCompetitorCacheItem"/></param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        Task<ITeamCompetitor> BuildTeamCompetitorAsync(Urn teamCompetitorId, IReadOnlyCollection<CultureInfo> cultures, ICompetitionCacheItem rootCompetitionCacheItem, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds the instance of the <see cref="ICategorySummary"/> class
        /// </summary>
        /// <param name="categoryId">A <see cref="Urn"/> of the <see cref="ICategorySummary"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="ICategorySummary"/></param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        Task<ICategorySummary> BuildCategoryAsync(Urn categoryId, IReadOnlyCollection<CultureInfo> cultures);
    }
}
