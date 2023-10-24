/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public class MapAliveTests : MapEntityTestBase
{
    /// <inheritdoc />
    public MapAliveTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    [Fact]
    public void AliveIsMapped()
    {
        var record = Load<alive>("alive.xml", null, null);
        TestData.FillMessageTimestamp(record);
        var entity = Mapper.MapAlive(record);
        Assert.NotNull(entity);
    }

    [Fact]
    public void TestAliveMapping()
    {
        var record = Load<alive>("alive.xml", null, null);
        TestData.FillMessageTimestamp(record);
        var entity = Mapper.MapAlive(record);
        TestEntityValues(entity, record);
    }

    private void TestEntityValues(IAlive entity, alive message)
    {
        TestMessageProperties(entity, message.timestamp, message.product);
        Assert.Equal(entity.Subscribed, message.subscribed == 1);
    }
}
