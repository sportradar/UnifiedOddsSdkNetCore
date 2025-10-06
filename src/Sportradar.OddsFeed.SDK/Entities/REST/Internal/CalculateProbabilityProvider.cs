// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal
{
    /// <summary>
    /// An implementation of the <see cref="ICalculateProbabilityProvider"/> which fetches the data, deserializes it and than maps / converts it to the output type
    /// </summary>
    internal class CalculateProbabilityProvider : ICalculateProbabilityProvider
    {
        private readonly IDataPoster _poster;
        private readonly IDeserializer<CalculationResponseType> _deserializer;
        private readonly ISingleTypeMapperFactory<CalculationResponseType, CalculationDto> _mapperFactory;
        private readonly string _uriFormat;
        private readonly XmlSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateProbabilityProvider" /> class.
        /// </summary>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="poster">A <see cref="IDataPoster" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{CalculationResponseType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{CalculationResponseType, CalculationDto}" /> used to construct instances of <see cref="ISingleTypeMapper{CalculationDto}" /></param>
        public CalculateProbabilityProvider(string uriFormat, IDataPoster poster, IDeserializer<CalculationResponseType> deserializer, ISingleTypeMapperFactory<CalculationResponseType, CalculationDto> mapperFactory)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
            {
                throw new ArgumentOutOfRangeException(nameof(uriFormat));
            }

            _uriFormat = uriFormat;
            _poster = poster ?? throw new ArgumentNullException(nameof(poster));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
            _serializer = new XmlSerializer(typeof(SelectionsType));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="CalculationDto"/> instance
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be fetched</param>
        /// <returns>A <see cref="Task{CalculationDto}"/> representing the probability calculation</returns>
        public async Task<CalculationDto> GetDataAsync(IEnumerable<ISelection> selections)
        {
            var capiSelections = new List<SelectionType>();
            foreach (var selection in selections)
            {
                var capiSelection = new SelectionType
                {
                    id = selection.EventId.ToString(),
                    market_id = selection.MarketId,
                    outcome_id = selection.OutcomeId,
                    specifiers = selection.Specifiers
                };

                if (selection.Odds != null)
                {
                    capiSelection.odds = selection.Odds.Value;
                    capiSelection.oddsSpecified = true;
                }
                capiSelections.Add(capiSelection);
            }

            var content = GetContent(_serializer, new SelectionsType { selection = capiSelections.ToArray() });

            var responseMessage = await _poster.PostDataAsync(new Uri(_uriFormat), content).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
            {
                var message = SdkInfo.ExtractHttpResponseMessage(responseMessage.Content);
                throw new CommunicationException($"Getting probability calculations failed with message={message}",
                                                 _uriFormat,
                                                 responseMessage.StatusCode,
                                                 null);
            }

            var stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return _mapperFactory.CreateMapper(_deserializer.Deserialize(stream)).Map();
        }

        internal static HttpContent GetContent<T>(XmlSerializer xmlSerializer, T content)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream))
                {
                    xmlSerializer.Serialize(writer, content);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    var str = reader.ReadToEnd();
                    var httpContent = new StringContent(str);
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

                    return httpContent;
                }
            }
        }
    }
}
