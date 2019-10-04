/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// An implementation of the <see cref="ICalculateProbabilityProvider"/> which fetches the data, deserializes it and than maps / converts it
    /// to the output type
    /// </summary>
    public class CalculateProbabilityProvider : ICalculateProbabilityProvider
    {
        private readonly IDataPoster _poster;
        private readonly IDeserializer<CalculationResponseType> _deserializer;
        private readonly ISingleTypeMapperFactory<CalculationResponseType, CalculationDTO> _mapperFactory;
        private readonly string _uriFormat;
        private readonly XmlSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateProbabilityProvider" /> class.
        /// </summary>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="poster">A <see cref="IDataPoster" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{CalculationResponseType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{CalculationResponseType, CalculationDTO}" /> used to construct instances of <see cref="ISingleTypeMapper{CalculationDTO}" /></param>
        public CalculateProbabilityProvider(string uriFormat, IDataPoster poster, IDeserializer<CalculationResponseType> deserializer, ISingleTypeMapperFactory<CalculationResponseType, CalculationDTO> mapperFactory)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
                throw new ArgumentOutOfRangeException(nameof(uriFormat));
            if (poster == null)
                throw new ArgumentNullException(nameof(poster));
            if (deserializer == null)
                throw new ArgumentNullException(nameof(deserializer));
            if (mapperFactory == null)
                throw new ArgumentNullException(nameof(mapperFactory));

            _uriFormat = uriFormat;
            _poster = poster;
            _deserializer = deserializer;
            _mapperFactory = mapperFactory;
            _serializer = new XmlSerializer(typeof(SelectionsType));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="CalculationDTO"/> instance
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be fetched</param>
        /// <returns>A <see cref="Task{CalculationDTO}"/> representing the probability calculation</returns>
        public async Task<CalculationDTO> GetDataAsync(IEnumerable<ISelection> selections)
        {
            var content = GetContent(new SelectionsType
            {
                selection = selections.Select(s => new SelectionType
                {
                    id = s.EventId.ToString(),
                    market_id = s.MarketId,
                    outcome_id = s.OutcomeId,
                    specifiers = s.Specifiers
                }).ToArray()
            });


            var responseMessage = await _poster.PostDataAsync(new Uri(_uriFormat), content).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new CommunicationException($"Getting probability calculations failed with StatusCode={responseMessage.StatusCode}",
                    _uriFormat,
                    responseMessage.StatusCode,
                    null);
            }

            var stream = await responseMessage.Content.ReadAsStreamAsync();
            return _mapperFactory.CreateMapper(_deserializer.Deserialize(stream)).Map();
        }

        private HttpContent GetContent(SelectionsType content)
        {
            using (var stream = new MemoryStream())
            using (var writer = XmlWriter.Create(stream))
            {
                _serializer.Serialize(writer, content);
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
