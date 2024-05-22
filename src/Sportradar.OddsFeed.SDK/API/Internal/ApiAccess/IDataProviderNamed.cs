// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// Defines a contract implemented by classes used to provide data specified by it's id
    /// </summary>
    /// <typeparam name="T">Specifies the type of data returned by this <see cref="IDataProvider{T}"/></typeparam>
    internal interface IDataProviderNamed<T> : IDataProvider<T> where T : class
    {
        /// <summary>
        /// The name of the data provider
        /// </summary>
        string DataProviderName { get; }
    }
}
