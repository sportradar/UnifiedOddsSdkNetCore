// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a competing team
    /// </summary>
    /// <seealso cref="Competitor" />
    /// <seealso cref="ITeamCompetitor" />
    internal class TeamCompetitor : Competitor, ITeamCompetitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompetitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="TeamCompetitorCacheItem"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="TeamCompetitorCacheItem"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayer"/></param>
        /// <param name="exceptionStrategy">A <see cref="ExceptionHandlingStrategy"/> used in sport entity factory</param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="rootCompetitionCacheItem">A root <see cref="CompetitionCacheItem"/> to which this competitor belongs to</param>
        public TeamCompetitor(TeamCompetitorCacheItem ci,
                              IReadOnlyCollection<CultureInfo> cultures,
                              ISportEntityFactory sportEntityFactory,
                              ExceptionHandlingStrategy exceptionStrategy,
                              IProfileCache profileCache,
                              ICompetitionCacheItem rootCompetitionCacheItem)
            : base(ci, profileCache, cultures, sportEntityFactory, exceptionStrategy, rootCompetitionCacheItem)
        {
        }

        /// <summary>
        /// Gets the qualifier value
        /// </summary>
        public string Qualifier
        {
            get
            {
                FetchEventCompetitorsQualifiers();
                return TeamQualifier;
            }
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var res = base.PrintF() + $", Qualifier={Qualifier}";
            return res;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
