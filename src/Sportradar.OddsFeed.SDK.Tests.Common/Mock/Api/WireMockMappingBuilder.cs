// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;
using WireMock;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Types;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Api;

public class WireMockMappingBuilder
{
    private readonly WireMockServer _server;
    private const string MappingNameForWhoAmI = "WhoAmIMapping";
    private const string MappingNameForProducers = "ProducersMapping";
    private const string MappingNameForMatchStatus = "MatchStatusMapping";
    private const string MappingNameForVoidReasons = "VoidReasonsMapping";
    private const string MappingNameForBetstopReasons = "BetstopReasonsMapping";
    private const string MappingNameForBettingStatus = "BettingStatusMapping";
    private const string MappingNameForInvariantMarketList = "InvariantMarketListMapping";
    private const string MappingNameForVariantMarketList = "VariantMarketListMapping";
    private const string MappingNameForAllSports = "AllSportsMapping";
    private const string MappingNameForAllTournamentsForAllSports = "AllTournamentsForAllSportsMapping";
    private const string MappingNameForScheduleForDate = "ScheduleForDateMapping";
    private const string MappingNameForRecoveryRequest = "ScheduleForRecoveryRequestMapping";

    public static ICollection<string> LogIgnoreXmls { get; } = new List<string>
    {
        "<match_status",
        "<betstop_reasons_descriptions",
        "<betting_status_descriptions",
        "<void_reasons_descriptions",
        "<match_status_descriptions",
        "<market_descriptions",
        "<variant_descriptions ",
        "<schedule",
        "<sports ",
        "<tournaments "
    };

    public WireMockMappingBuilder(WireMockServer server)
    {
        _server = server;
    }

    public WireMockMappingBuilder WithDefaultMaps(CultureInfo culture)
    {
        return WithDefaultMaps(new List<CultureInfo>
        {
            culture
        });
    }

    public WireMockMappingBuilder WithDefaultMaps(ICollection<CultureInfo> cultures)
    {
        WithAllProducers();
        WithVoidReasons();
        WithBetstopReasons();
        WithBettingStatus();
        WithAcceptAnyRecovery();
        //WithScheduleForDate();

        foreach (var culture in cultures)
        {
            WithMatchStatus(culture);
            WithInvariantMarketList(culture);
            WithVariantMarketList(culture);
            WithAllSports(culture);
            WithAllTournamentsForAllSports(culture);
            WithScheduleForDate(culture);
        }

        return this;
    }

