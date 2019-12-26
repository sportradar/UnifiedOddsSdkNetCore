/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to build <see cref="ITournament"/> instances
    /// </summary>
    public interface ISportEntityFactory
    {
        /// <summary>
        /// Builds and returns a <see cref="IEnumerable{ISport}"/> representing all sports supported by the feed
        /// </summary>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<ISport>> BuildSportsAsync(IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds and returns a new instance of the <see cref="ISport"/> representing the sport specified by its id
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the sport which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>A <see cref="Task{ITournament}"/> representing the asynchronous operation</returns>
        Task<ISport> BuildSportAsync(URN id, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy);

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player or competitor profile specified by its id
        /// </summary>
        /// <param name="id">A <see cref="URN"/> specifying the id of the player or competitor which will be represented by the constructed instance</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <returns>A <see cref="Task{IPlayer}"/> representing the asynchronous operation</returns>
        Task<IPlayer> BuildPlayerAsync(URN id, IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Builds and returns a new instance of the <see cref="IPlayer"/> representing the player and/or competitor profiles specified by ids
        /// </summary>
        /// <param name="ids">A list of <see cref="URN"/> specifying the ids of the players or competitors which will be represented by the constructed instances</param>
        /// <param name="cultures">A list of all supported languages</param>
        /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation</returns>
        Task<IEnumerable<IPlayer>> BuildPlayersAsync(IEnumerable<URN> ids, IEnumerable<CultureInfo> cultures);

        /// <summary>
        /// Builds the <see cref="ISportEvent"/> derived class based on specified id
        /// </summary>
        /// <typeparam name="T">A <see cref="ISportEvent"/></typeparam>
        /// <param name="id">The identifier</param>
        /// <param name="sportId">The sport identifier</param>
        /// <param name="cultures">The cultures used for returned instance</param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> enum member specifying how the build instance will handle potential exceptions</param>
        /// <returns>The constructed <see cref="ISportEvent"/> derived instance</returns>
        T BuildSportEvent<T>(URN id, URN sportId, IEnumerable<CultureInfo> cultures, ExceptionHandlingStrategy exceptionStrategy) where T : ISportEvent;

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        ICompetitor BuildCompetitor(CompetitorCI ci, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        ICompetitor BuildCompetitor(CompetitorCI ci, IEnumerable<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences);

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="culture">A culture of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        ITeamCompetitor BuildTeamCompetitor(TeamCompetitorCI ci, IEnumerable<CultureInfo> culture, ICompetitionCI rootCompetitionCI);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IEnumerable<CultureInfo> cultures, ICompetitionCI rootCompetitionCI);

        /// <summary>
        /// Builds the instance of the <see cref="ICompetitor"/> class
        /// </summary>
        /// <param name="competitorId">A <see cref="URN"/> of the <see cref="CompetitorCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="CompetitorCI"/></param>
        /// <param name="competitorsReferences">The dictionary of competitor references (associated with specific match)</param>
        /// <returns>The constructed <see cref="ICompetitor"/> instance</returns>
        Task<ICompetitor> BuildCompetitorAsync(URN competitorId, IEnumerable<CultureInfo> cultures, IDictionary<URN, ReferenceIdCI> competitorsReferences);

        /// <summary>
        /// Builds the instance of the <see cref="ITeamCompetitor"/> class
        /// </summary>
        /// <param name="teamCompetitorId">A <see cref="URN"/> of the <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="culture">A culture of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        /// <returns>The constructed <see cref="ITeamCompetitor"/> instance</returns>
        Task<ITeamCompetitor> BuildTeamCompetitorAsync(URN teamCompetitorId, IEnumerable<CultureInfo> culture, ICompetitionCI rootCompetitionCI);
    }
}
