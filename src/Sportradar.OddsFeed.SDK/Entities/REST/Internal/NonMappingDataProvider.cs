// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// An implementation of the <see cref="IDataProvider{T}"/> which fetches the data and deserializes it
    /// </summary>
    /// <typeparam name="T">Specifies the type of Dto instance which will be obtained by deserialization and returned</typeparam>
    /// <seealso cref="IDataProvider{T}" />
    internal sealed class NonMappingDataProvider<T> : IDataProvider<T> where T : class
    {
        /// <summary>
        /// Event raised when the data provider receives the api message
        /// </summary>
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// A <see cref="IDataFetcher"/> used to fetch the data
        /// </summary>
        public IDataFetcher DataFetcher { get; }

        /// <summary>
        /// A <see cref="IDeserializer{T}"/> used to deserialize the fetch data
        /// </summary>
        private readonly IDeserializer<T> _deserializer;

        /// <summary>
        /// The url format specifying the url of the resources fetched by the fetcher
        /// </summary>
        private readonly string _uriFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider{T, T1}" /> class
        /// </summary>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{T}" /> used to deserialize the fetch data</param>
        public NonMappingDataProvider(string uriFormat, IDataFetcher fetcher, IDeserializer<T> deserializer)
        {
            Guard.Argument(uriFormat, nameof(uriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(deserializer, nameof(deserializer)).NotNull();

            _uriFormat = uriFormat;
            DataFetcher = fetcher;
            _deserializer = deserializer;
        }

        /// <summary>
        /// Fetches and deserializes the data from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> specifying the data location</param>
        /// <returns>A <see cref="Task{T}"/> representing the ongoing operation</returns>
        private async Task<T> GetDataAsyncInternal(Uri uri)
        {
            Guard.Argument(uri, nameof(uri)).NotNull();

            using (var stream = await DataFetcher.GetDataAsync(uri))
            {
                return _deserializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Fetches and deserializes the data from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">A <see cref="Uri"/> specifying the data location</param>
        /// <returns>A <see cref="Task{T}"/> representing the ongoing operation</returns>
        private T GetDataInternal(Uri uri)
        {
            Guard.Argument(uri, nameof(uri)).NotNull();

            using (var stream = DataFetcher.GetData(uri))
            {
                return _deserializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Constructs and returns an <see cref="Uri"/> instance used to retrieve resource with specified <c>id</c>
        /// </summary>
        /// <param name="identifiers">Identifiers uniquely identifying the data to fetch</param>
        /// <returns>an <see cref="Uri"/> instance used to retrieve resource with specified <c>identifiers</c></returns>
        private Uri GetRequestUri(params object[] identifiers)
        {
            Guard.Argument(identifiers, nameof(identifiers)).NotNull();

            if (identifiers.Length == 0 || identifiers.Any(a => a == null))
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
        public async Task<T> GetDataAsync(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            return await GetDataAsyncInternal(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <typeparamref name="T"/> instance specified by the provided identifiersA two letter language code of the <see cref="CultureInfo"/>
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        /// <exception cref="Common.Exceptions.DeserializationException">The deserialization failed</exception>
        /// <exception cref="Common.Exceptions.MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        public async Task<T> GetDataAsync(params string[] identifiers)
        {
            var uri = GetRequestUri(identifiers);
            return await GetDataAsyncInternal(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance in language specified by the provided <c>languageCode</c>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public T GetData(string languageCode)
        {
            var uri = GetRequestUri(languageCode);
            return GetDataInternal(uri);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        public T GetData(params string[] identifiers)
        {
            var uri = GetRequestUri(identifiers);
            return GetDataInternal(uri);
        }
    }
}
