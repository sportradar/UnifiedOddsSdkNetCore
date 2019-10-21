/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using System.Text;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Classes representing internal odds feed configuration / settings
    /// </summary>
    /// <seealso cref="IOddsFeedConfiguration" />
    internal class OddsFeedConfigurationInternal : IOddsFeedConfigurationInternal
    {
        /// <summary>
        /// A <see cref="ExecutionLog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(OddsFeedConfigurationInternal));

        /// <summary>
        /// A <see cref="IOddsFeedConfiguration"/> representing public / provided by user / configuration
        /// </summary>
        private readonly IOddsFeedConfiguration _publicConfig;

        /// <summary>
        /// A <see cref="BookmakerDetailsProvider"/> used to get bookmaker info
        /// </summary>
        private readonly BookmakerDetailsProvider _bookmakerDetailsProvider;

        /// <summary>
        /// A <see cref="object"/> used to enforce thread safety
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// A value indicating whether API config is already loaded
        /// </summary>
        private bool _apiConfigLoaded;

        /// <summary>
        /// The <see cref="IBookmakerDetails"/> instance providing detailed bookmaker info
        /// </summary>
        private IBookmakerDetails _bookmakerDetails;

        /// <summary>
        /// Value indicating whether we are connecting to the replay server
        /// </summary>
        private bool _useReplay;

        /// <summary>
        /// Gets the access token used when accessing feed's REST interface
        /// </summary>
        public string AccessToken => _publicConfig.AccessToken;

        /// <summary>
        /// Gets the maximum allowed timeout in seconds, between consecutive AMQP messages associated with the same producer.
        /// If this value is exceeded, the producer is considered to be down
        /// </summary>
        public int InactivitySeconds => _publicConfig.InactivitySeconds;

        /// <summary>
        /// Gets a <see cref="CultureInfo" /> specifying default locale to which translatable values will be translated
        /// </summary>
        public CultureInfo DefaultLocale => _publicConfig.DefaultLocale;

        /// <summary>
        /// Gets a <see cref="IEnumerable{CultureInfo}" /> specifying locales (languages) to which translatable values will be translated
        /// </summary>
        public IEnumerable<CultureInfo> Locales => _publicConfig.Locales;

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers (default: none)
        /// </summary>
        public IEnumerable<int> DisabledProducers => _publicConfig.DisabledProducers;

        /// <summary>
        /// Gets the maximum recovery time
        /// </summary>
        public int MaxRecoveryTime => _publicConfig.MaxRecoveryTime;

        /// <summary>
        /// Gets the node identifier
        /// </summary>
        public int NodeId => _publicConfig.NodeId;

        /// <summary>
        /// Gets the <see cref="SdkEnvironment"/> value specifying the environment to which to connect.
        /// </summary>
        public SdkEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the exception handling strategy
        /// </summary>
        public ExceptionHandlingStrategy ExceptionHandlingStrategy => _publicConfig.ExceptionHandlingStrategy;

        /// <summary>
        /// Gets a value specifying the host name of the AQMP broker
        /// </summary>
        public virtual string Host => _useReplay ? SdkInfo.ReplayHost : _publicConfig.Host;

        /// <summary>
        /// Gets a value specifying the virtual host of the AQMP broker
        /// </summary>
        public string VirtualHost => string.IsNullOrEmpty(_publicConfig.VirtualHost) ? _bookmakerDetails?.VirtualHost : _publicConfig.VirtualHost;

        /// <summary>
        /// Gets the user name for connecting to the AQMP broker
        /// </summary>
        public string Username => _publicConfig.Username;

        /// <summary>
        /// Gets the password for connecting to the AQMP broker
        /// </summary>
        public virtual string Password => _publicConfig.Password;

        /// <summary>
        /// Gets the port used when connecting to the AMQP broker
        /// </summary>
        public int Port => _publicConfig.Port;

        /// <summary>
        /// Gets a value specifying whether the connection to AMQP broker should use SSL encryption
        /// </summary>
        public virtual bool UseSsl => _publicConfig.UseSsl;

        /// <summary>
        /// Gets a host name of the Sports API
        /// </summary>
        public string ApiHost { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the connection to Sports API should use SSL
        /// </summary>
        public bool UseApiSsl => _publicConfig.UseApiSsl;

        /// <summary>
        /// Gets a value indicating whether the after age should be enforced before executing recovery request
        /// </summary>
        /// <value><c>true</c> if [enforce after age]; otherwise, <c>false</c></value>
        public bool AdjustAfterAge => _publicConfig.AdjustAfterAge;

        /// <summary>
        /// Gets a <see cref="string"/> representation of a Sports API base <see cref="Uri"/>
        /// </summary>
        public string ApiBaseUri => UseApiSsl ? "https://" + ApiHost : "http://" + ApiHost;

        /// <summary>
        /// Gets the URL of the feed's xReplay Server REST interface
        /// </summary>
        public string ReplayApiHost => SdkInfo.ReplayApiHost + "/v1/replay";

        /// <summary>
        /// Gets a <see cref="string"/> representation of Replay API base url
        /// </summary>
        public string ReplayApiBaseUrl => UseApiSsl ? "https://" + ReplayApiHost : "http://" + ReplayApiHost;

        /// <summary>
        /// Gets a value indication whether statistics collection is enabled
        /// </summary>
        public bool StatisticsEnabled { get; }

        /// <summary>
        /// Gets the timeout for automatically collecting statistics
        /// </summary>
        public int StatisticsTimeout { get; }

        /// <summary>
        /// Gets the limit of records for automatically writing statistics
        /// </summary>
        public int StatisticsRecordLimit { get; }

        /// <summary>
        /// Gets the bookmaker details
        /// </summary>
        /// <value>The bookmaker details</value>
        public IBookmakerDetails BookmakerDetails => _bookmakerDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="OddsFeedConfigurationInternal"/> class
        /// </summary>
        /// <param name="publicConfig">A <see cref="IOddsFeedConfiguration"/> representing public / provided by user / configuration</param>
        /// <param name="bookmakerDetailsProvider">A <see cref="BookmakerDetailsProvider"/> used to get bookmaker info</param>
        public OddsFeedConfigurationInternal(IOddsFeedConfiguration publicConfig, BookmakerDetailsProvider bookmakerDetailsProvider)
        {
            Guard.Argument(publicConfig).NotNull();
            Guard.Argument(bookmakerDetailsProvider).NotNull();

            _publicConfig = publicConfig;
            _bookmakerDetailsProvider = bookmakerDetailsProvider;

            StatisticsEnabled = true;
            StatisticsTimeout = 1800;
            StatisticsRecordLimit = 1000000;
            var sdkConfig = _publicConfig as OddsFeedConfiguration;
            if (sdkConfig?.Section != null)
            {
                StatisticsEnabled = sdkConfig.Section.StatisticsEnabled;
                StatisticsTimeout = sdkConfig.Section.StatisticsTimeout;
                StatisticsRecordLimit = sdkConfig.Section.StatisticsRecordLimit;
            }
            Environment = publicConfig.Environment;
            _useReplay = _publicConfig.Environment == SdkEnvironment.Replay;
        }

        /// <summary>
        /// Loads the whoami endpoint data
        /// </summary>
        /// <param name="hostName">The host name</param>
        /// <param name="useSsl">Value indicating whether a secure connection should be attempted</param>
        /// <param name="rethrow">Value indicating whether caught exceptions should be rethrown</param>
        /// <returns>True if data was successfully retrieved. False otherwise. May throw <see cref="CommunicationException"/></returns>
        private bool LoadWhoamiData(string hostName, bool useSsl, bool rethrow)
        {
            Guard.Argument(!string.IsNullOrEmpty(hostName));

            var hostUrl = useSsl
                              ? "https://" + hostName
                              : "http://" + hostName;

            try
            {
                ExecutionLog.Info($"Attempting to retrieve whoami data. Host URL={hostUrl}, Environment={Enum.GetName(typeof(SdkEnvironment), Environment)}");
                var bookmakerDetailsDTO = _bookmakerDetailsProvider.GetData(hostUrl);
                _bookmakerDetails = new BookmakerDetails(bookmakerDetailsDTO);
                ApiHost = hostName;
                ExecutionLog.Info($"Whoami data successfully retrieved. Host URL={hostUrl}, Environment={Enum.GetName(typeof(SdkEnvironment), Environment)}");

                if (_bookmakerDetails.ServerTimeDifference > TimeSpan.FromSeconds(5))
                {
                    ExecutionLog.Error($"Machine time is out of sync for {_bookmakerDetails.ServerTimeDifference.TotalSeconds} sec. It may produce unwanted results with time sensitive operations within sdk.");
                }
                else if (_bookmakerDetails.ServerTimeDifference > TimeSpan.FromSeconds(2))
                {
                    ExecutionLog.Warn($"Machine time is out of sync for {_bookmakerDetails.ServerTimeDifference.TotalSeconds} sec. It may produce unwanted results with time sensitive operations within sdk.");
                }

                return true;
            }
            catch (Exception ex)
            {
                ExecutionLog.Info($"Failed to retrieve whoami data. Host URL={hostUrl}, Environment={Enum.GetName(typeof(SdkEnvironment), Environment)}, Error message={ex.Message}");
                if (rethrow)
                {
                    throw;
                }
                return false;
            }
        }

        /// <summary>
        /// Loads the current config object with data retrieved from the Sports API
        /// </summary>
        public void Load()
        {
            ExecutionLog.Info("Loading config info from Sports API");
            lock (_lock)
            {
                if (_apiConfigLoaded)
                {
                    throw new InvalidOperationException("The API configuration is already loaded");
                }
                if (Environment != SdkEnvironment.Replay)
                {
                    try
                    {
                        // we have 3 options: production, integration and custom host
                        LoadWhoamiData(_publicConfig.ApiHost, UseApiSsl, true);
                    }
                    catch (Exception ex)
                    {
                        if (!_publicConfig.ApiHost.Equals(SdkInfo.IntegrationApiHost, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (LoadWhoamiData(SdkInfo.IntegrationApiHost, UseApiSsl, false))
                            {
                                var message = $"Access denied. The provided access token is for the Integration environment but the SDK is configured to access the {_publicConfig.Environment} environment.";
                                ExecutionLog.Error(message);
                                throw new InvalidOperationException(message, ex);
                            }
                        }
                        if (!_publicConfig.ApiHost.Equals(SdkInfo.ProductionApiHost, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (LoadWhoamiData(SdkInfo.ProductionApiHost, UseApiSsl, false))
                            {
                                var message = $"Access denied. The provided access token is for the Production environment but the SDK is configured to access the {_publicConfig.Environment} environment.";
                                ExecutionLog.Error(message);
                                throw new InvalidOperationException(message, ex);
                            }
                        }
                        ExecutionLog.Error($"Failed to load whoami data. Environment={Enum.GetName(typeof(SdkEnvironment), Environment)}");
                        throw;
                    }
                }

                //replay server supports both integration & production tokens, so the token must be checked against both environments
                else
                {
                    if (!LoadWhoamiData(SdkInfo.IntegrationApiHost, UseApiSsl, false))
                    {
                        try
                        {
                            LoadWhoamiData(SdkInfo.ProductionApiHost, UseApiSsl, true);
                        }
                        catch (Exception)
                        {
                            ExecutionLog.Error($"Failed to load whoami data. Environment={Enum.GetName(typeof(SdkEnvironment), Environment)}");
                        }
                    }
                }
                _apiConfigLoaded = true;
            }
        }

        /// <summary>
        /// Indicates that the SDK will be used to connect to Replay Server
        /// </summary>
        public void EnableReplayServer()
        {
            if (_apiConfigLoaded)
            {
                throw new InvalidOperationException("Replay server cannot be enabled once the API configuration is loaded.");
            }

            _useReplay = true;
            Environment = SdkEnvironment.Replay;
        }

        public override string ToString()
        {
            var locales = Locales != null && Locales.Any()
                ? string.Join(",", Locales.Select(s => s.TwoLetterISOLanguageName))
                : string.Empty;
            var disabledProducers = DisabledProducers != null && DisabledProducers.Any()
                ? string.Join(",", DisabledProducers)
                : string.Empty;
            var token = !string.IsNullOrEmpty(AccessToken) && AccessToken.Length > 3
                            ? AccessToken.Substring(0, 3) + "***" + AccessToken.Substring(AccessToken.Length - 3)
                            : AccessToken;

            //var server = _useReplay
            //    ? $"Host={Host}, VirtualHost={VirtualHost}, Port={Port}, UseSsl={UseSsl}, UseApiSsl={UseApiSsl}, Username={Username}, Password={Password}, NodeId={NodeId}"
            //    : $"Host={ReplayApiBaseUrl}, VirtualHost={VirtualHost}, Port={Port}, UseSsl={UseSsl}, UseApiSsl={UseApiSsl}, Username={Username}, Password={Password}, NodeId={NodeId}";
            var sb = new StringBuilder();
            sb.Append("AccessToken=").Append(token)
              .Append(" Username=").Append(Username == AccessToken ? token : Username)
              .Append(" Password=").Append(Password)
              .Append(" NodeId=").Append(NodeId)
              .Append(" BookmakerId=").Append(_bookmakerDetails?.BookmakerId)
              .Append(" VirtualHost=").Append(_bookmakerDetails?.VirtualHost)
              .Append(" TokenExpires=").Append(_bookmakerDetails?.ExpireAt.ToShortDateString())
              .Append(" InactivitySeconds=").Append(InactivitySeconds)
              .Append(" DefaultLocale=").Append(DefaultLocale)
              .Append(" Locales=[").Append(locales).Append("]")
              .Append(" DisabledProducers=").Append(disabledProducers)
              .Append(" MaxRecoveryTime=").Append(MaxRecoveryTime)
              .Append(" Environment=").Append(Environment)
              .Append(" ExceptionHandlingStrategy=").Append(ExceptionHandlingStrategy)
              .Append(" UseReplayServer=").Append(_useReplay)
              .Append(" ApiHost=").Append(ApiHost)
              .Append(" Host=").Append(Host)
              .Append(" Port=").Append(Port)
              .Append(" UseSsl=").Append(UseSsl)
              .Append(" UseApiSsl=").Append(UseApiSsl)
              .Append(" ExceptionStrategy=").Append(ExceptionHandlingStrategy)
              .Append(" AdjustAfterAge=").Append(AdjustAfterAge);

            return sb.ToString();
        }
    }
}
