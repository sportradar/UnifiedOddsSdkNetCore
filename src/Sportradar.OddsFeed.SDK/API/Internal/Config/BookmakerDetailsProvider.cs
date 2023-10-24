/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// A provider for bookmaker details
    /// </summary>
    internal class BookmakerDetailsProvider : IBookmakerDetailsProvider
    {
        private readonly IDataProvider<BookmakerDetailsDto> _bookmakerDetailsProvider;
        private readonly ILogger<BookmakerDetailsProvider> _logger;
        private readonly object _lock = new object();

        public BookmakerDetailsProvider(IDataProvider<BookmakerDetailsDto> bookmakerDetailsProvider, ILogger<BookmakerDetailsProvider> logger)
        {
            _bookmakerDetailsProvider = bookmakerDetailsProvider;
            _logger = logger;
        }

        /// <summary>
        /// Loads the current config object with bookmaker details data retrieved from the Sports API
        /// </summary>
        /// <exception cref="InvalidOperationException">The configuration could not be loaded or the configuration does not contain the requested section</exception>
        public void LoadBookmakerDetails(UofConfiguration config)
        {
            _logger.LogInformation("Loading bookmaker details from Sports API");
            lock (_lock)
            {
                if (config.BookmakerDetails != null)
                {
                    return;
                }

                if (config.Environment != SdkEnvironment.Replay)
                {
                    try
                    {
                        // first try get bookmaker details from wanted UF environment
                        LoadWhoamiData(config.Api.Host, config.Api.UseSsl, true, config.Environment, config);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to load whoami data. Environment={Enum.GetName(typeof(SdkEnvironment), config.Environment)}");

                        var wantedEnvironment = EnvironmentManager.GetSetting(config.Environment);
                        if (wantedEnvironment != null && !wantedEnvironment.EnvironmentRetryList.IsNullOrEmpty())
                        {
                            foreach (var sdkEnvironment in wantedEnvironment.EnvironmentRetryList)
                            {
                                var newSetting = EnvironmentManager.GetSetting(sdkEnvironment);
                                if (newSetting != null && !string.IsNullOrEmpty(newSetting.ApiHost))
                                {
                                    if (newSetting.OnlySsl && !config.Api.UseSsl)
                                    {
                                        _logger.LogWarning("Configuration set to not use SSL when connecting to API.");
                                    }
                                    var result = LoadWhoamiData(newSetting.ApiHost, config.Api.UseSsl, false, sdkEnvironment, config);
                                    var message = $"Access denied. The provided access token is not valid for the {sdkEnvironment} environment.";
                                    if (result)
                                    {
                                        message = $"Access granted. The provided access token is valid for the {sdkEnvironment} environment.";
                                    }
                                    _logger.LogWarning(message);
                                }
                            }
                        }

                        throw;
                    }
                }
                else
                {
                    //replay server supports both integration & production tokens, so the token must be checked against both environments
                    if (!LoadWhoamiData(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), config.Api.UseSsl, false, SdkEnvironment.Integration, config))
                    {
                        try
                        {
                            LoadWhoamiData(EnvironmentManager.GetApiHost(SdkEnvironment.Production), config.Api.UseSsl, true, SdkEnvironment.Production, config);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to load whoami data. Environment={Enum.GetName(typeof(SdkEnvironment), config.Environment)}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the whoami endpoint data
        /// </summary>
        /// <param name="hostName">The host name</param>
        /// <param name="useSsl">Value indicating whether a secure connection should be attempted</param>
        /// <param name="rethrow">Value indicating whether caught exceptions should be rethrown</param>
        /// <param name="environment">Get data for selected environment</param>
        /// <param name="config">The config to populate the received bookmaker details</param>
        /// <returns>True if data was successfully retrieved. False otherwise. May throw <see cref="CommunicationException"/></returns>
        private bool LoadWhoamiData(string hostName, bool useSsl, bool rethrow, SdkEnvironment environment, UofConfiguration config)
        {
            Guard.Argument(hostName, nameof(hostName)).NotNull().NotEmpty();

            var hostUrl = useSsl
                              ? "https://" + hostName
                              : "http://" + hostName;

            try
            {
                _logger.LogInformation($"Attempting to retrieve whoami data. Host URL={hostUrl}, Environment={Enum.GetName(typeof(SdkEnvironment), environment)}");
                var bookmakerDetailsDto = _bookmakerDetailsProvider.GetData(hostUrl);
                _logger.LogInformation($"Whoami data successfully retrieved. Host URL={hostUrl}, Environment={Enum.GetName(typeof(SdkEnvironment), environment)}");

                config.UpdateBookmakerDetails(new BookmakerDetails(bookmakerDetailsDto), hostName);

                if (config.BookmakerDetails.ServerTimeDifference > TimeSpan.FromSeconds(5))
                {
                    _logger.LogError($"Machine time is out of sync for {config.BookmakerDetails.ServerTimeDifference.TotalSeconds} sec. It may produce unwanted results with time sensitive operations within sdk.");
                }
                else if (config.BookmakerDetails.ServerTimeDifference > TimeSpan.FromSeconds(2))
                {
                    _logger.LogWarning($"Machine time is out of sync for {config.BookmakerDetails.ServerTimeDifference.TotalSeconds} sec. It may produce unwanted results with time sensitive operations within sdk.");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to retrieve whoami data. Host URL={hostUrl}, Environment={Enum.GetName(typeof(SdkEnvironment), environment)}", ex);
                if (rethrow)
                {
                    throw;
                }
                return false;
            }
        }
    }
}
