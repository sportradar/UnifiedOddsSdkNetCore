// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class CustomBetEndpoint
{
    public static AvailableSelectionsType GetAvailableSelections(Urn eventId)
    {
        const string resourceName = "available_selections.xml";
        var restDeserializer = new Deserializer<AvailableSelectionsType>();

        _ = new AvailableSelectionsMapperFactory();
        var stream = FileHelper.GetResource(resourceName);
        var result = restDeserializer.Deserialize(stream);
        result.@event.id = eventId.ToString();
        return result;
    }

    public class CalculateEndpoint
    {
        //<calculation odds="14.654619322466335" probability="0.03827598152270673" harmonization="true"/>
        private double _odds;
        private double _probability;
        private bool? _harmonization;
        private Urn _eventId;

        public static CalculateEndpoint Create()
        {
            return new CalculateEndpoint();
        }

        public CalculateEndpoint WithOdds(double odds)
        {
            _odds = odds;
            return this;
        }

        public CalculateEndpoint WithProbability(double probability)
        {
            _probability = probability;
            return this;
        }

        public CalculateEndpoint WithHarmonization(bool harmonization)
        {
            _harmonization = harmonization;
            return this;
        }

        public CalculateEndpoint WithEventId(Urn eventId)
        {
            _eventId = eventId;
            return this;
        }

        public CalculationResponseType Build()
        {
            var result = new CalculationResponseType
            {
                calculation = new CalculationResultType
                {
                    odds = _odds,
                    probability = _probability,
                    harmonization = _harmonization.HasValue && _harmonization.Value,
                    harmonizationSpecified = _harmonization.HasValue
                },
                generated_at = DateTime.Now.ToString("o"),
                available_selections = _eventId != null
                    ? new[] { GetAvailableSelections(_eventId).@event }
                    : new[] { GetAvailableSelections(TestData.EventMatchId).@event }
            };
            return result;
        }
    }

    public class CalculateFilterEndpoint
    {
        //<calculation odds="14.654619322466335" probability="0.03827598152270673" harmonization="true"/>
        private double _odds;
        private double _probability;
        private bool? _harmonization;
        private Urn _eventId;

        public static CalculateFilterEndpoint Create()
        {
            return new CalculateFilterEndpoint();
        }

        public CalculateFilterEndpoint WithOdds(double odds)
        {
            _odds = odds;
            return this;
        }

        public CalculateFilterEndpoint WithProbability(double probability)
        {
            _probability = probability;
            return this;
        }

        public CalculateFilterEndpoint WithHarmonization(bool harmonization)
        {
            _harmonization = harmonization;
            return this;
        }

        public CalculateFilterEndpoint WithEventId(Urn eventId)
        {
            _eventId = eventId;
            return this;
        }

        public FilteredCalculationResponseType Build()
        {
#pragma warning disable format
            var filteredEventType = new FilteredEventType { id = _eventId.ToString(), markets = Array.Empty<FilteredMarketType>() };
            var result = new FilteredCalculationResponseType
                             {
                                 calculation = new FilteredCalculationResultType
                                                   {
                                                       odds = _odds,
                                                       probability = _probability,
                                                       harmonization = _harmonization.HasValue && _harmonization.Value,
                                                       harmonizationSpecified = _harmonization.HasValue
                                                   },
                                 generated_at = DateTime.Now.ToString("o"),
                                 available_selections = new[] { filteredEventType }
                             };
#pragma warning restore format
            return result;
        }
    }
}
