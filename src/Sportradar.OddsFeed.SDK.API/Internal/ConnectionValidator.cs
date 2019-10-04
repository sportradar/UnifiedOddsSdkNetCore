/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Used to validate connection to the message broker
    /// </summary>
    internal class ConnectionValidator
    {
        /// <summary>
        /// A <see cref="IOddsFeedConfigurationInternal"/> instance representing odds configuration
        /// </summary>
        private readonly IOddsFeedConfigurationInternal _config;

        /// <summary>
        /// A <see cref="IDataFetcher"/> instance used to execute http requests
        /// </summary>
        private readonly IDataFetcher _dataFetcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionValidator"/> class.
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfigurationInternal"/> instance representing odds configuration.</param>
        /// <param name="dataFetcher">A <see cref="IDataFetcher"/> instance used to execute http requests</param>
        public ConnectionValidator(IOddsFeedConfigurationInternal config, IDataFetcher dataFetcher)
        {
            Contract.Requires(config != null);
            Contract.Requires(dataFetcher != null);

            _config = config;
            _dataFetcher = dataFetcher;
        }

        /// <summary>
        /// Lists the object invariants as required by code-contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_config != null);
            Contract.Invariant(_dataFetcher != null);
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
                    client.Connect(_config.ApiHost, _config.UseApiSsl ? 443 : 80);
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
        public IPAddress GetPublicIp()
        {
            string data;

            try
            {
                var stream = _dataFetcher.GetDataAsync(new Uri("http://ipecho.net/plain")).Result;
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (AggregateException)
            {
                return null;
            }


            IPAddress address;
            return (IPAddress.TryParse(data, out address))
                ? address
                : null;
        }
    }
}
