/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Class ProducersProvider
    /// </summary>
    /// <seealso cref="IProducersProvider" />
    internal class ProducersProvider : IProducersProvider
    {
        /// <summary>
        /// The <see cref="IDataProvider{producers}"/> used to fetch producers
        /// </summary>
        private readonly IDataProvider<producers> _dataProvider;

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly IOddsFeedConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducersProvider"/> class
        /// </summary>
        /// <param name="dataProvider">The <see cref="IDataProvider{producers}"/> used to fetch producers</param>
        /// <param name="config">The <see cref="IOddsFeedConfiguration"/> used to get properties to build <see cref="IProducer"/></param>
        public ProducersProvider(IDataProvider<producers> dataProvider, IOddsFeedConfiguration config)
        {
            Guard.Argument(dataProvider).NotNull();
            Guard.Argument(config).NotNull();

            _dataProvider = dataProvider;
            _config = config;
        }

        /// <summary>
        /// Gets the available producers from api
        /// </summary>
        /// <returns>A list of <see cref="IProducer"/></returns>
        public IEnumerable<IProducer> GetProducers()
        {
            var data = _dataProvider.GetData("en");

            return data == null
                ? null
                : MapProducers(data);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="producers" /> instance to the <see cref="IEnumerable{IProducer}" /> instance
        /// </summary>
        /// <param name="message">A <see cref="producers" /> instance to be mapped (converted)</param>
        /// <returns>A <see cref="IEnumerable{IProducer}" /> instance constructed from information in the provided <see cref="producers" /></returns>
        private IEnumerable<IProducer> MapProducers(producers message)
        {
            Guard.Argument(message).NotNull();
            Guard.Argument(message.producer.Length).Positive();

            return message.producer.Select(producer => new Producer(
                                                                    (int) producer.id,
                                                                    producer.name,
                                                                    producer.description,
                                                                    _config.Environment != SdkEnvironment.Custom
                                                                        ? producer.api_url
                                                                        : ReplaceProducerApiUrl(producer.api_url),
                                                                    producer.active,
                                                                    _config.InactivitySeconds,
                                                                    producer.stateful_recovery_window_in_minutes,
                                                                    producer.scope)).Cast<IProducer>().ToList();
        }

        private string ReplaceProducerApiUrl(string url)
        {
            if (url.Contains(SdkInfo.IntegrationApiHost))
            {
                return url.Replace(SdkInfo.IntegrationApiHost, _config.ApiHost);
            }
            return url.Replace(SdkInfo.ProductionApiHost, _config.ApiHost);
        }
    }
}
