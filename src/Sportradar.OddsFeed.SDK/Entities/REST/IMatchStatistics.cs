// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    ///     Defines a contract for classes implementing match specific statistics
    /// </summary>
    /// <seealso cref="ICompetitionStatistics" />
    public interface IMatchStatistics : ICompetitionStatistics
    {
        IEnumerable<ITeamStatistics> TotalStatistics { get; }

        IEnumerable<IPeriodStatistics> PeriodStatistics { get; }
    }
}
