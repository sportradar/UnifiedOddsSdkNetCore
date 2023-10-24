/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// An implementation of the <see cref="IDataProvider{T}"/> which fetches the data, deserializes it and than maps / converts it
    /// to the output type
    /// </summary>
    /// <typeparam name="TIn">Specifies the type of data-transfer-object instance which will be mapped to returned instance</typeparam>
    /// <typeparam name="TOut">Specifies the type of instances provided</typeparam>
    /// <seealso cref="IDataProvider{T}" />
    internal class DataProviderNamed<TIn, TOut> : DataProvider<TIn, TOut>, IDataProviderNamed<TOut> where TIn : RestMessage where TOut : class
    {
        public string DataProviderName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider{T, T1}" /> class.
        /// </summary>
        /// <param name="name">A unique name for this data provider</param>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{T}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{T, T1}" /> used to construct instances of <see cref="ISingleTypeMapper{T}" /></param>
        public DataProviderNamed(string name, string uriFormat, IDataFetcher fetcher, IDeserializer<TIn> deserializer, ISingleTypeMapperFactory<TIn, TOut> mapperFactory)
            : base(uriFormat, fetcher, deserializer, mapperFactory)
        {
            DataProviderName = name ?? throw new ArgumentNullException(nameof(name));
        }
    }
}
