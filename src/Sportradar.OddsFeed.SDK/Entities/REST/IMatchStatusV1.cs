// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Entities.Rest
{
    /// <summary>
    /// Defines a contract implemented by classes representing sport events of match type
    /// </summary>
    public interface IMatchStatusV1 : IMatchStatus
    {
        IMatchStatistics Statistics { get; }
    }
}
