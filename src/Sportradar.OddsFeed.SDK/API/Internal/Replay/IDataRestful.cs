/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal.Replay
{
    /// <summary>
    /// Defines a contract implemented by classes used REST request to obtain <see cref="HttpResponseMessage"/> instance containing status code and data
    /// </summary>
    public interface IDataRestful : IDataPoster, IDataFetcher
    {
        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of PUT request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <param name="content">A <see cref="HttpContent"/> to be posted to the specific <see cref="Uri"/></param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http post</exception>
        Task<HttpResponseMessage> PutDataAsync(Uri uri, HttpContent content = null);

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of DELETE request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="Common.Exceptions.CommunicationException">Failed to execute http post</exception>
        Task<HttpResponseMessage> DeleteDataAsync(Uri uri);
    }
}
