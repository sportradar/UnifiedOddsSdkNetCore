// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public static class TournamentInfoEndpoints
{
    public static tournamentInfoEndpoint GetVolleyballWorldChampionshipWomenTournamentInfo()
    {
        return new TournamentInfoBuilder()
            .WithTournament(() => new TournamentBuilder()
                .WithId(Urn.Parse("sr:tournament:32"))
                .WithName("World Championship, Women")
                .WithCurrentSeason(SeasonEndpoints.GetVolleyballWorldChampionshipWomenSeason2025())
                .WithSport(Urn.Parse("sr:sport:23"), "Volleyball")
                .WithCategory(Urn.Parse("sr:category:136"), "International", null))
            .WithSeason(SeasonEndpoints.GetVolleyballWorldChampionshipWomenSeason2025())
            .WithLiveCoverageInfo(true)
            .AddGroup("A", "sr:group:92271",
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6717))
                    .WithName("Netherlands")
                    .WithAbbreviation("NED")
                    .WithCountry("Netherlands")
                    .WithCountryCode("NLD")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(216186)),
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6701))
                    .WithName("Egypt")
                    .WithAbbreviation("EGY")
                    .WithCountry("Egypt")
                    .WithCountryCode("EGY")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(279354)),
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6729))
                    .WithName("Thailand")
                    .WithAbbreviation("THA")
                    .WithCountry("Thailand")
                    .WithCountryCode("THA")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(231379)),
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6728))
                    .WithName("Sweden")
                    .WithAbbreviation("SWE")
                    .WithCountry("Sweden")
                    .WithCountryCode("SWE")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(802095)))
            .AddGroup("B", "sr:group:92273",
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6698))
                    .WithName("Cuba")
                    .WithAbbreviation("CUB")
                    .WithCountry("Cuba")
                    .WithCountryCode("CUB")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(231380)),
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6688))
                    .WithName("Belgium")
                    .WithAbbreviation("BEL")
                    .WithCountry("Belgium")
                    .WithCountryCode("BEL")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(265921)),
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6709))
                    .WithName("Italy")
                    .WithAbbreviation("ITA")
                    .WithCountry("Italy")
                    .WithCountryCode("ITA")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(231386)),
                () => new CompetitorBuilder()
                    .WithId(UrnCreate.CompetitorId(6724))
                    .WithName("Slovakia")
                    .WithAbbreviation("SVK")
                    .WithCountry("Slovakia")
                    .WithCountryCode("SVK")
                    .IsMale(false)
                    .WithReferences(new ReferencesBuilder().WithBetradar(260378)))
            .Build();
    }
}
