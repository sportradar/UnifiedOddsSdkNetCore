// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Replay;

namespace Sportradar.OddsFeed.SDK.Api
{
    /// <summary>
    /// Represent a root object of the unified odds sdk when using Replay Server 
    /// </summary>
    public interface IUofSdkForReplay : IUofSdk
    {
        /// <summary>
        /// The replay manager for interaction with Replay Server
        /// </summary>
        IReplayManager ReplayManager { get; }
    }
}
