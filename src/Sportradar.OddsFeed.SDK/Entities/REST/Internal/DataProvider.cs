/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// An implementation of the <see cref="IDataProvider{T}"/> which fetches the data, deserializes it and than maps / converts it
    /// to the output type
    /// </summary>
    /// <typeparam name="TIn">Specifies the type of DTO instance which will be mapped to returned instance</typeparam>
    /// <typeparam name="TOut">Specifies the type of instances provided</typeparam>
    /// <seealso cref="IDataProvider{T}" />
    internal class DataProvider<TIn, TOut> : IDataProvider<TOut> where TIn : RestMessage where TOut : class
    {
        /// <summary>
        /// A <see cref="ILogger"/> used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLogger(typeof(DataProvider<TIn, TOut>));

        /// <summary>
        /// Event raised when the data provider receives the message
        /// </summary>
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// A <see cref="IDataFetcher"/> used to fetch the data
        /// </summary>
        private readonly IDataFetcher _fetcher;

        /// <summary>
        /// A <see cref="IDeserializer{T}"/> used to deserialize the fetch data
        /// </summary>
        private readonly IDeserializer<TIn> _deserializer;

        /// <summary>
        /// A <see cref="ISingleTypeMapperFactory{T, T1}"/> used to construct instances of <see cref="ISingleTypeMapper{T}"/>
        /// </summary>
        private readonly ISingleTypeMapperFactory<TIn, TOut> _mapperFactory;

        /// <summary>
        /// The url format specifying the url of the resources fetched by the fetcher
        /// </summary>
        private readonly string _uriFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider{T, T1}" /> class.
        /// </summary>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{T}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{T, T1}" /> used to construct instances of <see cref="ISingleTypeMapper{T}" /></param>
        public DataProvider(string uriFormat, IDataFetcher fetcher, IDeserializer<TIn> deserializer, ISingleTypeMapperFactory<TIn, TOut> mapperFactory)
        {
            Guard.Argument(uriFormat, nameof(uriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();
            Guard.Argument(mapperFactory, nameof(mapperFactory)).NotNull();

            _uriFormat = uriFormat;
            _fetcher = fetcher;
            _deserializer = deserializer;
            _mapperFactory = mapperFactory;
        }

        /// <summary>
        /// Fetches and deserializes the data from the provided <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> specifying the data location</param>
        /// <param name="requestParams">The parameters associated with the request (if present)</param>
        /// <param name="culture">The language associated with the request</param>
        /// <returns>A <see cref="Task{T}"/> representing the ongoing operation</returns>
        protected async Task<TOut> GetDataAsyncInternal(Uri uri, string requestParams, string culture)
        {
            Guard.Argument(uri, nameof(uri)).NotNull();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var stream = await _fetcher.GetDataAsync(uri).ConfigureAwait(false);
            var item = _deserializer.Deserialize(stream);
            stopWatch.Stop();
            DispatchReceivedRawApiData(uri, item, requestParams, TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds), culture);
            return _mapperFactory.CreateMapper(item).Map();
        }

        /// <summary>
        /// Fetches and deserializes the data from the provided <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> specifying the data location</param>
        /// <param name="requestParams">The parameters associated with the request (if present)</param>
        /// <param name="culture">The language associated with the request</param>
        /// <returns>A <see cref="Task{T}"/> representing the ongoing operation</returns>
        protected TOut GetDataInternal(Uri uri, string requestParams, string culture)
        {
            Guard.Argument(uri, nameof(uri)).NotNull();

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var stream = _fetcher.GetData(uri);
            var item = _deserializer.Deserialize(stream);
            stopWatch.Stop();
            DispatchReceivedRawApiData(uri, item, requestParams, TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds), culture);
            return _mapperFactory.CreateMapper(item).Map();
        }

        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <c>id</c>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <c>identifiers</c></returns>
        protected virtual Uri GetRequestUri(params object[] identifiers)
        {
            Guard.Argument(identifiers, nameof(identifiers)).NotNull();
            if (identifiers.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(identifiers));
            }

            return new Uri(string.Format(_uriFormat, identifiers));
        }

        /// <summary>
        /// get data as an asynchronous operation.
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        /// <exception cref="Common.Exceptions.DeserializationException">The deserialization failed</exception>
        /// <exception cref="Common.Exceptions.MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        public async Task<TOut> GetDataAsync(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            return await GetDataAsyncInternal(uri, null, languageCode).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <typeparamref name="TOut"/> instance specified by the provided identifiersA two letter language code of the <see cref="CultureInfo"/>
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        /// <exception cref="Common.Exceptions.DeserializationException">The deserialization failed</exception>
        /// <exception cref="Common.Exceptions.MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        public async Task<TOut> GetDataAsync(params string[] identifiers)
        {
            var uri = GetRequestUri(identifiers);
            var requestedId = GetRequestedId(identifiers);
            var requestedLang = GetRequestedLanguage(identifiers);
            return await GetDataAsyncInternal(uri, requestedId, requestedLang).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance in language specified by the provided <c>languageCode</c>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public TOut GetData(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            return GetDataInternal(uri, null, languageCode);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public TOut GetData(params string[] identifiers)
        {
            var uri = GetRequestUri(identifiers);
            var requestedId = GetRequestedId(identifiers);
            var requestedLang = GetRequestedLanguage(identifiers);
            return GetDataInternal(uri, requestedId, requestedLang);
        }

        private void DispatchReceivedRawApiData(Uri uri, RestMessage restMessage, string requestParams, TimeSpan requestTime, string culture)
        {
            // send RawFeedMessage if needed
            try
            {
                var args = new RawApiDataEventArgs(uri, restMessage, requestParams, requestTime, culture);
                RawApiDataReceived?.Invoke(this, args);
            }
            catch (Exception e)
            {
                ExecutionLog.LogError(e, $"Error dispatching raw message for {uri}");
            }
            // continue normal processing
        }

        private string GetRequestedId(params string[] identifiers)
        {
            if (identifiers?.Length > 0)
            {
                if (identifiers.Length > 2)
                {
                    return string.Join("-", identifiers);
                }
                foreach (var identifier in identifiers)
                {
                    if (identifier == null)
                    {
                        continue;
                    }
                    if (SdkInfo.IsNumeric(identifier) || identifier.Contains(":"))
                    {
                        return identifier;
                    }
                }

                return identifiers[0];
            }
            return null;
        }

        private string GetRequestedLanguage(params string[] identifiers)
        {
            if (identifiers?.Length > 0)
            {
                foreach (var identifier in identifiers)
                {
                    if (identifier.IsNullOrEmpty() || identifier.Length > 3 || SdkInfo.IsNumeric(identifier))
                    {
                        continue;
                    }
                    try
                    {
                        var c = CultureInfo.GetCultureInfo(identifier);
                        if (c.TwoLetterISOLanguageName == identifier)
                        {
                            return identifier;
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
            return null;
        }
    }
}
