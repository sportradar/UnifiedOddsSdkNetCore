/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
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
    public class DataProvider<TIn, TOut> : IDataProvider<TOut> where TIn : RestMessage where TOut : class
    {
        /// <summary>
        /// A <see cref="ILog"/> used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLogger(typeof(DataProvider<TIn, TOut>));

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
            Contract.Requires(!string.IsNullOrWhiteSpace(uriFormat));
            Contract.Requires(fetcher != null);
            Contract.Requires(deserializer != null);
            Contract.Requires(mapperFactory != null);

            _uriFormat = uriFormat;
            _fetcher = fetcher;
            _deserializer = deserializer;
            _mapperFactory = mapperFactory;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(!string.IsNullOrWhiteSpace(_uriFormat));
            Contract.Invariant(_fetcher != null);
            Contract.Invariant(_deserializer != null);
            Contract.Invariant(_mapperFactory != null);
        }

        /// <summary>
        /// Fetches and deserializes the data from the provided <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> specifying the data location</param>
        /// <returns>A <see cref="Task{T}"/> representing the ongoing operation</returns>
        protected async Task<TOut> GetDataAsyncInternal(Uri uri)
        {
            Contract.Requires(uri != null);

            var stream = await _fetcher.GetDataAsync(uri).ConfigureAwait(false);
            var item = _deserializer.Deserialize(stream);
            DispatchReceivedRawApiData(uri, item);
            return _mapperFactory.CreateMapper(item).Map();
        }

        /// <summary>
        /// Fetches and deserializes the data from the provided <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> specifying the data location</param>
        /// <returns>A <see cref="Task{T}"/> representing the ongoing operation</returns>
        protected TOut GetDataInternal(Uri uri)
        {
            Contract.Requires(uri != null);

            var stream = _fetcher.GetData(uri);
            var item = _deserializer.Deserialize(stream);
            DispatchReceivedRawApiData(uri, item);
            return _mapperFactory.CreateMapper(item).Map();
        }

        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <code>id</code>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <code>identifiers</code></returns>
        protected virtual Uri GetRequestUri(params object[] identifiers)
        {
            Contract.Requires(identifiers != null && identifiers.Any());
            Contract.Ensures(Contract.Result<Uri>() != null);

            return new Uri(string.Format(_uriFormat, identifiers));
        }

        /// <summary>
        /// get data as an asynchronous operation.
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.DeserializationException">The deserialization failed</exception>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        public async Task<TOut> GetDataAsync(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            return await GetDataAsyncInternal(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <typeparamref name="TOut"/> instance specified by the provided identifiersA two letter language code of the <see cref="CultureInfo"/>
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.DeserializationException">The deserialization failed</exception>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        public async Task<TOut> GetDataAsync(params string[] identifiers)
        {
            var uri = GetRequestUri(identifiers);
            return await GetDataAsyncInternal(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance in language specified by the provided <code>languageCode</code>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public TOut GetData(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            return GetDataInternal(uri);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public TOut GetData(params string[] identifiers)
        {
            var uri = GetRequestUri(identifiers);
            return GetDataInternal(uri);
        }

        private void DispatchReceivedRawApiData(Uri uri, RestMessage restMessage)
        {
            // send RawFeedMessage if needed
            try
            {
                var args = new RawApiDataEventArgs(uri, restMessage);
                RawApiDataReceived?.Invoke(this, args);
            }
            catch (Exception e)
            {
                ExecutionLog.Error($"Error dispatching raw message for {uri}", e);
            }
            // continue normal processing
        }
    }
}
