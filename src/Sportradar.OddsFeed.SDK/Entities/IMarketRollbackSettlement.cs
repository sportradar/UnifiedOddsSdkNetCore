// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    ///     Represents information for a market in <see cref="IRollbackBetSettlement{T}" /> message
    /// </summary>
    public interface IMarketRollbackSettlement : IMarketCancel
    {
        /// <summary>
        ///     An outcome for a market in <see cref="IRollbackBetSettlement{T}" /> message
        /// </summary>
        IEnumerable<IOutcomeRollbackSettlement> Outcomes { get; }
    }
}
