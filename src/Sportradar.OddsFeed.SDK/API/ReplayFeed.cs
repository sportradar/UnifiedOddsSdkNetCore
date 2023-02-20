/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API.Internal;
using Unity;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// A <see cref="IOddsFeed"/> implementation acting as an entry point to the odds feed Replay Service for doing integration tests against played matches that are older than 48 hours
    /// </summary>
    public class ReplayFeed : Feed
    {
        /// <summary>
        /// The replay manager for interaction with xReplay Server
        /// </summary>
        public IReplayManager ReplayManager
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<IReplayManager>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayFeed"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfiguration" /> instance representing feed configuration.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> used to create <see cref="ILogger"/> used within sdk</param>
        /// <param name="metricsRoot">A <see cref="IMetricsRoot"/> used to provide metrics within sdk</param>
        public ReplayFeed(IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
            : base(config, true, loggerFactory, metricsRoot)
        {
        }

        /// <inheritdoc />
        protected override void InitFeed()
        {
            if (FeedInitialized)
            {
                return;
            }
            base.InitFeed();
            ((ProducerManager)ProducerManager).SetIgnoreRecovery(0);
        }
    }
}
