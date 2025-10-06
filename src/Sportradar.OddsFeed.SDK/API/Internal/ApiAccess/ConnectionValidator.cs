// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// Used to validate connection to the message broker
    /// </summary>
    internal class ConnectionValidator
    {
        /// <summary>
        /// A <see cref="IUofConfiguration"/> instance representing odds configuration
        /// </summary>
        private readonly IUofConfiguration _config;

        /// <summary>
        /// A <see cref="IDataFetcher"/> instance used to execute http requests
        /// </summary>
        private readonly IDataFetcher _dataFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionValidator"/> class.
        /// </summary>
        /// <param name="config">A <see cref="IUofConfiguration"/> instance representing odds configuration.</param>
        /// <param name="dataFetcher">A <see cref="IDataFetcher"/> instance used to execute http requests</param>
        public ConnectionValidator(IUofConfiguration config, IDataFetcher dataFetcher)
        {
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(dataFetcher, nameof(dataFetcher)).NotNull();

            _config = config;
            _dataFetcher = dataFetcher;
        }

        /// <summary>
        /// Validates connection to the message broker
        /// </summary>
        /// <returns>A <see cref="ConnectionValidationResult"/> enum member specifying the result of validation.</returns>
        public ConnectionValidationResult ValidateConnection()
        {
            using (var client = new TcpClient())
            {
                try
                {
                    client.Connect(_config.Api.Host, _config.Api.UseSsl ? 443 : 80);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == 10060)
                    {
                        return ConnectionValidationResult.ConnectionRefused;
                    }

                    return ex.ErrorCode >= 11001 && ex.ErrorCode <= 11004
                               ? ConnectionValidationResult.NoInternetConnection
                               : ConnectionValidationResult.Unknown;
                }

                return ConnectionValidationResult.Success;
            }
        }

        /// <summary>
        /// Gets the public IP of the current machine
        /// </summary>
        /// <returns>A <see cref="IPAddress"/> representing the IP of the current machine or a null reference or a null reference if public IP could not be determined.</returns>
        [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "Ok")]
        public IPAddress GetPublicIp()
        {
            string data;

            try
            {
                var stream = _dataFetcher.GetDataAsync(new Uri("https://ipecho.net/plain")).GetAwaiter().GetResult();
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (AggregateException)
            {
                return null;
            }

            return IPAddress.TryParse(data, out var address)
                       ? address
                       : null;
        }
    }
}
