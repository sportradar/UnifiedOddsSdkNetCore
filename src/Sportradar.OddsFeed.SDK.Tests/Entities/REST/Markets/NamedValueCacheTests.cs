/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST.Markets
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
            _fetcherMock.Setup(args => args.GetDataAsync(_betStopReasonsUri))
                .Returns(dataFetcher.GetDataAsync(_betStopReasonsUri));

            var uriFormat = $"{TestData.RestXmlPath}/betstop_reasons.xml";
            var nameCacheSdkTimer = cacheSdkTimer ?? SdkTimer.Create(TimeSpan.FromMilliseconds(10), TimeSpan.Zero);
            _cache = new NamedValueCache(new NamedValueDataProvider(uriFormat, _fetcherMock.Object, "betstop_reason"), exceptionStrategy, "BetstopReasons", nameCacheSdkTimer);

        }

        [Fact]
        [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "Allowed in this test")]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        public void Data_is_fetched_only_once()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var item = _cache.GetNamedValue(0);
            item = _cache.GetNamedValue(1);
            item = _cache.GetNamedValue(2);
            item = _cache.GetNamedValue(1000);

            Assert.NotNull(item);

            _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once);
        }

        [Fact]
        public void Data_is_fetched_only_when_requested()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Never);
        }

        [Fact]
        public void Correct_value_are_loaded()
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
        public void Throwing_exception_strategy_is_respected()
        {
            Setup(ExceptionHandlingStrategy.THROW);

            Action action = () => _cache.GetNamedValue(1000);
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Catching_exception_strategy_is_respected()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var value = _cache.GetNamedValue(1000);

            Assert.Equal(1000, value.Id);
            Assert.Null(value.Description);
        }
    }
}
