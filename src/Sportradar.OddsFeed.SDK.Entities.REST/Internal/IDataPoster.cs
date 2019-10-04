/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used POST request to obtain <see cref="HttpResponseMessage"/> instance containing status code and data
    /// </summary>
    public interface IDataPoster
    {
        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage"/> as a result of POST request send to the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be send to</param>
        /// <param name="content">A <see cref="HttpContent"/> to be posted to the specific <see cref="Uri"/></param>
        /// <returns>A <see cref="Task"/> which, when completed will return a <see cref="HttpResponseMessage"/> containing status code and data</returns>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.CommunicationException">Failed to execute http post</exception>
        Task<HttpResponseMessage> PostDataAsync(Uri uri, HttpContent content = null);
    }
}
