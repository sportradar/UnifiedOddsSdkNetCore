/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
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
        private readonly IUofConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducersProvider"/> class
        /// </summary>
        /// <param name="dataProvider">The <see cref="IDataProvider{producers}"/> used to fetch producers</param>
        /// <param name="config">The <see cref="IUofConfiguration"/> used to get properties to build <see cref="IProducer"/></param>
        public ProducersProvider(IDataProvider<producers> dataProvider, IUofConfiguration config)
        {
            Guard.Argument(dataProvider, nameof(dataProvider)).NotNull();
            Guard.Argument(config, nameof(config)).NotNull();

            _dataProvider = dataProvider;
            _config = config;
        }

        /// <summary>
        /// Gets the available producers from api
        /// </summary>
        /// <returns>A list of <see cref="IProducer"/></returns>
        public IReadOnlyCollection<IProducer> GetProducers()
        {
            var data = _dataProvider.GetData(_config.DefaultLanguage.TwoLetterISOLanguageName);

            return data == null
                ? null
                : MapProducers(data);
        }

        /// <summary>
        /// Maps (converts) the provided <see cref="producers" /> instance to the <see cref="IEnumerable{IProducer}" /> instance
        /// </summary>
        /// <param name="message">A <see cref="producers" /> instance to be mapped (converted)</param>
        /// <returns>A <see cref="IEnumerable{IProducer}" /> instance constructed from information in the provided <see cref="producers" /></returns>
        private IReadOnlyCollection<IProducer> MapProducers(producers message)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(message.producer.Length).Positive();

            return message.producer.Select(producer => new Producer((int)producer.id,
                                                                    producer.name,
                                                                    producer.description,
                                                                    _config.Environment != SdkEnvironment.Custom
                                                                        ? producer.api_url
                                                                        : ReplaceProducerApiUrl(producer.api_url),
                                                                    producer.active,
                                                                    (int)_config.Producer.InactivitySeconds.TotalSeconds,
                                                                    (int)_config.Producer.MaxRecoveryTime.TotalSeconds,
                                                                    producer.scope,
                                                                    producer.stateful_recovery_window_in_minutes)).Cast<IProducer>().ToList();
        }

        private string ReplaceProducerApiUrl(string url)
        {
            if (url.Contains(_config.Api.Host))
            {
                return url;
            }
            if (url.Contains(EnvironmentManager.GetApiHost(SdkEnvironment.Integration)))
            {
                return url.Replace(EnvironmentManager.GetApiHost(SdkEnvironment.Integration), _config.Api.Host);
            }
            return url.Replace(EnvironmentManager.GetApiHost(SdkEnvironment.Production), _config.Api.Host);
        }
    }
}
