// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// An implementation of the <see cref="ICalculateProbabilityFilteredProvider"/> which fetches the data, deserializes it and than maps / converts it to the output type
    /// </summary>
    internal class CalculateProbabilityFilteredProvider : ICalculateProbabilityFilteredProvider
    {
        private readonly IDataPoster _poster;
        private readonly IDeserializer<FilteredCalculationResponseType> _deserializer;
        private readonly ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto> _mapperFactory;
        private readonly string _uriFormat;
        private readonly XmlSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculateProbabilityFilteredProvider" /> class.
        /// </summary>
        /// <param name="uriFormat">The url format specifying the url of the resources fetched by the fetcher</param>
        /// <param name="poster">A <see cref="IDataPoster" /> used to fetch the data</param>
        /// <param name="deserializer">A <see cref="IDeserializer{FilteredCalculationResponseType}" /> used to deserialize the fetch data</param>
        /// <param name="mapperFactory">A <see cref="ISingleTypeMapperFactory{FilteredCalculationResponseType, FilteredCalculationDto}" /> used to construct instances of <see cref="ISingleTypeMapper{FilteredCalculationDto}" /></param>
        public CalculateProbabilityFilteredProvider(string uriFormat, IDataPoster poster, IDeserializer<FilteredCalculationResponseType> deserializer, ISingleTypeMapperFactory<FilteredCalculationResponseType, FilteredCalculationDto> mapperFactory)
        {
            if (string.IsNullOrWhiteSpace(uriFormat))
            {
                throw new ArgumentOutOfRangeException(nameof(uriFormat));
            }

            _uriFormat = uriFormat;
            _poster = poster ?? throw new ArgumentNullException(nameof(poster));
            _deserializer = deserializer ?? throw new ArgumentNullException(nameof(deserializer));
            _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
            _serializer = new XmlSerializer(typeof(FilterSelectionsType));
        }

        /// <summary>
        /// Asynchronously gets a <see cref="FilteredCalculationDto"/> instance
        /// </summary>
        /// <param name="selections">The <see cref="IEnumerable{ISelection}"/> containing selections for which the probability should be fetched</param>
        /// <returns>A <see cref="Task{FilteredCalculationDto}"/> representing the probability calculation</returns>
        public async Task<FilteredCalculationDto> GetDataAsync(IEnumerable<ISelection> selections)
        {
            var resultSelection = new FilterSelectionsType();
            var selectionSelection = new List<FilterSelectionType>();

            foreach (var selection in selections)
            {
                var filterSelectionMarketType = new FilterSelectionMarketType
                {
                    market_id = selection.MarketId,
                    outcome_id = selection.OutcomeId,
                    specifiers = selection.Specifiers
                };

                if (selection.Odds != null)
                {
                    filterSelectionMarketType.odds = selection.Odds.Value;
                    filterSelectionMarketType.oddsSpecified = true;
                }
                var filterSelectionType = new FilterSelectionType
                {
                    id = selection.EventId.ToString(),
                    market = new[] { filterSelectionMarketType }
                };
                selectionSelection.Add(filterSelectionType);
            }

            resultSelection.selection = selectionSelection.ToArray();

            var content = CalculateProbabilityProvider.GetContent(_serializer, resultSelection);

            var responseMessage = await _poster.PostDataAsync(new Uri(_uriFormat), content).ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
            {
                var message = SdkInfo.ExtractHttpResponseMessage(responseMessage.Content);
                throw new CommunicationException($"Getting probability calculations (filtered) failed with message={message}",
                                                 _uriFormat,
                                                 responseMessage.StatusCode,
                                                 null);
            }

            var stream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return _mapperFactory.CreateMapper(_deserializer.Deserialize(stream)).Map();
        }
    }
}
