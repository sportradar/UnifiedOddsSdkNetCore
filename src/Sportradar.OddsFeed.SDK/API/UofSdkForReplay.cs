// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.DependencyInjection;
using Sportradar.OddsFeed.SDK.Api.Replay;

namespace Sportradar.OddsFeed.SDK.Api
{
    /// <summary>
    /// A <see cref="IUofSdk"/> implementation acting as an entry point to the odds feed Replay Service for doing integration tests against played matches that are older than 48 hours
    /// </summary>
    public class UofSdkForReplay : UofSdk, IUofSdkForReplay
    {
        /// <summary>
        /// The replay manager for interaction with xReplay Server
        /// </summary>
        public IReplayManager ReplayManager { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UofSdkForReplay"/> class
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance including UofSdk configuration</param>
        public UofSdkForReplay(IServiceProvider serviceProvider)
            : base(serviceProvider, true)
        {
            ReplayManager = ServiceProvider.GetRequiredService<IReplayManager>();
        }
    }
}
