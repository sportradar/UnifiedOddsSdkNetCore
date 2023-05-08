/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Replay;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    /// <summary>
    /// Data fetcher and poster for testing (can specify responses to be set or overridden)
    /// </summary>
    /// <seealso cref="IDataFetcher" />
    /// <seealso cref="IDataPoster" />
    /// <seealso cref="IDataRestful" />
    public class TestDataFetcher : IDataRestful
    {
        /// <summary>
        /// The list of URI replacements (to get wanted response when specific url is called)
        /// </summary>
        public List<Tuple<string, string>> UriReplacements;

        /// <summary>
        /// The list of possible post responses (to get wanted response when specific url is called)
        /// </summary>
        /// <remarks>string: url (or part of it) to be searched, int: 0-wanted response or <see cref="HttpStatusCode.BadRequest"/>; 1-wanted response; 2-wanted response or <see cref="HttpStatusCode.NotFound"/></remarks>
        public List<Tuple<string, int, HttpResponseMessage>> PostResponses;

        /// <summary>
        /// The list of possible put responses (to get wanted response when specific url is called)
        /// </summary>
        /// <remarks>string: url (or part of it) to be searched, int: 0-wanted response or <see cref="HttpStatusCode.BadRequest"/>; 1-wanted response; 2-wanted response or <see cref="HttpStatusCode.NotFound"/></remarks>
        public List<Tuple<string, int, HttpResponseMessage>> PutResponses;

        /// <summary>
        /// The list of possible delete responses (to get wanted response when specific url is called)
        /// </summary>
        /// <remarks>string: url (or part of it) to be searched, int: 0-wanted response or <see cref="HttpStatusCode.BadRequest"/>; 1-wanted response; 2-wanted response or <see cref="HttpStatusCode.NotFound"/></remarks>
        public List<Tuple<string, int, HttpResponseMessage>> DeleteResponses;

        /// <summary>
        /// The list of URI exceptions (to get wanted response when specific url is called)
        /// </summary>
        public readonly List<Tuple<string, Exception>> UriExceptions;

        /// <summary>
        /// The list of called urls
        /// </summary>
        public List<string> CalledUrls;

        public TestDataFetcher()
        {
            UriReplacements = new List<Tuple<string, string>>();
            PostResponses = new List<Tuple<string, int, HttpResponseMessage>>();
            PutResponses = new List<Tuple<string, int, HttpResponseMessage>>();
            DeleteResponses = new List<Tuple<string, int, HttpResponseMessage>>();
            UriExceptions = new List<Tuple<string, Exception>>();
            CalledUrls = new List<string>();
        }

        public TestDataFetcher(IEnumerable<Tuple<string, string>> uriReplacements)
        {
            UriReplacements = uriReplacements?.ToList() ?? new List<Tuple<string, string>>();
            PostResponses = new List<Tuple<string, int, HttpResponseMessage>>();
            PutResponses = new List<Tuple<string, int, HttpResponseMessage>>();
            DeleteResponses = new List<Tuple<string, int, HttpResponseMessage>>();
            UriExceptions = new List<Tuple<string, Exception>>();
            CalledUrls = new List<string>();
        }

        private string GetPathWithReplacements(string path)
        {
            return UriReplacements == null || !UriReplacements.Any()
                ? path
                : UriReplacements.Aggregate(path, (current, replacement) => current.Replace(replacement.Item1, replacement.Item2));
        }

        private void FindUriException(string path)
        {
            var replacement = UriExceptions.Where(w => path.Contains(w.Item1)).ToList();
            if (!replacement.IsNullOrEmpty())
            {
                throw replacement.First().Item2;
            }
        }

        public virtual Task<Stream> GetDataAsync(Uri uri)
        {
            CalledUrls.Add(uri.PathAndQuery);
            return Task.FromResult(FileHelper.OpenFile(GetPathWithReplacements(uri.LocalPath)));
        }

        public virtual Stream GetData(Uri uri)
        {
            CalledUrls.Add(uri.PathAndQuery);
            return FileHelper.OpenFile(GetPathWithReplacements(uri.LocalPath));
        }

        public virtual Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            CalledUrls.Add(uri.PathAndQuery);
            FindUriException(uri.PathAndQuery);
            var response = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            var getResponse = UriReplacements.FirstOrDefault(f => uri.ToString().Contains(f.Item1));
            if (getResponse != null)
            {
                switch (getResponse.Item2)
                {
                    case "0":
                        response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                        response.Content = new StringContent("response bad request");
                        break;
                    case "1":
                        response = new HttpResponseMessage(HttpStatusCode.Accepted);
                        response.Content = new StringContent("response content");
                        break;
                    case "2":
                        response = new HttpResponseMessage(HttpStatusCode.NotFound);
                        response.Content = new StringContent("response not found");
                        break;
                }
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage" /> as a result of POST request send to the provided <see cref="Uri" />. Will check <see cref="PostResponses"/> for wanted responses. If not found, will also check
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be send to</param>
        /// <param name="content">A <see cref="HttpContent" /> to be posted to the specific <see cref="Uri" /></param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="HttpResponseMessage" /> containing status code and data</returns>
        /// <remarks>If result defined in <see cref="PostResponses"/> int: 0-wanted response or <see cref="HttpStatusCode.BadRequest"/>; 1-Accepted; 2-wanted response or <see cref="HttpStatusCode.NotFound"/></remarks>
        public virtual Task<HttpResponseMessage> PostDataAsync(Uri uri, HttpContent content = null)
        {
            CalledUrls.Add(uri.PathAndQuery);
            FindUriException(uri.PathAndQuery);
            var response = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            var postResponse = PostResponses.FirstOrDefault(f => uri.ToString().Contains(f.Item1));
            if (postResponse != null)
            {
                switch (postResponse.Item2)
                {
                    case 0:
                        response = postResponse.Item3 ?? new HttpResponseMessage(HttpStatusCode.BadRequest);
                        break;
                    case 1:
                        response = new HttpResponseMessage(HttpStatusCode.Accepted);
                        break;
                    case 2:
                        response = postResponse.Item3 ?? new HttpResponseMessage(HttpStatusCode.NotFound);
                        break;
                }
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage" /> as a result of PUT request send to the provided <see cref="Uri" />. Will check <see cref="PutResponses"/> for wanted responses. If not found, will also check
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be send to</param>
        /// <param name="content">A <see cref="HttpContent" /> to be posted to the specific <see cref="Uri" /></param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="HttpResponseMessage" /> containing status code and data</returns>
        /// <remarks>If result defined in <see cref="PutResponses"/> int: 0-wanted response or <see cref="HttpStatusCode.BadRequest"/>; 1-Accepted; 2-wanted response or <see cref="HttpStatusCode.NotFound"/></remarks>
        public Task<HttpResponseMessage> PutDataAsync(Uri uri, HttpContent content = null)
        {
            CalledUrls.Add(uri.PathAndQuery);
            FindUriException(uri.PathAndQuery);
            var response = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            var putResponse = PutResponses.FirstOrDefault(f => uri.ToString().Contains(f.Item1));
            if (putResponse != null)
            {
                switch (putResponse.Item2)
                {
                    case 0:
                        response = putResponse.Item3 ?? new HttpResponseMessage(HttpStatusCode.BadRequest);
                        break;
                    case 1:
                        response = new HttpResponseMessage(HttpStatusCode.Accepted);
                        break;
                    case 2:
                        response = putResponse.Item3 ?? new HttpResponseMessage(HttpStatusCode.NotFound);
                        break;
                }
            }

            return Task.FromResult(response);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="HttpResponseMessage" /> as a result of DELETE request send to the provided <see cref="Uri" />. Will check <see cref="DeleteResponses"/> for wanted responses. If not found, will also check
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be send to</param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="HttpResponseMessage" /> containing status code and data</returns>
        /// <remarks>If result defined in <see cref="DeleteResponses"/> int: 0-wanted response or <see cref="HttpStatusCode.BadRequest"/>; 1-Accepted; 2-wanted response or <see cref="HttpStatusCode.NotFound"/></remarks>
        public Task<HttpResponseMessage> DeleteDataAsync(Uri uri)
        {
            CalledUrls.Add(uri.PathAndQuery);
            FindUriException(uri.PathAndQuery);
            var response = new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            var deleteResponse = DeleteResponses.FirstOrDefault(f => uri.ToString().Contains(f.Item1));
            if (deleteResponse != null)
            {
                switch (deleteResponse.Item2)
                {
                    case 0:
                        response = deleteResponse.Item3 ?? new HttpResponseMessage(HttpStatusCode.BadRequest);
                        break;
                    case 1:
                        response = new HttpResponseMessage(HttpStatusCode.Accepted);
                        break;
                    case 2:
                        response = deleteResponse.Item3 ?? new HttpResponseMessage(HttpStatusCode.NotFound);
                        break;
                }
            }

            return Task.FromResult(response);
        }
    }
}
