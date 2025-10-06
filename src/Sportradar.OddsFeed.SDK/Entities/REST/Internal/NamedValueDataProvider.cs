// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// Provider for match status descriptions
    /// </summary>
    internal class NamedValueDataProvider : IDataProviderNamed<EntityList<NamedValueDto>>
    {
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        private readonly string _uriFormat;

        public IDataFetcher DataFetcher { get; }

        private readonly string _xmlElementName;

        public string DataProviderName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider{T, T1}" /> class
        /// </summary>
        /// <param name="dataProviderName">Name of data provider</param>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="xmlElementName">The name of the XML element containing id / description attributes</param>
        public NamedValueDataProvider(string dataProviderName, string uriFormat, IDataFetcher fetcher, string xmlElementName)
        {
            Guard.Argument(dataProviderName, nameof(dataProviderName)).NotNull().NotEmpty();
            Guard.Argument(uriFormat, nameof(uriFormat)).NotNull().NotEmpty();
            Guard.Argument(fetcher, nameof(fetcher)).NotNull();
            Guard.Argument(xmlElementName, nameof(xmlElementName)).NotNull().NotEmpty();

            DataProviderName = dataProviderName;
            _uriFormat = uriFormat;
            DataFetcher = fetcher;
            _xmlElementName = xmlElementName;
        }

        private async Task<EntityList<NamedValueDto>> GetDescriptionsInternalAsync(Uri uri)
        {
            var stream = await DataFetcher.GetDataAsync(uri).ConfigureAwait(false);

            return GetDescriptions(stream, uri);
        }

        private EntityList<NamedValueDto> GetDescriptionsInternal(Uri uri)
        {
            var stream = DataFetcher.GetData(uri);

            return GetDescriptions(stream, uri);
        }

        private EntityList<NamedValueDto> GetDescriptions(Stream stream, Uri uri)
        {
            var document = new XmlDocument();
            document.Load(stream);

            if (document.DocumentElement != null)
            {
                var nodes = document.DocumentElement.SelectNodes(_xmlElementName);
                var result = from XmlNode m in nodes
                             where m.Attributes?["id"] != null && m.Attributes["description"] != null
                             let id = int.Parse(m.Attributes["id"].Value)
                             let desc = m.Attributes?["description"].Value
                             select new NamedValueDto(id, desc);
                return new EntityList<NamedValueDto>(result);
            }

            throw new CommunicationException("Failed to get match status descriptions.", uri.AbsoluteUri, null);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="!:T" /> instance in language specified by the provided <c>languageCode</c>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<EntityList<NamedValueDto>> GetDataAsync(string languageCode)
        {
            var uri = new Uri(string.Format(_uriFormat, languageCode));
            return await GetDescriptionsInternalAsync(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<EntityList<NamedValueDto>> GetDataAsync(params string[] identifiers)
        {
            var uri = new Uri(string.Format(_uriFormat, identifiers));
            return await GetDescriptionsInternalAsync(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance in language specified by the provided <c>languageCode</c>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="NotImplementedException"></exception>
        public EntityList<NamedValueDto> GetData(string languageCode)
        {
            var uri = new Uri(string.Format(_uriFormat, languageCode));
            return GetDescriptionsInternal(uri);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="NotImplementedException"></exception>
        public EntityList<NamedValueDto> GetData(params string[] identifiers)
        {
            var uri = new Uri(string.Format(_uriFormat, identifiers));
            return GetDescriptionsInternal(uri);
        }
    }
}
