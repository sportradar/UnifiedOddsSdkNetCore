/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// Provider for match status descriptions
    /// </summary>
    public class NamedValueDataProvider : IDataProvider<EntityList<NamedValueDTO>>
    {
        /// <summary>
        /// Event raised when the data provider receives the api message
        /// </summary>
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// The url format specifying the url of the resources fetched by the fetcher
        /// </summary>
        private readonly string _uriFormat;

        /// <summary>
        /// A <see cref="IDataFetcher"/> used to fetch the data
        /// </summary>
        private readonly IDataFetcher _fetcher;

        /// <summary>
        /// The name of the xml element containing id / description attributes
        /// </summary>
        private readonly string _xmlElementName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider{T, T1}" /> class
        /// </summary>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="fetcher">A <see cref="IDataFetcher" /> used to fetch the data</param>
        /// <param name="xmlElementName">The name of the xml element containing id / description attributes</param>
        public NamedValueDataProvider(string uriFormat, IDataFetcher fetcher, string xmlElementName)
        {
            Guard.Argument(uriFormat).NotNull().NotEmpty();
            Guard.Argument(fetcher).NotNull();
            Guard.Argument(xmlElementName).NotNull().NotEmpty();

            _uriFormat = uriFormat;
            _fetcher = fetcher;
            _xmlElementName = xmlElementName;
        }

        private async Task<EntityList<NamedValueDTO>> GetDescriptionsInternalAsync(Uri uri)
        {
            var stream = await _fetcher.GetDataAsync(uri).ConfigureAwait(false);

            return GetDescriptions(stream, uri);
        }

        private EntityList<NamedValueDTO> GetDescriptionsInternal(Uri uri)
        {
            var stream = _fetcher.GetData(uri);

            return GetDescriptions(stream, uri);
        }

        private EntityList<NamedValueDTO> GetDescriptions(Stream stream, Uri uri)
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
                    select new NamedValueDTO(id, desc);
                return new EntityList<NamedValueDTO>(result);
            }

            throw new CommunicationException("Failed to get match status descriptions.", uri.AbsoluteUri, null);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="!:T" /> instance in language specified by the provided <code>languageCode</code>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<EntityList<NamedValueDTO>> GetDataAsync(string languageCode)
        {
            var uri = new Uri(string.Format(_uriFormat, languageCode));
            return await GetDescriptionsInternalAsync(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<EntityList<NamedValueDTO>> GetDataAsync(params string[] identifiers)
        {
            var uri = new Uri(string.Format(_uriFormat, identifiers));
            return await GetDescriptionsInternalAsync(uri).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance in language specified by the provided <code>languageCode</code>
        /// </summary>
        /// <param name="languageCode">A two letter language code of the <see cref="T:System.Globalization.CultureInfo" /></param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public EntityList<NamedValueDTO> GetData(string languageCode)
        {
            var uri = new Uri(string.Format(_uriFormat, languageCode));
            return GetDescriptionsInternal(uri);
        }

        /// <summary>
        /// Gets a <see cref="!:T" /> instance specified by the provided identifiersA two letter language code of the <see cref="T:System.Globalization.CultureInfo" />
        /// </summary>
        /// <param name="identifiers">A list of identifiers uniquely specifying the instance to fetch</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the async operation</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public EntityList<NamedValueDTO> GetData(params string[] identifiers)
        {
            var uri = new Uri(string.Format(_uriFormat, identifiers));
            return GetDescriptionsInternal(uri);
        }
    }
}
