/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to obtain <see cref="Stream"/> instances containing some data
    /// </summary>
    public interface IDataFetcher
    {
        /// <summary>
        /// Asynchronously gets a <see cref="Stream"/> containing data fetched from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be fetched</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="Stream"/> containing fetched data</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        Task<Stream> GetDataAsync(Uri uri);

        /// <summary>
        /// Gets a <see cref="Stream"/> containing data fetched from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be fetched</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="Stream"/> containing fetched data</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http get</exception>
        Stream GetData(Uri uri);
    }
}
