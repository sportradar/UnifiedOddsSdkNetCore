/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class NamedValueCacheTests
    {
        private Mock<IDataFetcher> _fetcherMock;
        private Uri _betStopReasonsUri;
        private NamedValueCache _cache;

        private void Setup(ExceptionHandlingStrategy exceptionStrategy, SdkTimer cacheSdkTimer = null)
        {
            var dataFetcher = new TestDataFetcher();
            _fetcherMock = new Mock<IDataFetcher>();

            _betStopReasonsUri = new Uri($"{TestData.RestXmlPath}/betstop_reasons.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_betStopReasonsUri)).Returns(dataFetcher.GetDataAsync(_betStopReasonsUri));

            var uriFormat = $"{TestData.RestXmlPath}/betstop_reasons.xml";
            var nameCacheSdkTimer = cacheSdkTimer ?? SdkTimer.Create(TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
            _cache = new NamedValueCache(new NamedValueDataProvider(uriFormat, _fetcherMock.Object, "betstop_reason"), exceptionStrategy, "BetstopReasons", nameCacheSdkTimer);
        }

        [Fact]
        public void DataIsFetchedOnlyOnce()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var item = _cache.GetNamedValue(0);
            _cache.GetNamedValue(1);
            _cache.GetNamedValue(2);
            _cache.GetNamedValue(1000);

            Assert.NotNull(item);

            _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once);
        }

        [Fact]
        public void InitialDataFetchDoesNotBlockConstructor()
        {
            Setup(ExceptionHandlingStrategy.CATCH, SdkTimer.Create(TimeSpan.FromSeconds(10), TimeSpan.Zero));
            _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Never);
        }

        [Fact]
        public void InitialDataFetchStartedByConstructor()
        {
            Setup(ExceptionHandlingStrategy.CATCH, SdkTimer.Create(TimeSpan.FromMilliseconds(10), TimeSpan.Zero));
            var finished = ExecutionHelper.WaitToComplete(() => _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once), 15000);
            Assert.True(finished);
        }

        [Fact]
        public void CorrectValuesAreLoaded()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            var doc = XDocument.Load($"{TestData.RestXmlPath}/betstop_reasons.xml");

            foreach (var xElement in doc.Element("betstop_reasons_descriptions")?.Elements("betstop_reason")!)
            {
                var id = int.Parse(xElement.Attribute("id")!.Value);
                var namedValue = _cache.GetNamedValue(id);

                Assert.NotNull(namedValue);
                Assert.Equal(id, namedValue.Id);
                Assert.False(string.IsNullOrEmpty(namedValue.Description));
            }
        }

        [Fact]
        public void ThrowingExceptionStrategyIsRespected()
        {
            Setup(ExceptionHandlingStrategy.THROW);

            Action action = () => _cache.GetNamedValue(1000);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void CatchingExceptionStrategyIsRespected()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var value = _cache.GetNamedValue(1000);

            Assert.Equal(1000, value.Id);
            Assert.Null(value.Description);
        }
    }
}
