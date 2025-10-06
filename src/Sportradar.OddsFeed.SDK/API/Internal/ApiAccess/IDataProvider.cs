// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide data specified by it's id
    /// </summary>
    /// <typeparam name="T">Specifies the type of data returned by this <see cref="IDataProvider{T}"/></typeparam>
    internal interface IDataProvider<T> where T : class
    {
        /// <summary>
        /// The data fetcher to make API request
        /// </summary>
        IDataFetcher DataFetcher { get; }

        /// <summary>
        /// Event raised when the data provider receives the api message
        /// </summary>
        event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// Asynchronously gets a rest response class instance in language specified by the provided <c>languageCode</c>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="CultureInfo"/></param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        /// <exception cref="DeserializationException">The deserialization failed</exception>
        /// <exception cref="MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        Task<T> GetDataAsync(string languageCode);

        /// <summary>
        /// Asynchronously gets a rest response class instance specified by the provided identifiersA two letter language code of the <see cref="CultureInfo"/>
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        /// <exception cref="DeserializationException">The deserialization failed</exception>
        /// <exception cref="MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        Task<T> GetDataAsync(params string[] identifiers);

        /// <summary>
        /// Gets a rest response class instance in language specified by the provided <c>languageCode</c>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="CultureInfo"/></param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        /// <exception cref="DeserializationException">The deserialization failed</exception>
        /// <exception cref="MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        T GetData(string languageCode);

        /// <summary>
        /// Gets a rest response class instance specified by the provided identifiersA two letter language code of the <see cref="CultureInfo"/>
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="Task{T}"/> representing the async operation</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        /// <exception cref="DeserializationException">The deserialization failed</exception>
        /// <exception cref="MappingException">The deserialized entity could not be mapped to entity used by the SDK</exception>
        T GetData(params string[] identifiers);
    }
}
