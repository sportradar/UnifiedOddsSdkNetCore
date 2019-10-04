/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Profiles;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a competing team
    /// </summary>
    /// <seealso cref="Competitor" />
    /// <seealso cref="ITeamCompetitor" />
    internal class TeamCompetitor : Competitor, ITeamCompetitorV1
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamCompetitor"/> class
        /// </summary>
        /// <param name="ci">A <see cref="TeamCompetitorCI"/> used to create new instance</param>
        /// <param name="culture">A culture of the current instance of <see cref="TeamCompetitorCI"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayer"/></param>
        /// <param name="profileCache">A <see cref="IProfileCache"/> used for fetching profile data</param>
        /// <param name="rootCompetitionCI">A root <see cref="CompetitionCI"/> to which this competitor belongs to</param>
        public TeamCompetitor(TeamCompetitorCI ci,
                              IEnumerable<CultureInfo> culture,
                              ISportEntityFactory sportEntityFactory,
                              IProfileCache profileCache,
                              ICompetitionCI rootCompetitionCI)
            : base(ci, profileCache, culture, sportEntityFactory, rootCompetitionCI)
        {
            Division = ci.Division;
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
        /// Gets the division
        /// </summary>
        public int? Division { get; }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var res = base.PrintF() + $", Qualifier={Qualifier}, Division={Division}";
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
