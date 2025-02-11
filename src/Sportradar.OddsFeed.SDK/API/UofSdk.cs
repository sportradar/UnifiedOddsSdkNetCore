// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Dawn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Api
{
    /// <summary>
    /// A <see cref="IUofSdk"/> implementation acting as an entry point to the odds feed SDK
    /// </summary>
    public class UofSdk : EntityDispatcherBase, IUofSdk, IGlobalEventDispatcher
    {
        /// <summary>
        /// A service provider where all the UofSdk services are registered
        /// </summary>
        protected readonly IServiceProvider ServiceProvider;

        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// Value indicating whether the instance has been disposed
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Value indicating whether the current <see cref="IUofSdk"/> instance is already opened
        /// 0 indicates false; 1 indicates true
        /// </summary>
        private long _opened;

        /// <summary>
        /// The <see cref="ConfiguredConnectionFactory"/> instance representing the connection to the message broker
        /// </summary>
        private readonly ConfiguredConnectionFactory _connectionFactory;

        /// <summary>
        /// A <see cref="IList{IOpenable}"/> containing all user constructed sessions
        /// </summary>
        internal readonly IList<IOpenable> Sessions = new List<IOpenable>();

        /// <summary>
        /// The <see cref="UofConfiguration"/> representing sdk configuration
        /// </summary>
        internal readonly UofConfiguration UofConfig;

        /// <summary>
        /// Raised when the current instance of <see cref="IUofSdk"/> loses connection to the feed
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
        /// Raised when the current <see cref="IUofSdk" /> instance determines that the <see cref="IProducer" /> associated with
        /// the odds feed went down
        /// </summary>
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        /// <summary>
        /// Raised when the current <see cref="IUofSdk" /> instance determines that the <see cref="IProducer" /> associated with
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
        public IEventRecoveryRequestIssuer EventRecoveryRequestIssuer { get; }

        /// <summary>
        /// Gets a <see cref="ISportDataProvider" /> instance used to retrieve sport related data from the feed
        /// </summary>
        /// <value>The sport data provider</value>
        public ISportDataProvider SportDataProvider { get; }

        /// <summary>
        /// Gets a <see cref="IProducerManager" /> instance used to retrieve producer related data
        /// </summary>
        /// <value>The producer manager</value>
        public IProducerManager ProducerManager { get; }

        /// <summary>
        /// Gets a <see cref="IBookingManager" /> instance used to perform various booking calendar operations
        /// </summary>
        /// <value>The booking manager</value>
        public IBookingManager BookingManager { get; }

        /// <summary>
        /// Gets the<see cref= "ICashOutProbabilitiesProvider" /> instance used to retrieve cash out probabilities for betting markets
        /// </summary>
        public ICashOutProbabilitiesProvider CashOutProbabilitiesProvider { get; }

        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        public IBookmakerDetails BookmakerDetails { get; }

        /// <summary>
        /// Gets a <see cref="IMarketDescriptionManager"/> instance used to get info about available markets, and to get translations for markets and outcomes including outrights
        /// </summary>
        public IMarketDescriptionManager MarketDescriptionManager { get; }

        /// <summary>
        /// Gets a <see cref="ICustomBetManager" /> instance used to perform various custom bet operations
        /// </summary>
        /// <value>The custom bet manager</value>
        public ICustomBetManager CustomBetManager { get; }

        /// <summary>
        /// Gets a <see cref="IEventChangeManager"/> instance used to automatically receive fixture and result changes
        /// </summary>
        public IEventChangeManager EventChangeManager { get; }

        /// <summary>
        /// A <see cref="IFeedRecoveryManager"/> for managing recoveries and producer statuses in sessions
        /// </summary>
        private readonly IFeedRecoveryManager _feedRecoveryManager;

        /// <summary>
        /// A <see cref="ConnectionValidator"/> used to detect potential connectivity issues
        /// </summary>
        private readonly ConnectionValidator _connectionValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UofSdk"/> class
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance including UofSdk configuration</param>
        /// <param name="isReplay">Value indicating whether the constructed instance will be used to connect to replay server</param>
        protected UofSdk(IServiceProvider serviceProvider, bool isReplay)
        {
            Guard.Argument(serviceProvider, nameof(serviceProvider)).NotNull();

            ServiceProvider = serviceProvider;

            var tmpConfig = ServiceProvider.GetRequiredService<IUofConfiguration>();
            UofConfig = tmpConfig as UofConfiguration ?? GetUofConfigFromCustomConfig(tmpConfig);

            if (isReplay || UofConfig.Environment == SdkEnvironment.Replay)
            {
                UofConfig.UpdateSdkEnvironment(SdkEnvironment.Replay);
            }

            // check if ILoggerFactory is configured
            var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
            SdkLoggerFactory.SetLoggerFactory(loggerFactory ?? new NullLoggerFactory());
            _log = SdkLoggerFactory.GetLoggerForExecution(typeof(UofSdk));
            LogInit();

            _connectionFactory = ServiceProvider.GetRequiredService<ConfiguredConnectionFactory>();
            EventRecoveryRequestIssuer = ServiceProvider.GetRequiredService<IEventRecoveryRequestIssuer>();
            SportDataProvider = ServiceProvider.GetRequiredService<ISportDataProvider>();
            var producerManager = ServiceProvider.GetRequiredService<IProducerManager>();
            if (UofConfig.Environment == SdkEnvironment.Replay)
            {
                ((ProducerManager)producerManager).SetIgnoreRecovery(0);
            }
            ProducerManager = producerManager;
            BookingManager = ServiceProvider.GetRequiredService<IBookingManager>();
            CashOutProbabilitiesProvider = ServiceProvider.GetRequiredService<ICashOutProbabilitiesProvider>();
            BookmakerDetails = UofConfig.BookmakerDetails;
            MarketDescriptionManager = ServiceProvider.GetRequiredService<IMarketDescriptionManager>();
            CustomBetManager = ServiceProvider.GetRequiredService<ICustomBetManager>();
            EventChangeManager = ServiceProvider.GetRequiredService<IEventChangeManager>();
            _feedRecoveryManager = ServiceProvider.GetRequiredService<IAbstractFactory<IFeedRecoveryManager>>().Create();
            _connectionValidator = ServiceProvider.GetRequiredService<ConnectionValidator>();

            _ = UsageTelemetry.SetupUsageTelemetry(UofConfig);
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="UofSdk"/> class
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance including UofSdk configuration</param>
        public UofSdk(IServiceProvider serviceProvider)
            : this(serviceProvider, false)
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
            _log.LogError("Feed must be closed. Reason: {Reason}", e.GetReason());

            try
            {
                Close();
                Closed?.Invoke(this, e);
            }
            catch (ObjectDisposedException ex)
            {
                _log.LogWarning("Error happened during closing feed, because the instance {DisposedObject} is being disposed", ex.ObjectName);

                if (UofConfig.ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Error happened during closing feed");

                if (UofConfig.ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            _log.LogInformation("Feed was successfully disposed");
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
        void IGlobalEventDispatcher.DispatchEventRecoveryCompleted(long requestId, Urn eventId)
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();

            Dispatch(EventRecoveryCompleted, new EventRecoveryCompletedEventArgs(requestId, eventId), "EventRecoveryCompleted", 0);
        }

        /// <summary>
        /// Dispatches the information that the exception was thrown in connection loop
        /// </summary>
        /// <param name="callbackExceptionEventArgs">The information about the exception</param>
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
        /// Constructs a <see cref="IUofConfiguration"/> instance from provided information
        /// </summary>
        /// <returns>A <see cref="IUofConfiguration"/> instance created from provided information</returns>
        public static ITokenSetter GetConfigurationBuilder()
        {
            return new TokenSetter(new UofConfigurationSectionProvider(), null, null);
        }

        /// <summary>
        /// Constructs and returns a new instance of <see cref="IUofSessionBuilder"/>
        /// </summary>
        /// <returns>Constructed instance of the <see cref="IUofSessionBuilder"/></returns>
        public IUofSessionBuilder GetSessionBuilder()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(ToString());
            }

            if (Interlocked.Read(ref _opened) != 0)
            {
                throw new InvalidOperationException("Cannot create session associated with already opened feed");
            }

            return new UofSessionBuilder(ServiceProvider, Sessions, UofConfig.NodeId);
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
                if (Interlocked.CompareExchange(ref _opened, 1, 0) != 0)
                {
                    throw new InvalidOperationException("The feed is already opened");
                }

                _log.LogInformation("UofSdk configuration: [{Config}]", UofConfig);

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

                var interests = Sessions.Select(s => ((UofSession)s).MessageInterest).ToList();
                _feedRecoveryManager.Open(interests);

                LogProducerAvailability();
            }
            catch (CommunicationException ex)
            {
                Interlocked.CompareExchange(ref _opened, 0, 1);

                // this should really almost never happen
                var result = _connectionValidator.ValidateConnection();
                if (result == ConnectionValidationResult.Success)
                {
                    throw new CommunicationException("Connection to the RESTful API failed, Probable Reason={Invalid or expired token}",
                                                     $"{UofConfig.Api.BaseUrl}:443",
                                                     ex.InnerException);
                }

                var publicIp = _connectionValidator.GetPublicIp();
                throw new CommunicationException($"Connection to the RESTful API failed. Probable Reason={result.Message}, Public IP={publicIp}",
                                                 $"{UofConfig.Api.BaseUrl}:443",
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
                                                     $"{UofConfig.Rabbit.Host}:{UofConfig.Rabbit.Port}",
                                                     ex.InnerException);
                }

                var publicIp = _connectionValidator.GetPublicIp();
                throw new CommunicationException($"Connection to the message broker failed. Probable Reason={result.Message}, Public IP={publicIp}",
                                                 $"{UofConfig.Rabbit.Host}:{UofConfig.Rabbit.Port}",
                                                 ex);
            }
            catch (Exception)
            {
                Interlocked.CompareExchange(ref _opened, 0, 1);
                throw;
            }
        }

        private void LogProducerAvailability()
        {
            _log.LogInformation("Producers:");
            foreach (var p in ProducerManager.Producers.OrderBy(o => o.Id))
            {
                _log.LogInformation("Producer {ProducerId}-{ProducerName}  IsAvailable={ProducerIsAvailable} IsEnabled={ProducerIsEnabled}",
                    p.Id.ToString(CultureInfo.InvariantCulture),
                    p.Name.FixedLength(15),
                    p.IsAvailable.ToString(CultureInfo.InvariantCulture),
                    (!p.IsDisabled).ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <inheritdoc />
        public bool IsOpen()
        {
            return _opened == 1;
        }

        /// <summary>
        /// Closes the current <see cref="UofSdk"/> instance and disposes resources used by it
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes the current instance and resources associated with it
        /// </summary>
        /// <param name="disposing">Value indicating whether the managed resources should also be disposed</param>
        protected void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _opened = 0;

            if (_feedRecoveryManager != null)
            {
                _feedRecoveryManager.ProducerDown -= MarkProducerAsDown;
                _feedRecoveryManager.ProducerUp -= MarkProducerAsUp;
                _feedRecoveryManager.CloseFeed -= OnCloseFeed;
                _feedRecoveryManager.EventRecoveryCompleted -= OnEventRecoveryCompleted;
                _feedRecoveryManager.Close();
            }

            //_metricsTaskScheduler?.Dispose();

            foreach (var session in Sessions)
            {
                session.Close();
            }

            EventChangeManager.Stop();

            if (_connectionFactory != null)
            {
                DetachFromConnectionEvents();
            }

            if (disposing)
            {
                try
                {
                    //ServiceProvider.Dispose();
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex, "An exception has occurred while disposing the feed instance");
                }
            }

            _isDisposed = true;
        }

        private void AttachToConnectionEvents()
        {
            if (_connectionFactory == null)
            {
                return;
            }
            _connectionFactory.ConnectionShutdown += OnConnectionShutdown;
            _connectionFactory.CallbackException += OnCallbackException;
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
            _connectionFactory.ConnectionBlocked -= OnConnectionBlocked;
            _connectionFactory.ConnectionUnblocked -= OnConnectionUnblocked;
            _connectionFactory.CloseConnection();
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
            _log.LogError("The connection is shutdown.{Reason}", cause);
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
            _log.LogInformation("The connection is unblocked.{Reason}", cause);
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            var cause = e == null || e.Reason.IsNullOrEmpty() ? string.Empty : $" Reason: {e.Reason}";
            _log.LogError("The connection is blocked.{Reason}", cause);
        }

        private void OnEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs e)
        {
            ((IGlobalEventDispatcher)this).DispatchEventRecoveryCompleted(e.GetRequestId(), e.GetEventId());
        }
        private void OnRecoveryInitiated(object sender, RecoveryInitiatedEventArgs e)
        {
            RecoveryInitiated?.Invoke(this, e);
        }

        private static void LogInit()
        {
            var msg = "UF SDK .NET Std initialization. Version: " + SdkInfo.GetVersion();
            var logger = SdkLoggerFactory.GetLoggerForExecution(typeof(UofSdk));

            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), "{Msg}. LogLevel: {LoggerLogLevel}", msg, SdkLoggerFactory.GetLoggerLogLevel(logger));
            logger = SdkLoggerFactory.GetLoggerForCache(typeof(UofSdk));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), "{Msg}. LogLevel: {LoggerLogLevel}", msg, SdkLoggerFactory.GetLoggerLogLevel(logger));
            logger = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(UofSdk));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), "{Msg}. LogLevel: {LoggerLogLevel}", msg, SdkLoggerFactory.GetLoggerLogLevel(logger));
            logger = SdkLoggerFactory.GetLoggerForRestTraffic(typeof(UofSdk));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), "{Msg}. LogLevel: {LoggerLogLevel}", msg, SdkLoggerFactory.GetLoggerLogLevel(logger));
            logger = SdkLoggerFactory.GetLoggerForFeedTraffic(typeof(UofSdk));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), "{Msg}. LogLevel: {LoggerLogLevel}", msg, SdkLoggerFactory.GetLoggerLogLevel(logger));
            logger = SdkLoggerFactory.GetLoggerForStats(typeof(UofSdk));
            logger.Log(SdkLoggerFactory.GetWriteLogLevel(logger, LogLevel.Information), "{Msg}. LogLevel: {LoggerLogLevel}", msg, SdkLoggerFactory.GetLoggerLogLevel(logger));
        }

        private static UofConfiguration GetUofConfigFromCustomConfig(IUofConfiguration tmpConfig)
        {
            return new UofConfiguration(new UofConfigurationSectionProvider())
            {
                AccessToken = tmpConfig.AccessToken,
                Api = tmpConfig.Api,
                Additional = tmpConfig.Additional,
                BookmakerDetails = tmpConfig.BookmakerDetails,
                Cache = tmpConfig.Cache,
                DefaultLanguage = tmpConfig.DefaultLanguage,
                Languages = tmpConfig.Languages,
                Environment = tmpConfig.Environment,
                ExceptionHandlingStrategy = tmpConfig.ExceptionHandlingStrategy,
                NodeId = tmpConfig.NodeId,
                Producer = tmpConfig.Producer,
                Rabbit = tmpConfig.Rabbit,
                Usage = tmpConfig.Usage
            };
        }
    }
}
