// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.Rest;

public class TournamentDetailsMapperTests
{
    private const string InputXml = "tournament_info.xml";

    [Fact]
    public async Task TestInstanceIsNotNull()
    {
        var deserializer = new Deserializer<tournamentInfoEndpoint>();
        var dataFetcher = new TestDataFetcher();
        var mapperFactory = new TournamentInfoMapperFactory();

        var dataProvider = new DataProvider<tournamentInfoEndpoint, TournamentInfoDto>(TestData.RestXmlPath + InputXml,
                                                                                       dataFetcher,
                                                                                       deserializer,
                                                                                       mapperFactory);

        var entity = await dataProvider.GetDataAsync("en");

        Assert.NotNull(entity);
    }
}
