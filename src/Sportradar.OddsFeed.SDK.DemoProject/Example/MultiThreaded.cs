using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.DemoProject.Example
{
    /// <summary>
    /// Advanced Example with single session and multi-threaded message parsing
    /// </summary>
    public class MultiThreaded
    {
        private readonly BlockingCollection<EventArgs> _messages = new BlockingCollection<EventArgs>();

        private readonly ILogger _log;
        private readonly ILoggerFactory _loggerFactory;

        public MultiThreaded(ILoggerFactory loggerFactory = null)
        {
            _loggerFactory = loggerFactory;
            _log = _loggerFactory?.CreateLogger(typeof(MultiThreaded)) ?? new NullLogger<MultiThreaded>();
        }

        public void Run(MessageInterest messageInterest)
        {
            _log.LogInformation("Running the OddsFeed SDK MultiThreaded example");

            _log.LogInformation("Retrieving configuration from application configuration file");
            var configuration = Feed.GetConfigurationBuilder().BuildFromConfigFile();
            //you can also create the IOddsFeedConfiguration instance by providing required values
            //var configuration = Feed.CreateConfiguration("myAccessToken", new[] {"en"});

            _log.LogInformation("Creating Feed instance");
            var oddsFeed = new Feed(configuration, _loggerFactory);

            _log.LogInformation("Creating IOddsFeedSession");
            var session = oddsFeed.CreateBuilder()
                .SetMessageInterest(messageInterest)
                .Build();

            _log.LogInformation("Attaching to feed events");
            AttachToFeedEvents(oddsFeed);
            AttachToSessionEvents(session);

            _log.LogInformation("Opening the feed instance");
            oddsFeed.Open();

            _log.LogInformation("Starting the consumer thread");
            var thread = new Thread(ThreadStart);
            thread.Start();

            _log.LogInformation("Example successfully started. Hit <enter> to quit");
            Console.WriteLine(string.Empty);
            Console.ReadLine();

            _log.LogInformation("Closing / disposing the feed");
            thread.Abort();
            oddsFeed.Close();

            DetachFromFeedEvents(oddsFeed);
            DetachFromSessionEvents(session);

            _log.LogInformation("Stopped");
        }

        private void ThreadStart()
        {
            while (true)
            {
                var message = _messages.Take();
                _log.LogInformation($"Processing message: {message}");
            }
            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Attaches to events raised by <see cref="IOddsFeed"/>
        /// </summary>
        /// <param name="oddsFeed">A <see cref="IOddsFeed"/> instance </param>
        private void AttachToFeedEvents(IOddsFeed oddsFeed)
        {
            Contract.Requires(oddsFeed != null);

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
            Contract.Requires(oddsFeed != null);

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
            Contract.Requires(session != null);

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
            Contract.Requires(session != null);

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
            _messages.Add(rollbackBetSettlementEventArgs);
        }

        private void SessionOnRollbackBetCancel(object sender, RollbackBetCancelEventArgs<ISportEvent> rollbackBetCancelEventArgs)
        {
            _messages.Add(rollbackBetCancelEventArgs);
        }

        private void SessionOnOddsChange(object sender, OddsChangeEventArgs<ISportEvent> oddsChangeEventArgs)
        {
            _messages.Add(oddsChangeEventArgs);
        }

        private void SessionOnFixtureChange(object sender, FixtureChangeEventArgs<ISportEvent> fixtureChangeEventArgs)
        {
            _messages.Add(fixtureChangeEventArgs);
        }

        private void SessionOnBetStop(object sender, BetStopEventArgs<ISportEvent> betStopEventArgs)
        {
            _messages.Add(betStopEventArgs);
        }

        private void SessionOnBetSettlement(object sender, BetSettlementEventArgs<ISportEvent> betSettlementEventArgs)
        {
            _messages.Add(betSettlementEventArgs);
        }

        private void SessionOnBetCancel(object sender, BetCancelEventArgs<ISportEvent> betCancelEventArgs)
        {
            _messages.Add(betCancelEventArgs);
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
    }
}