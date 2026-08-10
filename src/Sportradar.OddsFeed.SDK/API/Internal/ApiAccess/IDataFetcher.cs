// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// Defines a contract implemented by classes used to obtain <see cref="Stream"/> instances containing some data
    /// </summary>
    internal interface IDataFetcher
    {
        /// <summary>
        /// Asynchronously gets a <see cref="Stream"/> containing data fetched from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be fetched</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="Stream"/> containing fetched data</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        Task<Stream> GetDataAsync(Uri uri);

        /// <summary>
        /// Asynchronously gets a <see cref="Stream" /> containing data fetched from the provided <see cref="Uri" />
        /// using custom query parameters and request headers.
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be fetched</param>
        /// <param name="queryParameters">The query parameters to include</param>
        /// <param name="headers">The request headers to include</param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="Stream" /> containing fetched data</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        Task<Stream> GetDataAsync(Uri uri, IReadOnlyDictionary<string, string> queryParameters, IReadOnlyDictionary<string, string> headers);

        /// <summary>
        /// Gets a <see cref="Stream"/> containing data fetched from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be fetched</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="Stream"/> containing fetched data</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        Stream GetData(Uri uri);
    }
}
