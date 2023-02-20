/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.Internal.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// FeedRecoveryManager takes care of initial snapshot recovery
    /// </summary>
    internal class FeedRecoveryManager : IFeedRecoveryManager
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLogger(typeof(FeedRecoveryManager));

        /// <summary>
        /// Delay in seconds specifying how much after the start-up the trackers are checked
        /// </summary>
        private const int TrackersInitialCheckDelaySeconds = 1;

        /// <summary>
        /// Specifies a period in seconds between consequential fetcher checks
        /// </summary>
        private const int TrackersCheckPeriodSeconds = 10;

        /// <summary>
        /// A <see cref="IProducerRecoveryManagerFactory"/> instance used to construct <see cref="IProducerRecoveryManager"/> instances
        /// </summary>
        private readonly IProducerRecoveryManagerFactory _producerRecoveryManagerFactory;

        /// <summary>
        /// A <see cref="List{ISessionMessageManager}"/> containing managers monitoring user's sessions
        /// </summary>
        private readonly List<ISessionMessageManager> _sessionMessageManagers = new List<ISessionMessageManager>();

        /// <summary>
        /// A <see cref="ITimer"/> used to track timeouts between alive messages
        /// </summary>
        private readonly ITimer _inactivityTimer;

        /// <summary>
        /// A value used to store information whether the current <see cref="OddsFeedSession"/> is opened
        /// 0 indicate closed, 1 indicate opened
        /// </summary>
        private long _isOpened;

        /// <summary>
        /// Gets a value indicating whether the current <see cref="OddsFeedSession"/> is opened
        /// </summary>
        public bool IsOpened => Interlocked.Read(ref _isOpened) == 1;

        /// <summary>
        /// List of <see cref="IProducerRecoveryManager"/> instances where each instance is assigned to one producer
        /// </summary>
        private IDictionary<IProducer, IProducerRecoveryManager> _producerRecoveryManagers = new Dictionary<IProducer, IProducerRecoveryManager>();

        /// <summary>
        /// Occurs when the specific <see cref="IProducer"/> is marked as up indicating the state of the SDK is synchronized
        /// with the state of the feed
        /// </summary>
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        /// <summary>
        /// Occurs when the specific <see cref="IProducer"/> is marked as down indicating the state of the SDK is NOT synchronized
        /// with the state of the feed, or the associated producer is experiencing problems
        /// </summary>
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        /// <summary>
        /// Occurs when the specific <see cref="IProducerRecoveryManager"/> is marked as <see cref="ProducerRecoveryStatus.FatalError"/> indicating the recovery request could not be invoked
        /// </summary>
        public event EventHandler<FeedCloseEventArgs> CloseFeed;

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        /// <summary>
        /// The <see cref="IProducerManager"/> with all available <see cref="IProducer"/>
        /// </summary>
        private readonly IProducerManager _producerManager;

        /// <summary>
        /// The system feed session for processing alive messages
        /// </summary>
        private readonly IFeedSystemSession _systemSession;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedRecoveryManager"/> class
        /// </summary>
        /// <param name="producerRecoveryManagerFactory">A <see cref="IProducerRecoveryManagerFactory"/> used to create new <see cref="IProducerRecoveryManager"/></param>
        /// <param name="config">A <see cref="IOddsFeedConfigurationInternal"/> used on feed</param>
        /// <param name="timer">A <see cref="ITimer"/> used for invocation of period tasks</param>
        /// <param name="producerManager">The <see cref="IProducerManager"/> with all available <see cref="IProducer"/></param>
        /// <param name="systemSession">The <see cref="IFeedSystemSession"/> for processing alive messages</param>
        public FeedRecoveryManager(IProducerRecoveryManagerFactory producerRecoveryManagerFactory,
                                   IOddsFeedConfiguration config,
                                   ITimer timer,
                                   IProducerManager producerManager,
                                   IFeedSystemSession systemSession)
        {
            Guard.Argument(producerRecoveryManagerFactory, nameof(producerRecoveryManagerFactory)).NotNull();
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(timer, nameof(timer)).NotNull();
            Guard.Argument(producerManager, nameof(producerManager)).NotNull();
            Guard.Argument(systemSession, nameof(systemSession)).NotNull();

            _producerRecoveryManagerFactory = producerRecoveryManagerFactory;
            _inactivityTimer = timer;
            _producerManager = producerManager;
            _systemSession = systemSession;
        }

        /// <summary>
        /// Invoked when the timer used to check the status of trackers is invoked
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnTimerElapsed(object source, EventArgs e)
        {
            if (!IsOpened)
            {
                return;
            }

            foreach (var recoveryTracker in _producerRecoveryManagers.Values)
            {
                recoveryTracker.CheckStatus();
            }

            try
            {
                if (_isOpened != 0)
                {
                    _inactivityTimer.FireOnce(TimeSpan.FromSeconds(TrackersCheckPeriodSeconds));
                }
            }
            catch (ObjectDisposedException ex)
            {
                _executionLog.LogInformation($"Error happened during invoking timer, because the instance {ex.ObjectName} is being disposed.");
            }
        }

        private void OnSystemSessionMessageReceived(object sender, FeedMessageReceivedEventArgs feedMessageReceivedEventArgs)
        {
            var manager = _producerRecoveryManagers.FirstOrDefault(f => f.Key.Id == feedMessageReceivedEventArgs.Message.ProducerId).Value;
            manager?.ProcessSystemMessage(feedMessageReceivedEventArgs.Message);
        }

        private void OnUserSessionMessageReceived(object sender, FeedMessageReceivedEventArgs feedMessageReceivedEventArgs)
        {
            var manager = _producerRecoveryManagers.FirstOrDefault(f => f.Key.Id == feedMessageReceivedEventArgs.Message.ProducerId).Value;
            manager?.ProcessUserMessage(feedMessageReceivedEventArgs.Message, feedMessageReceivedEventArgs.Interest);
        }

        /// <summary>
        /// Handles the change in the status of a specific recovery manager
        /// </summary>
        /// <param name="sender">An <see cref="object"/> representation of a <see cref="IProducerRecoveryManager"/> instance whose status has changed.</param>
        /// <param name="e">The <see cref="TrackerStatusChangeEventArgs"/>Additional information about the event.</param>
        private void OnRecoveryTrackerStatusChanged(object sender, TrackerStatusChangeEventArgs e)
        {
            var tracker = (IProducerRecoveryManager)sender;
            if (e.NewStatus == ProducerRecoveryStatus.Started)
            {
                foreach (var sessionMessageManager in _sessionMessageManagers)
                {
                    sessionMessageManager.StashMessages(tracker.Producer, e.RequestId.GetValueOrDefault(0));
                }
            }
            if (e.NewStatus == ProducerRecoveryStatus.Completed || e.NewStatus == ProducerRecoveryStatus.Delayed)
            {
                foreach (var sessionMessageManager in _sessionMessageManagers)
                {
                    sessionMessageManager.ReleaseMessages(tracker.Producer, e.RequestId.GetValueOrDefault(0));
                }
            }
            if (e.NewStatus == ProducerRecoveryStatus.FatalError)
            {
                _inactivityTimer.Stop();
                CloseFeed?.Invoke(this, new FeedCloseEventArgs("Automatic recovery cannot be invoked."));
            }

            if (e.NewStatus == ProducerRecoveryStatus.Completed && e.OldStatus != ProducerRecoveryStatus.Completed)
            {
                var statusChange = new ProducerStatusChange(SdkInfo.ToEpochTime(TimeProviderAccessor.Current.Now), tracker.Producer);
                ProducerUp?.Invoke(this, new ProducerStatusChangeEventArgs(statusChange));
            }
            else if (e.NewStatus != ProducerRecoveryStatus.Completed && e.OldStatus == ProducerRecoveryStatus.Completed)
            {
                var statusChange = new ProducerStatusChange(SdkInfo.ToEpochTime(TimeProviderAccessor.Current.Now), tracker.Producer);
                ProducerDown?.Invoke(this, new ProducerStatusChangeEventArgs(statusChange));
            }
        }

        /// <summary>
        /// Handles the event recovery completion of a specific recovery manager
        /// </summary>
        /// <param name="sender">An <see cref="object"/> representation of a <see cref="IProducerRecoveryManager"/> instance whose status has changed.</param>
        /// <param name="e">The <see cref="EventRecoveryCompletedEventArgs"/>Additional information about the event.</param>
        private void OnRecoveryTrackerEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs e)
        {
            EventRecoveryCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Opens the current instance
        /// </summary>
        /// <param name="interests">The interests for which to open trackers</param>
        public void Open(ICollection<MessageInterest> interests)
        {
            Guard.Argument(interests, nameof(interests)).NotNull().NotEmpty();

            var interestList = interests as List<MessageInterest> ?? interests.ToList();

            // the LINQ query below might produce a list with same entries (one producer found more than once)
            var producersToOpen = from producer in _producerManager.Producers
                                  where !producer.IsDisabled && producer.IsAvailable
                                  let producerScopes = ((Producer)producer).Scope.Select(MessageInterest.FromScope).ToList()
                                  from interest in interestList
                                  where !interest.IsScopeInterest || producerScopes.Contains(interest)
                                  select producer;

            var producerRecoveryManagers = producersToOpen.Distinct(new ProducerEqualityComparer()).Select(p => _producerRecoveryManagerFactory.GetRecoveryTracker(p, interestList)).ToList();

            if (!producerRecoveryManagers.Any())
            {
                throw new InvalidOperationException($"Message interests [{string.Join(", ", interestList)}] cannot be used. There are no suitable active producers.");
            }

            Open(producerRecoveryManagers);

            _systemSession.AliveReceived += OnSystemSessionMessageReceived;
            _systemSession.Open();
        }

        private void Open(IReadOnlyCollection<IProducerRecoveryManager> trackers)
        {
            if (trackers == null || !trackers.Any())
            {
                throw new InvalidOperationException("Missing trackers");
            }

            if (Interlocked.CompareExchange(ref _isOpened, 1, 0) == 1)
            {
                throw new InvalidOperationException("Current FeedRecoveryManager is already opened");
            }

            foreach (var recoveryTracker in trackers)
            {
                recoveryTracker.StatusChanged += OnRecoveryTrackerStatusChanged;
                recoveryTracker.EventRecoveryCompleted += OnRecoveryTrackerEventRecoveryCompleted;
            }

            _producerRecoveryManagers = trackers.ToDictionary(t => t.Producer, t => t);

            _inactivityTimer.Elapsed += OnTimerElapsed;

            foreach (var sessionMessageManager in _sessionMessageManagers)
            {
                sessionMessageManager.FeedMessageReceived += OnUserSessionMessageReceived;
            }

            _inactivityTimer.FireOnce(TimeSpan.FromSeconds(TrackersInitialCheckDelaySeconds));
        }

        /// <summary>
        /// Closes the current <see cref="FeedRecoveryManager"/> so it will stop checking for recovery requests
        /// </summary>
        /// <remarks>The <see cref="Close"/> method does not dispose resources associated with the current instance so the instance can be re-opened by calling the Open() method. In order to dispose resources associated
        /// with the current instance you must call the <see cref="IDisposable.Dispose"/> method. Once the instance is disposed it can no longer be opened.</remarks>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _isOpened, 0, 1) == 0)
            {
                return;
            }

            if (_inactivityTimer != null)
            {
                _inactivityTimer.Stop();
                _inactivityTimer.Elapsed -= OnTimerElapsed;
            }

            if (_systemSession != null)
            {
                _systemSession.Close();
                _systemSession.AliveReceived -= OnSystemSessionMessageReceived;
            }

            foreach (var recoveryTracker in _producerRecoveryManagers)
            {
                recoveryTracker.Value.StatusChanged -= OnRecoveryTrackerStatusChanged;
                recoveryTracker.Value.EventRecoveryCompleted -= OnRecoveryTrackerEventRecoveryCompleted;
            }

            foreach (var sessionMessageManager in _sessionMessageManagers)
            {
                sessionMessageManager.FeedMessageReceived -= OnUserSessionMessageReceived;
            }
        }

        /// <summary>
        /// Creates new <see cref="ISessionMessageManager"/>
        /// </summary>
        /// <returns>Newly created <see cref="ISessionMessageManager"/></returns>
        public ISessionMessageManager CreateSessionMessageManager()
        {
            if (Interlocked.Read(ref _isOpened) != 0)
            {
                throw new InvalidOperationException("Cannot create session associated with already opened feedRecoveryManager");
            }

            var sessionMessageManager = _producerRecoveryManagerFactory.CreateSessionMessageManager();
            _sessionMessageManagers.Add(sessionMessageManager);
            return sessionMessageManager;
        }

        /// <summary>
        /// Executes the steps required when the connection to the message broker is shutdown.
        /// </summary>
        public void ConnectionShutdown()
        {
            foreach (var producerRecoveryManager in _producerRecoveryManagers.Values)
            {
                producerRecoveryManager.ConnectionShutdown();
            }
        }

        /// <summary>
        /// Executes the steps required when the connection to the message broker is recovered.
        /// </summary>
        public void ConnectionRecovered()
        {
            foreach (var producerRecoveryManager in _producerRecoveryManagers.Values)
            {
                producerRecoveryManager.ConnectionRecovered();
            }
        }
    }
}
