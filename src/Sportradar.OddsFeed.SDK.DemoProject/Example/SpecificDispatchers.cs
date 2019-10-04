/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// SpecificDispatchers example shows how to use <see cref="ISpecificEntityDispatcher{T}"/> to differently process specific <see cref="ISportEvent"/>
    /// </summary>
    /// <seealso cref="ISpecificEntityDispatcher{T}"/>
    /// <seealso cref="SpecificEntityProcessor{T}"/>
    /// <seealso cref="IMatch"/>
    /// <seealso cref="IStage"/>
    /// <seealso cref="ITournament"/>
    /// <seealso cref="IBasicTournament"/>
    /// <seealso cref="ISeason"/>
    public class SpecificDispatchers
    {
        private readonly ILog _log;

        public SpecificDispatchers(ILog log)
        {
            _log = log;
        }

        public void Run(MessageInterest messageInterest)
        {
            Console.WriteLine(string.Empty);
            _log.Info("Running the OddsFeed SDK Specific Dispatchers example");

            var configuration = Feed.GetConfigurationBuilder().SetAccessTokenFromConfigFile().SelectIntegration().LoadFromConfigFile().Build();
            var oddsFeed = new Feed(configuration);
            AttachToFeedEvents(oddsFeed);

            _log.Info("Creating IOddsFeedSessions");

            var session = oddsFeed.CreateBuilder()
                   .SetMessageInterest(messageInterest)
                   .Build();

            _log.Info("Creating entity specific dispatchers");
            var matchDispatcher = session.CreateSportSpecificMessageDispatcher<IMatch>();
            var stageDispatcher = session.CreateSportSpecificMessageDispatcher<IStage>();
            var tournamentDispatcher = session.CreateSportSpecificMessageDispatcher<ITournament>();
            var basicTournamentDispatcher = session.CreateSportSpecificMessageDispatcher<IBasicTournament>();
            var seasonDispatcher = session.CreateSportSpecificMessageDispatcher<ISeason>();

            _log.Info("Creating event processors");
            var defaultEventsProcessor = new EntityProcessor<ISportEvent>(session);
            var matchEventsProcessor = new SpecificEntityProcessor<IMatch>(_log, matchDispatcher);
            var stageEventsProcessor = new SpecificEntityProcessor<IStage>(_log, stageDispatcher);
            var tournamentEventsProcessor = new SpecificEntityProcessor<ITournament>(_log, tournamentDispatcher);
            var basicTournamentEventsProcessor = new SpecificEntityProcessor<IBasicTournament>(_log, basicTournamentDispatcher);
            var seasonEventsProcessor = new SpecificEntityProcessor<ISeason>(_log, seasonDispatcher);

            _log.Info("Opening event processors");
            defaultEventsProcessor.Open();
            matchEventsProcessor.Open();
            stageEventsProcessor.Open();
            tournamentEventsProcessor.Open();
            basicTournamentEventsProcessor.Open();
            seasonEventsProcessor.Open();

            _log.Info("Opening the feed instance");
            oddsFeed.Open();
            _log.Info("Example successfully started. Hit <enter> to quit");
            Console.WriteLine(string.Empty);
            Console.ReadLine();

            _log.Info("Closing / disposing the feed");
            oddsFeed.Close();

            DetachFromFeedEvents(oddsFeed);

            _log.Info("Closing event processors");
            defaultEventsProcessor.Close();
            matchEventsProcessor.Close();
            stageEventsProcessor.Close();
            tournamentEventsProcessor.Close();
            basicTournamentEventsProcessor.Close();
            seasonEventsProcessor.Close();

            _log.Info("Stopped");
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="oddsFeed">A <see cref="IOddsFeed"/> instance </param>
        private void AttachToFeedEvents(IOddsFeed oddsFeed)
        {
            Contract.Requires(oddsFeed != null);

            _log.Info("Attaching to feed events");
            oddsFeed.ProducerUp += OnProducerUp;
            oddsFeed.ProducerDown += OnProducerDown;
            oddsFeed.Disconnected += OnDisconnected;
            oddsFeed.Closed += OnClosed;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="oddsFeed">A <see cref="IOddsFeed"/> instance</param>
        private void DetachFromFeedEvents(IOddsFeed oddsFeed)
        {
            Contract.Requires(oddsFeed != null);

            _log.Info("Detaching from feed events");
            oddsFeed.ProducerUp -= OnProducerUp;
            oddsFeed.ProducerDown -= OnProducerDown;
            oddsFeed.Disconnected -= OnDisconnected;
            oddsFeed.Closed -= OnClosed;
        }

        /// <summary>
        /// Invoked when the connection to the feed is lost
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnDisconnected(object sender, EventArgs e)
        {
            _log.Warn("Connection to the feed lost");
        }

        /// <summary>
        /// Invoked when the the feed is closed
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnClosed(object sender, FeedCloseEventArgs e)
        {
            _log.Warn($"The feed is closed with the reason: {e.GetReason()}");
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes down
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.Warn($"Producer {e.GetProducerStatusChange().Producer} is down");
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes up
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.Info($"Producer {e.GetProducerStatusChange().Producer} is up");
        }
    }
}