    public WireMockMappingBuilder MapWhoAmI()
    {

        const string fileNameTemplate = "whoami.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath("/v1/users/whoami.xml").UsingGet())
            .WithTitle(MappingNameForWhoAmI)
               .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithWhoAmI(Action<WhoAmIEndpoint> options)
    {
        var endpointResponseBuilder = WhoAmIEndpoint.Create();
        options(endpointResponseBuilder);
        var endpointResponse = endpointResponseBuilder.Build();
        var responseXmlBody = MsgSerializer.SerializeToXml(endpointResponse);

        _server.Given(Request.Create().WithPath("/v1/users/whoami.xml").UsingGet())
            .WithTitle(MappingNameForWhoAmI)
               .RespondWith(Response.Create().WithBody(responseXmlBody));
        return this;
    }

    public WireMockMappingBuilder MapProducers()
    {
        const string fileNameTemplate = "producers.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath("/v1/descriptions/producers.xml").UsingGet())
            .WithTitle(MappingNameForProducers)
               .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithProducers(Action<ProducersEndpoint> options)
    {
        var endpointResponseBuilder = ProducersEndpoint.Create();
        options(endpointResponseBuilder);
        var endpointResponse = endpointResponseBuilder.Build();
        var responseXmlBody = MsgSerializer.SerializeToXml(endpointResponse);

        _server.Given(Request.Create().WithPath("/v1/descriptions/producers.xml").UsingGet())
            .WithTitle(MappingNameForProducers)
               .RespondWith(Response.Create().WithBody(responseXmlBody));
        return this;
    }

    public WireMockMappingBuilder WithAllProducers()
    {
        var endpointResponse = ProducersEndpoint.BuildAll();
        var responseXmlBody = MsgSerializer.SerializeToXml(endpointResponse);

        _server.Given(Request.Create().WithPath("/v1/descriptions/producers.xml").UsingGet())
            .WithTitle(MappingNameForProducers)
               .RespondWith(Response.Create().WithBody(responseXmlBody));
        return this;
    }

    public WireMockMappingBuilder WithMatchStatus(CultureInfo culture)
    {
        var fileNameTemplate = $"match_status_descriptions_{culture.TwoLetterISOLanguageName}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath($"/v1/descriptions/{culture.TwoLetterISOLanguageName}/match_status.xml").UsingGet())
            .WithTitle(MappingNameForMatchStatus)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithVoidReasons()
    {
        const string fileNameTemplate = "void_reasons.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath("/v1/descriptions/void_reasons.xml").UsingGet())
            .WithTitle(MappingNameForVoidReasons)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithBetstopReasons()
    {
        const string fileNameTemplate = "betstop_reasons.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath("/v1/descriptions/betstop_reasons.xml").UsingGet())
            .WithTitle(MappingNameForBetstopReasons)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithBettingStatus()
    {
        const string fileNameTemplate = "betting_status.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath("/v1/descriptions/betting_status.xml").UsingGet())
            .WithTitle(MappingNameForBettingStatus)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithInvariantMarketList(CultureInfo culture)
    {
        var fileNameTemplate = $"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath($"/v1/descriptions/{culture.TwoLetterISOLanguageName}/markets.xml").UsingGet())
            .WithTitle(MappingNameForInvariantMarketList)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public async Task<WireMockMappingBuilder> ReplaceInvariantMarketList(CultureInfo culture, int marketId, string marketNameTemplate, IDictionary<string, string> newOutcomes)
    {
        if (!_server.Mappings.IsNullOrEmpty() && _server.Mappings.Any(a => a.Title != null && a.Title.Equals(MappingNameForInvariantMarketList, StringComparison.Ordinal)))
        {
            _server.DeleteMapping(_server.Mappings.First(w => w.Title != null && w.Title.Equals(MappingNameForInvariantMarketList, StringComparison.Ordinal)).Guid);
        }

        var restDeserializer = new Deserializer<market_descriptions>();
        var fileNameTemplate = $"invariant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
        await using var stream = FileHelper.GetResource(fileNameTemplate);
        var apiMarketDescriptions = restDeserializer.Deserialize(stream);

        ModifyApiMarketDescription(marketId, marketNameTemplate, newOutcomes, apiMarketDescriptions);

        var apiMarketDescriptionsXml = DeserializerHelper.SerializeApiMessageToXml(apiMarketDescriptions);

        _server.Given(Request.Create().WithPath($"/v1/descriptions/{culture.TwoLetterISOLanguageName}/markets.xml").UsingGet())
               .WithTitle(MappingNameForInvariantMarketList)
               .RespondWith(Response.Create().WithBody(apiMarketDescriptionsXml));
        return this;
    }
    private static void ModifyApiMarketDescription(int marketId, string marketNameTemplate, IDictionary<string, string> newOutcomes, market_descriptions apiMarketDescriptions)
    {
        var apiMarket = apiMarketDescriptions.market.FirstOrDefault(f => f.id == marketId);
        if (apiMarket == null)
        {
            return;
        }

        if (!marketNameTemplate.IsNullOrEmpty())
        {
            apiMarket.name = marketNameTemplate;
        }
        if (!newOutcomes.IsNullOrEmpty())
        {
            foreach (var newOutcome in newOutcomes)
            {
                var apiOutcome = apiMarket.outcomes.FirstOrDefault(f => f.id == newOutcome.Key);
                if (apiOutcome != null)
                {
                    apiOutcome.name = newOutcome.Value;
                }
            }
        }
        //LogIgnoreXmls.Remove("<market_descriptions");
    }

    public WireMockMappingBuilder WithVariantMarketList(CultureInfo culture)
    {
        var fileNameTemplate = $"variant_market_descriptions_{culture.TwoLetterISOLanguageName}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath($"/v1/descriptions/{culture.TwoLetterISOLanguageName}/variants.xml").UsingGet())
            .WithTitle(MappingNameForVariantMarketList)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithAllSports(CultureInfo culture)
    {
        var fileNameTemplate = $"sports_{culture.TwoLetterISOLanguageName}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath($"/v1/sports/{culture.TwoLetterISOLanguageName}/sports.xml").UsingGet())
            .WithTitle(MappingNameForAllSports)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithAllTournamentsForAllSports(CultureInfo culture)
    {
        var fileNameTemplate = $"tournaments_{culture.TwoLetterISOLanguageName}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath($"/v1/sports/{culture.TwoLetterISOLanguageName}/tournaments.xml").UsingGet())
            .WithTitle(MappingNameForAllTournamentsForAllSports)
            .RespondWith(Response.Create().WithBody(responseBody));
        return this;
    }

    public WireMockMappingBuilder WithScheduleForDate(CultureInfo culture)
    {
        var languagePattern = @"(?<lang>\w{2})";
        var datePattern = @"\d{4}-\d{2}-\d{2}";
        var pathPattern = $"/v1/sports/{languagePattern}/schedules/{datePattern}/schedule.xml";

        var fileNameTemplate = $"schedule_{culture.TwoLetterISOLanguageName}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);

        _server.Given(Request.Create().WithPath(new RegexMatcher(pathPattern)).UsingGet())
            .WithTitle(MappingNameForScheduleForDate)
            .RespondWith(Response.Create().WithBody(responseBody));

        return this;
    }

    public WireMockMappingBuilder WithScheduleForDate()
    {
        var languagePattern = @"(?<lang>\w{2})";
        var datePattern = @"\d{4}-\d{2}-\d{2}";
        var pathPattern = $"/v1/sports/{languagePattern}/schedules/{datePattern}/schedule.xml";

        _server.Given(Request.Create().WithPath(new RegexMatcher(pathPattern)).UsingGet())
            .WithTitle(MappingNameForScheduleForDate)
            .RespondWith(Response.Create().WithCallback(GenerateResponseForScheduleForDate));

        return this;
    }

    private ResponseMessage GenerateResponseForScheduleForDate(IRequestMessage request)
    {
        // Extract the language code from the URL
        var languageCode = request.PathSegments[2]; // Adjust the index based on the actual URL structure
        var fileNameTemplate = $"schedule_{languageCode}.xml";
        var responseBody = FileHelper.GetFileContent(fileNameTemplate);
        responseBody = UrlUtils.SanitizeHtml(responseBody);

        return new ResponseMessage()
        {
            StatusCode = StatusCodes.Status200OK,
            BodyOriginal = responseBody,
            Headers = new Dictionary<string, WireMockList<string>>()
            {
                { "Content-Type", new WireMockList<string>("application/xml") }
            }
        };
    }

    public WireMockMappingBuilder WithAcceptAnyRecovery()
    {
        var producerPattern = @"(?<lang>\w+)";
        var pathPattern = $"/v1/{producerPattern}/recovery/initiate_request";

        _server.Given(Request.Create().WithPath(new RegexMatcher(pathPattern)).UsingPost())
            .WithTitle(MappingNameForRecoveryRequest)
            .RespondWith(Response.Create().WithStatusCode(200));
        return this;
    }

    public WireMockMappingBuilder RemoveMap(string mappingName)
    {
        // TODO
        _server.ResetMappings();
        return this;
    }
}
