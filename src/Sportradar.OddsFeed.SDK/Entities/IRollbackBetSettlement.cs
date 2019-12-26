/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Defines a contract implemented by bet-settlement-rollback messages
    /// </summary>
    /// <typeparam name="T">A <see cref="ICompetition"/> derived type specifying the type of the associated sport event</typeparam>
    public interface IRollbackBetSettlement<out T> : IMarketMessage<IMarketCancel, T> where T : ISportEvent
    {
    }
}
