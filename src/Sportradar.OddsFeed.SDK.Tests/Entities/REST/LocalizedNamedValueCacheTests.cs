/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class LocalizedNamedValueCacheTests
    {
        private Mock<IDataFetcher> _fetcherMock;
        private Uri _enMatchStatusUri;
        private Uri _deMatchStatusUri;
        private Uri _huMatchStatusUri;
        private Uri _nlMatchStatusUri;

        private LocalizedNamedValueCache Setup(ExceptionHandlingStrategy exceptionStrategy)
        {
            var dataFetcher = new TestDataFetcher();
            _fetcherMock = new Mock<IDataFetcher>();

            _enMatchStatusUri = new Uri($"{TestData.RestXmlPath}/match_status_descriptions_en.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_enMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_enMatchStatusUri));

            _deMatchStatusUri = new Uri($"{TestData.RestXmlPath}/match_status_descriptions_de.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_deMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_deMatchStatusUri));

            _huMatchStatusUri = new Uri($"{TestData.RestXmlPath}/match_status_descriptions_hu.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_huMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_huMatchStatusUri));

            _nlMatchStatusUri = new Uri($"{TestData.RestXmlPath}/match_status_descriptions_nl.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_nlMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_nlMatchStatusUri));

            var uriFormat = $"{TestData.RestXmlPath}/match_status_descriptions_{{0}}.xml";
            return new LocalizedNamedValueCache(new NamedValueDataProvider(uriFormat, _fetcherMock.Object, "match_status"),
                new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu") }, exceptionStrategy);
        }

        [Fact]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "Allowed in this test")]
        public async Task Data_is_fetched_once_per_locale()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            var namedValue = await cache.GetAsync(0);
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("en") });
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("de") });
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("hu") });
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("nl") });
            namedValue = await cache.GetAsync(0, TestData.Cultures4);

            Assert.NotNull(namedValue);

            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Once);
        }

        [Fact]
        [SuppressMessage("ReSharper", "RedundantAssignment")]
        [SuppressMessage("Major Code Smell", "S1854:Unused assignments should be removed", Justification = "Allowed in this test")]
        public async Task Only_requested_locales_are_fetched()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            var namedValue = await cache.GetAsync(0, new[] { new CultureInfo("en") });
            namedValue = await cache.GetAsync(0, new[] { new CultureInfo("de") });

            Assert.NotNull(namedValue);

            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Never);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);
        }

        [Fact]
        public async Task Correct_value_are_loaded()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            var doc = XDocument.Load($"{TestData.RestXmlPath}/match_status_descriptions_en.xml");
            Assert.NotNull(doc);
            Assert.NotNull(doc.Element("match_status_descriptions"));

            foreach (var xElement in doc.Element("match_status_descriptions")!.Elements("match_status"))
            {
                Assert.NotNull(xElement.Attribute("id"));
                var id = int.Parse(xElement.Attribute("id")!.Value);
                var namedValue = await cache.GetAsync(id);

                Assert.NotNull(namedValue);
                Assert.Equal(id, namedValue.Id);

                Assert.True(namedValue.Descriptions.ContainsKey(new CultureInfo("en")));
                Assert.True(namedValue.Descriptions.ContainsKey(new CultureInfo("de")));
                Assert.True(namedValue.Descriptions.ContainsKey(new CultureInfo("hu")));
                Assert.False(namedValue.Descriptions.ContainsKey(new CultureInfo("nl")));

                Assert.NotEqual(namedValue.GetDescription(new CultureInfo("en")), new CultureInfo("de").Name);
                Assert.NotEqual(namedValue.GetDescription(new CultureInfo("en")), new CultureInfo("hu").Name);
                Assert.NotEqual(namedValue.GetDescription(new CultureInfo("de")), new CultureInfo("hu").Name);
            }
        }

        [Fact]
        public async Task Throwing_exception_strategy_is_respected()
        {
            var cache = Setup(ExceptionHandlingStrategy.THROW);
            Func<Task> action = () => cache.GetAsync(1000);
            await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
        }

        [Fact]
        public async Task Catching_exception_strategy_is_respected()
        {
            var cache = Setup(ExceptionHandlingStrategy.CATCH);
            var value = await cache.GetAsync(1000);

            Assert.Equal(1000, value.Id);
            Assert.NotNull(value.Descriptions);
            Assert.False(value.Descriptions.Any());
        }
    }
}
