/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.DemoProject.Utils;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// Example displaying interaction with Replay Server with single session, generic dispatcher, basic output
    /// </summary>
    public class ReplayServer
    {
        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;

        public ReplayServer(ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory;
            _log = _loggerFactory?.CreateLogger(typeof(ReplayServer)) ?? new NullLogger<ReplayServer>();
        }

        public void Run(MessageInterest messageInterest)
        {
            _log.LogInformation("Running the OddsFeed SDK Replay Server example");

            _log.LogInformation("Retrieving configuration from application configuration file");
            var configuration = Feed.GetConfigurationBuilder().BuildFromConfigFile();
            //you can also create the IOddsFeedConfiguration instance by providing required values
            //var configuration = Feed.CreateConfiguration("myAccessToken", new[] {"en"});

            _log.LogInformation("Creating Feed instance");
            var replayFeed = new ReplayFeed(configuration, _loggerFactory);

            _log.LogInformation("Creating IOddsFeedSession");
            var session = replayFeed.CreateBuilder()
                .SetMessageInterest(messageInterest)
                .Build();

            _log.LogInformation("Attaching to feed events");
            AttachToFeedEvents(replayFeed);
            AttachToSessionEvents(session);

            _log.LogInformation("Opening the feed instance");
            replayFeed.Open();

            ReplayServerInteraction(replayFeed);

            _log.LogInformation("Example successfully started. Hit <enter> to quit");
            Console.WriteLine(string.Empty);
            Console.ReadLine();

            _log.LogInformation("Stopping replay");
            replayFeed.ReplayManager.StopReplay();

            _log.LogInformation("Closing / disposing the feed");
            replayFeed.Close();

            DetachFromFeedEvents(replayFeed);
            DetachFromSessionEvents(session);

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
        /// Attaches to events raised by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="session">A <see cref="IOddsFeedSession"/> instance </param>
        private void AttachToSessionEvents(IOddsFeedSession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();

            _log.LogInformation("Attaching to session events");
            session.OnUnparsableMessageReceived += SessionOnUnparsableMessageReceived;
            session.OnBetCancel += SessionOnBetCancel;
            session.OnBetSettlement += SessionOnBetSettlement;
            session.OnBetStop += SessionOnBetStop;
            session.OnFixtureChange += SessionOnFixtureChange;
            session.OnOddsChange += SessionOnOddsChange;
            session.OnRollbackBetCancel += SessionOnRollbackBetCancel;
            session.OnRollbackBetSettlement += SessionOnRollbackBetSettlement;
        }

        /// <summary>
        /// Detaches from events defined by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="session">A <see cref="IOddsFeedSession"/> instance</param>
        private void DetachFromSessionEvents(IOddsFeedSession session)
        {
            Guard.Argument(session, nameof(session)).NotNull();

            _log.LogInformation("Detaching from session events");
            session.OnUnparsableMessageReceived -= SessionOnUnparsableMessageReceived;
            session.OnBetCancel -= SessionOnBetCancel;
            session.OnBetSettlement -= SessionOnBetSettlement;
            session.OnBetStop -= SessionOnBetStop;
            session.OnFixtureChange -= SessionOnFixtureChange;
            session.OnOddsChange -= SessionOnOddsChange;
            session.OnRollbackBetCancel -= SessionOnRollbackBetCancel;
            session.OnRollbackBetSettlement -= SessionOnRollbackBetSettlement;
        }

        private void SessionOnRollbackBetSettlement(object sender, RollbackBetSettlementEventArgs<ISportEvent> rollbackBetSettlementEventArgs)
        {
            var baseEntity = rollbackBetSettlementEventArgs.GetBetSettlementRollback();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
        {
            var baseEntity = rollbackBetCancelEventArgs.GetBetCancelRollback();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
        {
            var baseEntity = oddsChangeEventArgs.GetOddsChange();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
        {
            var baseEntity = fixtureChangeEventArgs.GetFixtureChange();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
        {
            var baseEntity = betStopEventArgs.GetBetStop();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
        {
            var baseEntity = betSettlementEventArgs.GetBetSettlement();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
        {
            var baseEntity = betCancelEventArgs.GetBetCancel();
            WriteSportEntity(baseEntity.GetType().Name, baseEntity.Event, baseEntity.Timestamps.Created);
        }

        private void SessionOnUnparsableMessageReceived(object sender, UnparsableMessageEventArgs unparsableMessageEventArgs)
        {
            _log.LogInformation($"{unparsableMessageEventArgs.MessageType.GetType()} message came for event {unparsableMessageEventArgs.EventId}.");
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

        private void WriteSportEntity(string msgType, ISportEvent message, long timestamp)
        {
            _log.LogDebug($"{msgType.Replace("`1", string.Empty)} message for eventId {message.Id}. Timestamp={timestamp}.");
        }

        private void ReplayServerInteraction(ReplayFeed replayFeed)
        {
            WriteReplayResponse(replayFeed.ReplayManager.StopAndClearReplay());
            var replayStatus = replayFeed.ReplayManager.GetStatusOfReplay();
            WriteReplayStatus(replayStatus);
            var queueEvents = replayFeed.ReplayManager.GetEventsInQueue();
            _log.LogInformation($"Currently {queueEvents.Count()} items in queue.");

            // there are two options:
            // play specific scenario or add specific matches to be replayed

            // option 1:
            PlayScenario(replayFeed);

            // option 2:
            //PlayMatches(replayFeed);

            replayStatus = replayFeed.ReplayManager.GetStatusOfReplay();
            WriteReplayStatus(replayStatus);
        }

        private void PlayScenario(ReplayFeed replayFeed)
        {
            replayFeed.ReplayManager.StartReplayScenario(1, 10, 1000);
            Thread.Sleep(1000);
            var queueEvents = replayFeed.ReplayManager.GetEventsInQueue();
            _log.LogInformation($"Currently {queueEvents.Count()} items in queue.");
        }

        private void PlayMatches(ReplayFeed replayFeed)
        {
            // option 1:
            // add events from sport data provider
            //foreach (var urn in SelectEventsFromSportDataProvider(replayFeed))
            //    WriteReplayResponse(replayFeed.ReplayManager.AddMessagesToReplayQueue(urn));

            // option 2:
            // add example events
            foreach (var urn in SelectExampleEvents())
            {
                WriteReplayResponse(replayFeed.ReplayManager.AddMessagesToReplayQueue(urn));
            }

            var queueEvents = replayFeed.ReplayManager.GetEventsInQueue();
            _log.LogInformation($"Currently {queueEvents.Count()} items in queue.");

            WriteReplayResponse(replayFeed.ReplayManager.StartReplay(10, 1000));
        }

        private IEnumerable<URN> SelectEventsFromSportDataProvider(ReplayFeed replayFeed)
        {
            // we can only add matches older then 48 hours
            var events = replayFeed.SportDataProvider.GetSportEventsByDateAsync(DateTime.Now.AddDays(-5)).Result.ToList();
            if (events.Count > 10)
            {
                for (var i = 0; i < 10; i++)
                {
                    yield return events[i].Id;
                }
            }
        }

        private IEnumerable<URN> SelectExampleEvents()
        {
            Console.WriteLine();
            Console.WriteLine("Sample events:");
            for (var i = 0; i < ExampleReplayEvents.SampleEvents.Count; i++)
            {
                Console.WriteLine($"{i,2} {ExampleReplayEvents.SampleEvents[i]}");
            }

            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("Select an event to add or enter 'x' if you do not want to add additional events:");
                var additionalConsoleInput = Console.ReadLine();

                if (additionalConsoleInput.Equals("x", StringComparison.CurrentCultureIgnoreCase))
                {
                    break;
                }

                if (!int.TryParse(additionalConsoleInput, out var additionalItemPosition)
                 || additionalItemPosition < 0
                 || additionalItemPosition > ExampleReplayEvents.SampleEvents.Count)
                {
                    Console.WriteLine("Invalid input, retry");
                    continue;
                }

                yield return ExampleReplayEvents.SampleEvents[additionalItemPosition].EventId;
            }
        }

        private void WriteReplayStatus(IReplayStatus status)
        {
            _log.LogInformation($"Status of replay: {status.PlayerStatus}. Last message for event: {status.LastMessageFromEvent}.");
        }

        private void WriteReplayResponse(IReplayResponse response)
        {
            _log.LogInformation($"Response of replay: {response.Success}. Message: {response.Message}");
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _log.LogInformation($"\t{response.ErrorMessage}");
            }
        }
    }
}
