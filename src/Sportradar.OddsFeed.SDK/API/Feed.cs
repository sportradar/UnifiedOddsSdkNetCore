/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using App.Metrics.Formatters.Ascii;
using App.Metrics.Formatters.Json.Extensions;
using App.Metrics.Scheduling;
using Castle.Core.Internal;
using Dawn;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using Unity.Resolution;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// A <see cref="IOddsFeed"/> implementation acting as an entry point to the odds feed SDK
    /// </summary>
    public class Feed : EntityDispatcherBase, IOddsFeed, IGlobalEventDispatcher
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// A <see cref="IUnityContainer"/> used to resolve
        /// </summary>
        protected readonly IUnityContainer UnityContainer;

        /// <summary>
        /// Value indicating whether the instance has been disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Value indicating whether the current <see cref="Feed"/> is already opened
        /// 0 indicates false; 1 indicates true
        /// </summary>
        private long _opened;

        /// <summary>
        /// The <see cref="ConfiguredConnectionFactory"/> instance representing the connection to the message broker
        /// </summary>
        private ConfiguredConnectionFactory _connectionFactory;

        /// <summary>
        /// A <see cref="IList{IOpenable}"/> containing all user constructed sessions
        /// </summary>
        internal readonly IList<IOpenable> Sessions = new List<IOpenable>();

        /// <summary>
        /// The <see cref="IOddsFeedConfigurationInternal"/> representing internal sdk configuration
        /// </summary>
        internal IOddsFeedConfigurationInternal InternalConfig;

        /// <summary>
        /// Raised when the current instance of <see cref="IOddsFeed"/> loses connection to the feed
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Occurs when feed is closed
        /// </summary>
        public event EventHandler<FeedCloseEventArgs> Closed;

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        /// <summary>
        /// Occurs when a recovery initiation completes
        /// </summary>
        public event EventHandler<RecoveryInitiatedEventArgs> RecoveryInitiated;

        /// <summary>
        /// Raised when the current <see cref="IOddsFeed" /> instance determines that the <see cref="IProducer" /> associated with
        /// the odds feed went down
        /// </summary>
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        /// <summary>
        /// Raised when the current <see cref="IOddsFeed" /> instance determines that the <see cref="IProducer" /> associated with
        /// the odds feed went up (back online)
        /// </summary>
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        /// <summary>
        /// Occurs when an exception occurs in the connection loop
        /// </summary>
        public event EventHandler<ConnectionExceptionEventArgs> ConnectionException;

        /// <summary>
        /// Gets a <see cref="IEventRecoveryRequestIssuer"/> instance used to issue recovery requests to the feed
        /// </summary>
        public IEventRecoveryRequestIssuer EventRecoveryRequestIssuer
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<IEventRecoveryRequestIssuer>();
            }
        }

        /// <summary>
        /// Gets a <see cref="ISportDataProvider" /> instance used to retrieve sport related data from the feed
        /// </summary>
        /// <value>The sport data provider</value>
        public ISportDataProvider SportDataProvider
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<ISportDataProvider>();
            }
        }

        /// <summary>
        /// Gets a <see cref="IProducerManager" /> instance used to retrieve producer related data
        /// </summary>
        /// <value>The producer manager</value>
        public IProducerManager ProducerManager
        {
            get
            {
                InitFeed();
                try
                {
                    var producerManager = UnityContainer.Resolve<IProducerManager>();
                    if (InternalConfig.Environment == SdkEnvironment.Replay)
                    {
                        ((ProducerManager)producerManager).SetIgnoreRecovery(0);
                    }
                    return producerManager;
                }
                catch (Exception e)
                {
                    _log.LogError(e, "Error getting available producers.");
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IBookingManager" /> instance used to perform various booking calendar operations
        /// </summary>
        /// <value>The booking manager</value>
        public IBookingManager BookingManager
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<IBookingManager>();
            }
        }

        /// <summary>
        /// Gets the<see cref= "ICashOutProbabilitiesProvider" /> instance used to retrieve cash out probabilities for betting markets
        /// </summary>
        public ICashOutProbabilitiesProvider CashOutProbabilitiesProvider
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<ICashOutProbabilitiesProvider>();
            }
        }

        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        public IBookmakerDetails BookmakerDetails
        {
            get
            {
                InitFeed();
                return InternalConfig.BookmakerDetails;
            }
        }

        /// <summary>
        /// Gets a <see cref="IMarketDescriptionManager"/> instance used to get info about available markets, and to get translations for markets and outcomes including outrights
        /// </summary>
        public IMarketDescriptionManager MarketDescriptionManager
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<IMarketDescriptionManager>();
            }
        }

        /// <summary>
        /// Gets a <see cref="ICustomBetManager" /> instance used to perform various custom bet operations
        /// </summary>
        /// <value>The custom bet manager</value>
        public ICustomBetManager CustomBetManager
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<ICustomBetManager>();
            }
        }

        /// <summary>
        /// Gets a <see cref="IEventChangeManager"/> instance used to automatically receive fixture and result changes
        /// </summary>
        public IEventChangeManager EventChangeManager
        {
            get
            {
                InitFeed();
                return UnityContainer.Resolve<IEventChangeManager>();
            }
        }

        /// <summary>
        /// A <see cref="IFeedRecoveryManager"/> for managing recoveries and producer statuses in sessions
        /// </summary>
        private IFeedRecoveryManager _feedRecoveryManager;

        /// <summary>
        /// A <see cref="ConnectionValidator"/> used to detect potential connectivity issues
        /// </summary>
        private ConnectionValidator _connectionValidator;

        /// <summary>
        /// The feed initialized
        /// </summary>
        protected bool FeedInitialized;
        private readonly object _lockInitialized = new object();

        /// <summary>
        /// A <see cref="IMetricsRoot"/> used to provide metrics within sdk
        /// </summary>
        protected readonly IMetricsRoot MetricsRoot;
        /// <summary>
        /// A <see cref="AppMetricsTaskScheduler"/> used to schedule task to log sdk metrics
        /// </summary>
        private readonly AppMetricsTaskScheduler _metricsTaskScheduler;
        /// <summary>
        /// An <see cref="ILogger"/> used to log metrics
        /// </summary>
        private readonly ILogger _metricsLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Feed"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfiguration"/> instance representing feed configuration</param>
        /// <param name="isReplay">Value indicating whether the constructed instance will be used to connect to replay server</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> used to create <see cref="ILogger"/> used within sdk</param>
        /// <param name="metricsRoot">A <see cref="IMetricsRoot"/> used to provide metrics within sdk</param>
        protected Feed(IOddsFeedConfiguration config, bool isReplay, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
        {
            Guard.Argument(config, nameof(config)).NotNull();

            FeedInitialized = false;

            UnityContainer = new UnityContainer();
            UnityContainer.RegisterBaseTypes(config, loggerFactory, metricsRoot);
            InternalConfig = UnityContainer.Resolve<IOddsFeedConfigurationInternal>();
            if (isReplay || InternalConfig.Environment == SdkEnvironment.Replay)
            {
                InternalConfig.EnableReplayServer();
            }

            _log = SdkLoggerFactory.GetLoggerForExecution(typeof(Feed));

            LogInit();

            MetricsRoot = UnityContainer.Resolve<IMetricsRoot>();
            _metricsLogger = SdkLoggerFactory.GetLoggerForStats(typeof(Feed));
            _metricsTaskScheduler = new AppMetricsTaskScheduler(TimeSpan.FromSeconds(InternalConfig.StatisticsTimeout), async () => await LogMetricsAsync().ConfigureAwait(false));
        }

        /// <summary>
        /// Initializes the feed (unity)
        /// </summary>
        protected virtual void InitFeed()
        {
            if (FeedInitialized)
            {
                return;
            }

            lock (_lockInitialized)
            {
                if (FeedInitialized)
                {
                    return;
                }

                InternalConfig.Load(); // loads bookmaker_details
                if (InternalConfig.BookmakerDetails == null)
                {
                    _log.LogError("Token not accepted.");
                    return;
                }

                UnityContainer.RegisterTypes(this, InternalConfig);
                UnityContainer.RegisterAdditionalTypes();

                UpdateDependency();

                _feedRecoveryManager = UnityContainer.Resolve<IFeedRecoveryManager>();
                _connectionValidator = UnityContainer.Resolve<ConnectionValidator>();

                FeedInitialized = true;
            }
        }

        /// <summary>
        /// Option to change/update dependency injection before resolving resources     
        /// </summary>
        protected virtual void UpdateDependency() { }

        /// <summary>
        /// Constructs a new instance of the <see cref="Feed"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfiguration"/> instance representing feed configuration</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> used to create <see cref="ILogger"/> used within sdk</param>
        /// <param name="metricsRoot">A <see cref="IMetricsRoot"/> used to provide metrics within sdk</param>
        public Feed(IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
            : this(config, false, loggerFactory, metricsRoot)
        {
        }

        /// <summary>
        /// Marks the producer as down indicating the state of the SDK is NOT in sync with the state of the feed
        /// or that the producer associated with the producer is experiencing issues
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the <see cref="IFeedRecoveryManager"/> instance raising the event</param>
        /// <param name="e">The <see cref="ProducerStatusChangeEventArgs"/> instance containing the event data</param>
        private void MarkProducerAsDown(object sender, ProducerStatusChangeEventArgs e)
        {
            ((IGlobalEventDispatcher)this).DispatchProducerDown(e.GetProducerStatusChange());
        }

        /// <summary>
        /// Marks the producer as up indicating the state of the SDK is in sync with the state of the feed
        /// </summary>
        /// <param name="sender">The <see cref="object"/> representation of the <see cref="IFeedRecoveryManager"/> instance raising the event</param>
        /// <param name="e">The <see cref="ProducerStatusChangeEventArgs"/> instance containing the event data</param>
        private void MarkProducerAsUp(object sender, ProducerStatusChangeEventArgs e)
        {
            ((IGlobalEventDispatcher)this).DispatchProducerUp(e.GetProducerStatusChange());
        }

        private void OnCloseFeed(object sender, FeedCloseEventArgs e)
        {
            _log.LogError("Feed must be closed. Reason: " + e.GetReason());

            try
            {
                Close();
                Closed?.Invoke(this, e);
            }
            catch (ObjectDisposedException ex)
            {
                _log.LogWarning($"Error happened during closing feed, because the instance {ex.ObjectName} is being disposed.");

                if (InternalConfig.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning($"Error happened during closing feed. Exception: {ex}");

                if (InternalConfig.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            _log.LogInformation("Feed was successfully disposed.");
        }

        /// <summary>
        /// Constructs and returns a <see cref="IEnumerable{String}"/> containing routing keys for the specified session
        /// </summary>
        /// <param name="session">The <see cref="OddsFeedSession"/> for which to get the routing keys</param>
        /// <returns>The <see cref="IEnumerable{String}"/> containing routing keys for the specified session</returns>
        private IEnumerable<string> GetSessionRoutingKeys(OddsFeedSession session)
        {
            var interests = Sessions.Select(s => ((OddsFeedSession)s).MessageInterest);
            var keys = FeedRoutingKeyBuilder.GenerateKeys(interests, InternalConfig.NodeId).ToList();
            return keys[Sessions.IndexOf(session)];
        }

        /// <summary>
        /// Constructs and returns a <see cref="IOddsFeedSession"/> instance with the specified <see cref="MessageInterest"/>
        /// </summary>
        /// <param name="msgInterest">A <see cref="MessageInterest"/> specifying what messages the constructed session will be getting from the broker</param>
        /// <returns>A <see cref="IOddsFeedSession"/> instance with the specified <see cref="MessageInterest"/></returns>
        internal IOddsFeedSession CreateSession(MessageInterest msgInterest)
        {
            Guard.Argument(msgInterest, nameof(msgInterest)).NotNull();

            if (_isDisposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            if (Interlocked.Read(ref _opened) != 0)
            {
                throw new InvalidOperationException("Cannot create session associated with already opened feed");
            }

            InitFeed();
            var childContainer = UnityContainer.CreateChildContainer();
            Func<OddsFeedSession, IEnumerable<string>> func = GetSessionRoutingKeys;
            var session = (OddsFeedSession)childContainer.Resolve<IOddsFeedSession>(new ParameterOverride("messageInterest", msgInterest), new ParameterOverride("getRoutingKeys", func));
            Sessions.Add(session);
            return session;
        }

        /// <summary>
        /// Disposes the current instance and resources associated with it
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispatches the information that the connection to the feed was lost
        /// </summary>
        void IGlobalEventDispatcher.DispatchDisconnected()
        {
            Dispatch(Disconnected, EventArgs.Empty, "Disconnected", 0);
        }

        /// <summary>
        /// Dispatches the information that the requested event recovery completed
        /// <param name="requestId">The identifier of the recovery request</param>
        /// <param name="eventId">The associated event identifier</param>
        /// </summary>
        void IGlobalEventDispatcher.DispatchEventRecoveryCompleted(long requestId, URN eventId)
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();

            Dispatch(EventRecoveryCompleted, new EventRecoveryCompletedEventArgs(requestId, eventId), "EventRecoveryCompleted", 0);
        }

        /// <summary>
        /// Dispatches the information that the exception was thrown in connection loop
        /// <param name="callbackExceptionEventArgs">The information about the exception</param>
        /// </summary>
        public void DispatchConnectionException(CallbackExceptionEventArgs callbackExceptionEventArgs)
        {
            Dispatch(ConnectionException, new ConnectionExceptionEventArgs(callbackExceptionEventArgs.Exception, callbackExceptionEventArgs.Detail), "ConnectionException", 0);
        }

        /// <summary>
        /// Dispatches the <see cref="IProducerStatusChange"/> message
        /// </summary>
        /// <param name="producerStatusChange">The <see cref="IProducerStatusChange"/> instance to be dispatched</param>
        void IGlobalEventDispatcher.DispatchProducerDown(IProducerStatusChange producerStatusChange)
        {
            Guard.Argument(producerStatusChange, nameof(producerStatusChange)).NotNull();

            var eventArgs = new ProducerStatusChangeEventArgs(producerStatusChange);
            Dispatch(ProducerDown, eventArgs, "ProducerDown", producerStatusChange.Producer.Id);
        }

        /// <summary>
        /// Dispatches the <see cref="IProducerStatusChange"/> message
        /// </summary>
        /// <param name="producerStatusChange">The <see cref="IProducerStatusChange"/> instance to be dispatched</param>
        void IGlobalEventDispatcher.DispatchProducerUp(IProducerStatusChange producerStatusChange)
        {
            Guard.Argument(producerStatusChange, nameof(producerStatusChange)).NotNull();

            var eventArgs = new ProducerStatusChangeEventArgs(producerStatusChange);
            Dispatch(ProducerUp, eventArgs, "ProducerUp", producerStatusChange.Producer.Id);
        }

        /// <summary>
        /// Constructs a <see cref="IOddsFeedConfiguration"/> instance from provided information
        /// </summary>
        /// <returns>A <see cref="IOddsFeedConfiguration"/> instance created from provided information</returns>
        /// <summary>
        /// Constructs a <see cref="IOddsFeedConfiguration"/> instance from provided information
        /// </summary>
        /// <returns>A <see cref="IOddsFeedConfiguration"/> instance created from provided information</returns>
        public static ITokenSetter GetConfigurationBuilder()
        {
            return new TokenSetter(new ConfigurationSectionProvider());
        }

        /// <summary>
        /// Get all available languages that can be used within SDK and are supported on feed messages
        /// </summary>
        /// <returns>List&lt;CultureInfo&gt;</returns>
        public static IEnumerable<CultureInfo> AvailableLanguages()
        {
            var codes2 = "aa,bs,br,bg,my,zh,hr,cs,da,nl,en,et,fi,fr,ka,de,el,hi,hu,Id,ja,km,ko,lo,lv,lt,ml,ms,no,fa,pl,pt,ro,ru,sr,sk,sl,es,sw,se,th,tr,vi,it".Split(',');
            var codes3 = "sqi,zht,heb,aze,kaz,srl,ukr".Split(',');

            var all = CultureInfo.GetCultures(CultureTypes.AllCultures);
            var cultures =
                 all.Where(a => codes2.Contains(a.TwoLetterISOLanguageName))
                    .Union(all.Where(a => codes3.Contains(a.ThreeLetterISOLanguageName) && !a.Name.Contains("-")))
                    .Where(c => (c.CultureTypes & CultureTypes.NeutralCultures) != 0)
                    .OrderBy(c => c.Name)
                    .ToList();
            return cultures;
        }

        /// <summary>
        /// Constructs and returns a new instance of <see cref="IOddsFeedSessionBuilder"/>
        /// </summary>
        /// <returns>Constructed instance of the <see cref="IOddsFeedSessionBuilder"/></returns>
        public IOddsFeedSessionBuilder CreateBuilder()
        {
            return new OddsFeedSessionBuilder(this);
        }

        /// <summary>
        /// Closes the current <see cref="Feed"/> instance and disposes resources used by it
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes the current instance and resources associated with it
        /// </summary>
        /// <param name="disposing">Value indicating whether the managed resources should also be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_connectionFactory != null)
            {
                DetachFromConnectionEvents();
            }

            if (_feedRecoveryManager != null)
            {
                _feedRecoveryManager.ProducerDown -= MarkProducerAsDown;
                _feedRecoveryManager.ProducerUp -= MarkProducerAsUp;
                _feedRecoveryManager.CloseFeed -= OnCloseFeed;
                _feedRecoveryManager.EventRecoveryCompleted -= OnEventRecoveryCompleted;
                _feedRecoveryManager.Close();
            }

            _metricsTaskScheduler?.Dispose();

            foreach (var session in Sessions)
            {
                session.Close();
            }

            EventChangeManager.Stop();

            if (disposing)
            {
                try
                {
                    UnityContainer.Dispose();
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "An exception has occurred while disposing the feed instance. Exception: ");
                }
            }

            _isDisposed = true;
        }

        /// <summary>
        /// Opens the current feed by opening all created sessions
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException">
        /// The feed is already opened
        /// or
        /// The configuration is not valid
        /// </exception>
        /// <exception cref="CommunicationException">
        /// Connection to the REST-ful API failed, Probable Reason={Invalid or expired token}
        /// or
        /// Connection to the message broker failed, Probable Reason={Invalid or expired token}
        /// or
        /// </exception>
        public void Open()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            try
            {
                InitFeed();

                if (Interlocked.CompareExchange(ref _opened, 1, 0) != 0)
                {
                    throw new InvalidOperationException("The feed is already opened");
                }

                _log.LogInformation($"Feed configuration: [{InternalConfig}]");

                _connectionFactory = (ConfiguredConnectionFactory)UnityContainer.Resolve<IConnectionFactory>();
                if (_connectionFactory == null)
                {
                    throw new MissingFieldException("ConnectionFactory missing.");
                }

                AttachToConnectionEvents();

                _feedRecoveryManager.ProducerUp += MarkProducerAsUp;
                _feedRecoveryManager.ProducerDown += MarkProducerAsDown;
                _feedRecoveryManager.CloseFeed += OnCloseFeed;
                _feedRecoveryManager.EventRecoveryCompleted += OnEventRecoveryCompleted;

                ((ProducerManager)ProducerManager).Lock();

                ((ProducerManager)ProducerManager).RecoveryInitiated += OnRecoveryInitiated;

                foreach (var session in Sessions)
                {
                    session.Open();
                }

                var interests = Sessions.Select(s => ((OddsFeedSession)s).MessageInterest).ToList();
                _feedRecoveryManager.Open(interests);

                if (InternalConfig.StatisticsEnabled)
                {
                    _metricsTaskScheduler.Start();
                }

                _log.LogInformation("Producers:");
                foreach (var p in ProducerManager.Producers.OrderBy(o => o.Id))
                {
                    _log.LogInformation($"\tProducer {p.Id}-{p.Name.FixedLength(15)}\tIsAvailable={p.IsAvailable} \tIsEnabled={!p.IsDisabled}");
                }
            }
            catch (CommunicationException ex)
            {
                Interlocked.CompareExchange(ref _opened, 0, 1);

                // this should really almost never happen
                var result = _connectionValidator.ValidateConnection();
                if (result == ConnectionValidationResult.Success)
                {
                    throw new CommunicationException("Connection to the RESTful API failed, Probable Reason={Invalid or expired token}",
                                                     $"{InternalConfig.ApiBaseUri}:443",
                                                     ex.InnerException);
                }

                var publicIp = _connectionValidator.GetPublicIp();
                throw new CommunicationException($"Connection to the RESTful API failed. Probable Reason={result.Message}, Public IP={publicIp}",
                                                 $"{InternalConfig.ApiBaseUri}:443",
                                                 ex);
            }
            catch (BrokerUnreachableException ex)
            {
                Interlocked.CompareExchange(ref _opened, 0, 1);

                // this should really almost never happen
                var result = _connectionValidator.ValidateConnection();
                if (result == ConnectionValidationResult.Success)
                {
                    throw new CommunicationException("Connection to the message broker failed, Probable Reason={Invalid or expired token}",
                                                     $"{InternalConfig.Host}:{InternalConfig.Port}",
                                                     ex.InnerException);
                }

                var publicIp = _connectionValidator.GetPublicIp();
                throw new CommunicationException($"Connection to the message broker failed. Probable Reason={result.Message}, Public IP={publicIp}",
                                                 $"{InternalConfig.Host}:{InternalConfig.Port}",
                                                 ex);
            }
            catch (Exception)
            {
                Interlocked.CompareExchange(ref _opened, 0, 1);
                throw;
            }
        }

        private void AttachToConnectionEvents()
        {
            if (_connectionFactory == null)
            {
                return;
            }
            _connectionFactory.ConnectionShutdown += OnConnectionShutdown;
            _connectionFactory.CallbackException += OnCallbackException;
            _connectionFactory.RecoverySucceeded += OnRecoverySucceeded;
            _connectionFactory.ConnectionRecoveryError += OnConnectionRecoveryError;
            _connectionFactory.ConnectionBlocked += OnConnectionBlocked;
            _connectionFactory.ConnectionUnblocked += OnConnectionUnblocked;
        }

        private void DetachFromConnectionEvents()
        {
            if (_connectionFactory == null)
            {
                return;
            }
            _connectionFactory.ConnectionShutdown -= OnConnectionShutdown;
            _connectionFactory.CallbackException -= OnCallbackException;
            _connectionFactory.RecoverySucceeded -= OnRecoverySucceeded;
            _connectionFactory.ConnectionRecoveryError -= OnConnectionRecoveryError;
            _connectionFactory.ConnectionBlocked -= OnConnectionBlocked;
            _connectionFactory.ConnectionUnblocked -= OnConnectionUnblocked;
            _connectionFactory.Dispose();
        }

        /// <summary>
        /// Invoked when the connection to the message broken was shutdown
        /// </summary>
        /// <param name="sender">The connection that was shutdown</param>
        /// <param name="shutdownEventArgs">A <see cref="ShutdownEventArgs"/> containing additional event information</param>
        private void OnConnectionShutdown(object sender, ShutdownEventArgs shutdownEventArgs)
        {
            var cause = shutdownEventArgs?.Cause == null ? string.Empty : $" Cause: {shutdownEventArgs.Cause}";
            _log.LogError($"The connection is shutdown.{cause}");
            _feedRecoveryManager.ConnectionShutdown();
            ((IGlobalEventDispatcher)this).DispatchDisconnected();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            ((IGlobalEventDispatcher)this).DispatchConnectionException(e);
        }

        private void OnConnectionUnblocked(object sender, EventArgs e)
        {
            var cause = e == null || e == EventArgs.Empty ? string.Empty : $" Cause: {e}";
            _log.LogInformation($"The connection is unblocked.{cause}");
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            var cause = e == null || e.Reason.IsNullOrEmpty() ? string.Empty : $" Reason: {e.Reason}";
            _log.LogError($"The connection is blocked.{cause}");
        }

        private void OnConnectionRecoveryError(object sender, ConnectionRecoveryErrorEventArgs e)
        {
            var cause = e == null || e.Exception == null ? string.Empty : $" Exception: {e.Exception.Message}";
            _log.LogError($"The connection recovery error.{cause}");
        }

        private void OnRecoverySucceeded(object sender, EventArgs e)
        {
            var cause = e == null || e == EventArgs.Empty ? string.Empty : $" Cause: {e}";
            _log.LogInformation($"The connection recovery succeeded.{cause}");
            _feedRecoveryManager.ConnectionRecovered();
        }

        private void OnEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs e)
        {
            ((IGlobalEventDispatcher)this).DispatchEventRecoveryCompleted(e.GetRequestId(), e.GetEventId());
        }
        private void OnRecoveryInitiated(object sender, RecoveryInitiatedEventArgs e)
        {
            RecoveryInitiated?.Invoke(this, e);
        }

        private void LogInit()
        {
            var msg = "UF SDK .NET Core initialization. Version: " + SdkInfo.GetVersion();
            var logger = SdkLoggerFactory.GetLoggerForExecution(typeof(Feed));
            if (logger == null)
            {
                return;
            }
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), $"{msg}. LogLevel: {SdkLoggerFactory.GetLoggerLogLevel(logger)}");
            logger = SdkLoggerFactory.GetLoggerForCache(typeof(Feed));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), $"{msg}. LogLevel: {SdkLoggerFactory.GetLoggerLogLevel(logger)}");
            logger = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(Feed));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), $"{msg}. LogLevel: {SdkLoggerFactory.GetLoggerLogLevel(logger)}");
            logger = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(Feed));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), $"{msg}. LogLevel: {SdkLoggerFactory.GetLoggerLogLevel(logger)}");
            logger = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(Feed));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), $"{msg}. LogLevel: {SdkLoggerFactory.GetLoggerLogLevel(logger)}");
            logger = SdkLoggerFactory.GetLoggerForStats(typeof(Feed));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), $"{msg}. LogLevel: {SdkLoggerFactory.GetLoggerLogLevel(logger)}");
        }

        private Task LogMetricsAsync()
        {
            if (!InternalConfig.StatisticsEnabled)
            {
                _log.LogDebug("Metrics logging is not enabled.");
                return Task.FromResult(false);
            }
            if (MetricsRoot == null)
            {
                return Task.FromResult(true);
            }

            SdkMetricsFactory.SystemMeasures();

            var snapshot = MetricsRoot.Snapshot.Get();
            snapshot.ToMetric();

            // write to statistics log
            _metricsLogger.LogInformation("Sdk metrics:");
            var metricsFormatter = new MetricsTextOutputFormatter();
            using var stream = new MemoryStream();
            metricsFormatter.WriteAsync(stream, snapshot);
            var result = Encoding.UTF8.GetString(stream.ToArray());
            _metricsLogger.LogInformation(result);

            // call all report runners defined by user
            var tasks = MetricsRoot.ReportRunner.RunAllAsync();
            Task.WhenAll(tasks);

            return Task.FromResult(true);
        }
    }
}