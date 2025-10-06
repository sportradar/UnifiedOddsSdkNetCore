// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    internal class CashOutProbabilitiesProvider : ICashOutProbabilitiesProvider
    {
        /// <summary>
        /// The <see cref="ILogger"/> used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(CashOutProbabilitiesProvider));

        /// <summary>
        /// The <see cref="IDataProvider{cashout}"/> used to fetch probabilities
        /// </summary>
        private readonly IDataProvider<cashout> _dataProvider;

        /// <summary>
        /// The <see cref="IFeedMessageMapper"/> used to map deserialized data
        /// </summary>
        private readonly IFeedMessageMapper _messageMapper;

        /// <summary>
        /// The <see cref="IReadOnlyCollection{T}"/> specifying the languages in which the data is fetched by default
        /// </summary>
        private readonly IReadOnlyCollection<CultureInfo> _defaultCultures;

        /// <summary>
        /// The <see cref="ExceptionHandlingStrategy"/> specifying how to handle potential exceptions
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CashOutProbabilitiesProvider"/> class
        /// </summary>
        /// <param name="dataProvider">The <see cref="IDataProvider{cashout}"/> used to fetch probabilities</param>
        /// <param name="messageMapper">The <see cref="IFeedMessageMapper"/> used to map deserialized data</param>
        /// <param name="configuration">The <see cref="IUofConfiguration"/> used to get the languages and <see cref="ExceptionHandlingStrategy"/></param>
        public CashOutProbabilitiesProvider(IDataProvider<cashout> dataProvider, IFeedMessageMapper messageMapper, IUofConfiguration configuration)
        {
            Guard.Argument(dataProvider, nameof(dataProvider)).NotNull();
            Guard.Argument(messageMapper, nameof(messageMapper)).NotNull();
            Guard.Argument(configuration, nameof(configuration)).NotNull();

            _dataProvider = dataProvider;
            _messageMapper = messageMapper;
            _defaultCultures = configuration.Languages;
            _exceptionStrategy = configuration.ExceptionHandlingStrategy;
        }
        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified sport event
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="param">A parameter specifying which probabilities to get</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        private async Task<ICashOutProbabilities<T>> GetProbabilitiesInternalAsync<T>(string param, CultureInfo culture = null) where T : ISportEvent
        {
            var data = _exceptionStrategy == ExceptionHandlingStrategy.Throw
                           ? await _dataProvider.GetDataAsync(param).ConfigureAwait(false)
                           : await new Func<string, Task<cashout>>(_dataProvider.GetDataAsync).SafeInvokeAsync(param, ExecutionLog, "Error occurred while fetching probabilities for " + param).ConfigureAwait(false);

            var cultureResult = culture == null ? _defaultCultures : new[] { culture };

            return data == null
                       ? null
                       : _messageMapper.MapCashOutProbabilities<T>(data, cultureResult, null);
        }

        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified sport event.
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="eventId">The <see cref="Urn" /> uniquely identifying the sport event</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        public Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(Urn eventId, CultureInfo culture = null) where T : ISportEvent
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();

            return GetProbabilitiesInternalAsync<T>(eventId.ToString(), culture);
        }

        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified market on the specified sport event.
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="eventId">The <see cref="Urn" /> uniquely identifying the sport event</param>
        /// <param name="marketId">The id of the market for which to get the probabilities</param>
        /// <param name="specifiers">A <see cref="IDictionary{String, String}" /> containing market specifiers or a null reference if market has no specifiers</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        public Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(Urn eventId, int marketId, IReadOnlyDictionary<string, string> specifiers, CultureInfo culture = null) where T : ISportEvent
        {
            Guard.Argument(eventId, nameof(eventId)).NotNull();

            var param = string.Format("{0}/{1}", eventId, marketId.ToString());
            if (specifiers != null && specifiers.Any())
            {
                var specifiersText = string.Join(SdkInfo.SpecifiersDelimiter, specifiers.Select(x => x.Key + "=" + x.Value));
                param = param + "/" + specifiersText;
            }
            return GetProbabilitiesInternalAsync<T>(param, culture);
        }
    }
}
