// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNameGeneration;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.MarketNames;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Entities;

public abstract class MapEntityTestBase
{
    internal static readonly IDeserializer<FeedMessage> Deserializer = new Deserializer<FeedMessage>();

    internal readonly IFeedMessageMapper Mapper;

    protected MapEntityTestBase(ITestOutputHelper outputHelper)
    {
        var nameProviderFactoryMock = new Mock<INameProviderFactory>();
        var nameProviderMock = new Mock<INameProvider>();
        nameProviderFactoryMock.Setup(m => m.BuildNameProvider(It.IsAny<ICompetition>(), It.IsAny<int>(), It.IsAny<IReadOnlyDictionary<string, string>>())).Returns(nameProviderMock.Object);

        var nameCacheSdkTimer = SdkTimer.Create(UofSdkBootstrap.TimerForNamedValueCache, TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
        var voidReasonCache = new NamedValueCache("VoidReasons", ExceptionHandlingStrategy.Throw, new NamedValueDataProvider(UofSdkBootstrap.DataProviderForNamedValueCacheVoidReason, TestData.RestXmlPath + @"\void_reasons.xml", new TestDataFetcher(), "void_reason"), nameCacheSdkTimer);

        var namedValuesProviderMock = new Mock<INamedValuesProvider>();
        namedValuesProviderMock.Setup(x => x.VoidReasons).Returns(voidReasonCache);

        var sportEntityFactoryBuilder = new TestSportEntityFactoryBuilder(outputHelper, ScheduleData.Cultures3);
        Mapper = new FeedMessageMapper(
            sportEntityFactoryBuilder.SportEntityFactory,
            nameProviderFactoryMock.Object,
            new Mock<IMarketMappingProviderFactory>().Object,
            namedValuesProviderMock.Object,
            ExceptionHandlingStrategy.Throw,
            TestProducerManager.Create(),
            new Mock<IMarketCacheProvider>().Object,
            voidReasonCache);
    }

    protected T Load<T>(string fileName, Urn sportId, IEnumerable<CultureInfo> cultures)
        where T : FeedMessage
    {
        var stream = FileHelper.OpenFile(TestData.FeedXmlPath, fileName);
        var record = Deserializer.Deserialize<T>(stream);

        record.SportId = sportId;
        return record;
    }

    protected static bool CompareSpecifiers(IReadOnlyDictionary<string, string> marketSpecifiers, string recordSpecifiers)
    {
        if (string.IsNullOrEmpty(recordSpecifiers))
        {
            return marketSpecifiers == null;
        }

        var specifierParts = recordSpecifiers.Split(new[] { SdkInfo.SpecifiersDelimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
        specifierParts = specifierParts.OrderBy(p => p).ToList();

        var orderedRecordSpecifiers = string.Join(SdkInfo.SpecifiersDelimiter, specifierParts);
        var orderedSpecifiers = string.Join(SdkInfo.SpecifiersDelimiter, marketSpecifiers.OrderBy(kv => kv.Key).Select(kv => kv.Key + "=" + kv.Value));

        return orderedSpecifiers == orderedRecordSpecifiers;
    }

    protected static T FindMarket<T>(IEnumerable<T> markets, int id, string specifiersString) where T : IMarket
    {
        return markets == null
            ? default
            : markets.FirstOrDefault(m => m.Id == id && CompareSpecifiers(m.Specifiers, specifiersString));
    }

    protected static T FindOutcome<T>(IEnumerable<T> outcomes, string id) where T : IOutcome
    {
        return outcomes == null
            ? default
            : outcomes.FirstOrDefault(o => o.Id == id);
    }

    protected void TestMessageProperties(IMessage message, long timestamp, int productId)
    {
        Assert.Equal(message.Timestamps.Created, timestamp);
        Assert.Equal(message.Producer, TestProducerManager.Create().GetProducer(productId));
    }

    protected void TestEventMessageProperties<T>(IEventMessage<T> message, long timestamp, int productId, string eventId, long? requestId) where T : class, ISportEvent
    {
        TestMessageProperties(message, timestamp, productId);
        Assert.NotNull(message.Event);
        Assert.Equal(message.Event.Id.ToString(), eventId);
        Assert.Equal(message.RequestId, requestId);
    }
}
