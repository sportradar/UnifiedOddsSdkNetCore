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
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    /// <summary>
    /// Data fetcher and poster for testing
    /// </summary>
    /// <seealso cref="IDataFetcher" />
    /// <seealso cref="IDataPoster" />
    public class TestDataFetcher : IDataFetcher, IDataPoster
    {
        private readonly List<Tuple<string, string>> _uriReplacements;
        public TestDataFetcher()
        {
        }

        public TestDataFetcher(IEnumerable<Tuple<string, string>> uriReplacements)
        {
            _uriReplacements = uriReplacements?.ToList();
        }

        private string GetPathWithReplacements(string path)
        {
            return _uriReplacements == null || !_uriReplacements.Any()
                ? path
                : _uriReplacements.Aggregate(path, (current, replacement) => current.Replace(replacement.Item1, replacement.Item2));
        }

        public virtual Task<Stream> GetDataAsync(Uri uri)
        {
            return FileHelper.OpenFileAsync(GetPathWithReplacements(uri.LocalPath));
        }

        public Stream GetData(Uri uri)
        {
            return FileHelper.OpenFile(GetPathWithReplacements(uri.LocalPath));
        }

        public virtual Task<HttpResponseMessage> PostDataAsync(Uri uri, HttpContent content = null)
        {
            return Task.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.Accepted));
        }
    }
}
