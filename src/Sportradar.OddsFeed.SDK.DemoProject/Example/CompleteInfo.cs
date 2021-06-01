﻿/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using Dawn;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;
using System;
using System.Globalization;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    ///<summary>
    /// A complete example using <see cref="ISpecificEntityDispatcher{T}"/> for various <see cref="ISportEvent"/> displaying all <see cref="ICompetition"/> info with Markets and Outcomes
    /// </summary>
    public class CompleteInfo
    {
        private CultureInfo _culture;

        private readonly TaskProcessor _taskProcessor = new TaskProcessor(TimeSpan.FromSeconds(20));

        private readonly IConfiguration _configuration;
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMetricsRoot _metricsRoot;

        public CompleteInfo(IConfiguration configuration, ILoggerFactory loggerFactory = null)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
            _log = _loggerFactory?.CreateLogger(typeof(CompleteInfo)) ?? new NullLogger<CompleteInfo>();

            _metricsRoot = new MetricsBuilder()
                .Configuration.Configure(
                    options =>
                    {
                        options.DefaultContextLabel = "DemoProject";
                        options.Enabled = true;
                        options.ReportingEnabled = true;
                    })
                .OutputMetrics.AsPlainText()
                .Build();
        }

        public void Run(MessageInterest messageInterest, CultureInfo culture)
        {
            Console.WriteLine(string.Empty);
            _log.LogInformation("Running the OddsFeed SDK Complete example");

            var configuration = Feed.GetConfigurationBuilder(_configuration).SetAccessTokenFromConfigFile().SelectCustom().LoadFromConfigFile().Build();
            var oddsFeed = new Feed(configuration, _loggerFactory, _metricsRoot);
            AttachToFeedEvents(oddsFeed);

            _log.LogInformation("Creating IOddsFeedSessions");

            var session = oddsFeed.CreateBuilder()
                   .SetMessageInterest(messageInterest)
                   .Build();

            _culture = culture;

            var marketWriter = new MarketWriter(_taskProcessor, _culture, _log);
            var sportEntityWriter = new SportEntityWriter(_taskProcessor, _culture, false, _log);

            _log.LogInformation("Creating entity specific dispatchers");
            var matchDispatcher = session.CreateSportSpecificMessageDispatcher<IMatch>();
            var stageDispatcher = session.CreateSportSpecificMessageDispatcher<IStage>();
            var tournamentDispatcher = session.CreateSportSpecificMessageDispatcher<ITournament>();
            var basicTournamentDispatcher = session.CreateSportSpecificMessageDispatcher<IBasicTournament>();
            var seasonDispatcher = session.CreateSportSpecificMessageDispatcher<ISeason>();

            _log.LogInformation("Creating event processors");
            var defaultEventsProcessor = new EntityProcessor<ISportEvent>(session, sportEntityWriter, marketWriter, _log);
            var matchEventsProcessor = new SpecificEntityProcessor<IMatch>(matchDispatcher, sportEntityWriter, marketWriter, _log);
            var stageEventsProcessor = new SpecificEntityProcessor<IStage>(stageDispatcher, sportEntityWriter, marketWriter, _log);
            var tournamentEventsProcessor = new SpecificEntityProcessor<ITournament>(tournamentDispatcher, sportEntityWriter, marketWriter, _log);
            var basicTournamentEventsProcessor = new SpecificEntityProcessor<IBasicTournament>(basicTournamentDispatcher, sportEntityWriter, marketWriter, _log);
            var seasonEventsProcessor = new SpecificEntityProcessor<ISeason>(seasonDispatcher, sportEntityWriter, marketWriter, _log);

            _log.LogInformation("Opening event processors");
            defaultEventsProcessor.Open();
            matchEventsProcessor.Open();
            stageEventsProcessor.Open();
            tournamentEventsProcessor.Open();
            basicTournamentEventsProcessor.Open();
            seasonEventsProcessor.Open();

            _log.LogInformation("Opening the feed instance");

            oddsFeed.Open();

            _log.LogInformation("Example successfully started. Hit <enter> to quit");
            Console.WriteLine(string.Empty);
            Console.ReadLine();

            _log.LogInformation("Closing / disposing the feed");
            oddsFeed.Close();

            DetachFromFeedEvents(oddsFeed);

            _log.LogInformation("Closing event processors");
            defaultEventsProcessor.Close();
            matchEventsProcessor.Close();
            stageEventsProcessor.Close();
            tournamentEventsProcessor.Close();
            basicTournamentEventsProcessor.Close();
            seasonEventsProcessor.Close();

            _log.LogInformation("Waiting for asynchronous operations to complete");
            var waitResult = _taskProcessor.WaitForTasks();
            _log.LogInformation($"Waiting for tasks completed. Result:{waitResult}");

            _log.LogInformation("Stopped");
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="oddsFeed">A <see cref="IOddsFeed"/> instance </param>
        private void AttachToFeedEvents(IOddsFeed oddsFeed)
        {
            Guard.Argument(oddsFeed, nameof(oddsFeed)).NotNull();

            _log.LogInformation("Attaching to feed events");
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
            Guard.Argument(oddsFeed, nameof(oddsFeed)).NotNull();

            _log.LogInformation("Detaching from feed events");
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
            _log.LogWarning("Connection to the feed lost");
        }

        /// <summary>
        /// Invoked when the feed is closed
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnClosed(object sender, FeedCloseEventArgs e)
        {
            _log.LogWarning($"The feed is closed with the reason: {e.GetReason()}");
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes down
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.LogWarning($"Producer {e.GetProducerStatusChange().Producer} is down");
        }

        /// <summary>
        /// Invoked when a product associated with the feed goes up
        /// </summary>
        /// <param name="sender">The instance raising the event</param>
        /// <param name="e">The event arguments</param>
        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.LogInformation($"Producer {e.GetProducerStatusChange().Producer} is up");
        }
    }
}
