/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    internal class CashOutProbabilitiesProvider : ICashOutProbabilitiesProvider
    {
        /// <summary>
        /// The <see cref="ILog"/> used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(CashOutProbabilitiesProvider));

        /// <summary>
        /// The <see cref="IDataProvider{cashout}"/> used to fetch probabilities
        /// </summary>
        private readonly IDataProvider<cashout> _dataProvider;

        /// <summary>
        /// The <see cref="IFeedMessageMapper"/> used to map deserialized data
        /// </summary>
        private readonly IFeedMessageMapper _messageMapper;

        /// <summary>
        /// The <see cref="IEnumerable{CultureInfo}"/> specifying the languages in which the data is fetched by default
        /// </summary>
        private readonly IEnumerable<CultureInfo> _defaultCultures;

        /// <summary>
        /// The <see cref="ExceptionHandlingStrategy"/> specifying how to handle potential exceptions
        /// </summary>
        private readonly ExceptionHandlingStrategy _exceptionStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CashOutProbabilitiesProvider"/> class
        /// </summary>
        /// <param name="dataProvider">The <see cref="IDataProvider{cashout}"/> used to fetch probabilities</param>
        /// <param name="messageMapper">The <see cref="IFeedMessageMapper"/> used to map deserialized data</param>
        /// <param name="defaultCultures">The <see cref="IEnumerable{CultureInfo}"/> specifying the languages in which the data is fetched by default</param>
        /// <param name="exceptionStrategy">The <see cref="ExceptionHandlingStrategy"/> specifying how to handle potential exceptions</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public CashOutProbabilitiesProvider(IDataProvider<cashout> dataProvider, IFeedMessageMapper messageMapper, IEnumerable<CultureInfo> defaultCultures, ExceptionHandlingStrategy exceptionStrategy)
        {
            Contract.Requires(dataProvider != null);
            Contract.Requires(messageMapper != null);
            Contract.Requires(defaultCultures != null && defaultCultures.Any());

            _dataProvider = dataProvider;
            _messageMapper = messageMapper;
            _defaultCultures = defaultCultures;
            _exceptionStrategy = exceptionStrategy;
        }
        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified sport event
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="param">a <see cref="String"/> specifying which probabilities to get</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        private async Task<ICashOutProbabilities<T>> GetProbabilitiesInternalAsync<T>(string param, CultureInfo culture = null) where T : ISportEvent
        {
            var data = _exceptionStrategy == ExceptionHandlingStrategy.THROW
                ? await _dataProvider.GetDataAsync(param).ConfigureAwait(false)
                : await new Func<string, Task<cashout>>(_dataProvider.GetDataAsync).SafeInvokeAsync(
                    param,
                    ExecutionLog,
                    "Error occurred while fetching probabilities for " + param).ConfigureAwait(false);

            return data == null
                ? null
                : _messageMapper.MapCashOutProbabilities<T>(data, culture == null ? _defaultCultures : new[] {culture}, null);
        }

        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified sport event.
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="eventId">The <see cref="URN" /> uniquely identifying the sport event</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        public Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(URN eventId, CultureInfo culture = null) where T : ISportEvent
        {
            return GetProbabilitiesInternalAsync<T>(eventId.ToString(), culture);
        }

        /// <summary>
        /// Asynchronously gets the cash out probabilities for the specified market on the specified sport event.
        /// </summary>
        /// <typeparam name="T">The type of the sport event</typeparam>
        /// <param name="eventId">The <see cref="URN" /> uniquely identifying the sport event</param>
        /// <param name="marketId">The id of the market for which to get the probabilities</param>
        /// <param name="specifiers">A <see cref="IDictionary{String, String}" /> containing market specifiers or a null reference if market has no specifiers</param>
        /// <param name="culture">A <see cref="CultureInfo"/> specifying the language of the returned data, or a null reference to use default languages</param>
        /// <returns>A <see cref="Task{T}" /> representing the asynchronous operation</returns>
        public Task<ICashOutProbabilities<T>> GetCashOutProbabilitiesAsync<T>(URN eventId, int marketId, IReadOnlyDictionary<string, string> specifiers, CultureInfo culture = null) where T : ISportEvent
        {
            var param = $"{eventId}/{marketId}";
            if (specifiers != null && specifiers.Any())
            {
                param = param + "/" + string.Join(SdkInfo.SpecifiersDelimiter, specifiers.Select(x => x.Key + "=" + x.Value));
            }
            return GetProbabilitiesInternalAsync<T>(param, culture);
        }
    }
}
