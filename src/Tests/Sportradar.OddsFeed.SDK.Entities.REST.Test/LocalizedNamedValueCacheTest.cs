/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Test
{
    [TestClass]
    public class LocalizedNamedValueCacheTest
    {
        private Mock<IDataFetcher> _fetcherMock;
        private Uri _enMatchStatusUri;
        private Uri _deMatchStatusUri;
        private Uri _huMatchStatusUri;
        private Uri _nlMatchStatusUri;
        private LocalizedNamedValueCache _cache;

        private void Setup(ExceptionHandlingStrategy exceptionStrategy)
        {
            var dataFetcher = new TestDataFetcher();
            _fetcherMock = new Mock<IDataFetcher>();

            _enMatchStatusUri = new Uri(TestData.RestXmlPath + @"\match_status_descriptions.en.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_enMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_enMatchStatusUri));

            _deMatchStatusUri = new Uri(TestData.RestXmlPath + @"\match_status_descriptions.de.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_deMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_deMatchStatusUri));

            _huMatchStatusUri = new Uri(TestData.RestXmlPath + @"\match_status_descriptions.hu.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_huMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_huMatchStatusUri));

            _nlMatchStatusUri = new Uri(TestData.RestXmlPath + @"\match_status_descriptions.nl.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_nlMatchStatusUri))
                .Returns(dataFetcher.GetDataAsync(_nlMatchStatusUri));

            var uriFormat = TestData.RestXmlPath + @"\match_status_descriptions.{0}.xml";
            _cache = new LocalizedNamedValueCache(new NamedValueDataProvider(uriFormat, _fetcherMock.Object, "match_status"),
                new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu") }, exceptionStrategy);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _cache.Dispose();
        }

        [TestMethod]
        public void data_is_fetched_once_per_locale()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            var namedValue = _cache.GetAsync(0).Result;
            namedValue = _cache.GetAsync(0, new[] {new CultureInfo("en")}).Result;
            namedValue = _cache.GetAsync(0, new[] { new CultureInfo("de") }).Result;
            namedValue = _cache.GetAsync(0, new[] { new CultureInfo("hu") }).Result;
            namedValue = _cache.GetAsync(0, new[] { new CultureInfo("nl") }).Result;
            namedValue = _cache.GetAsync(0, TestData.Cultures4).Result;

            Assert.IsNotNull(namedValue);

            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Once);
        }

        [TestMethod]
        public void only_requested_locales_are_fetched()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            var namedValue = _cache.GetAsync(0, new[] {new CultureInfo("en")}).Result;
            namedValue = _cache.GetAsync(0, new[] { new CultureInfo("de") }).Result;

            Assert.IsNotNull(namedValue);

            _fetcherMock.Verify(x => x.GetDataAsync(_enMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_deMatchStatusUri), Times.Once);
            _fetcherMock.Verify(x => x.GetDataAsync(_huMatchStatusUri), Times.Never);
            _fetcherMock.Verify(x => x.GetDataAsync(_nlMatchStatusUri), Times.Never);
        }

        [TestMethod]
        public void correct_value_are_loaded()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            var doc = XDocument.Load(TestData.RestXmlPath + @"\match_status_descriptions.en.xml");

            foreach (var xElement in doc.Element("match_status_descriptions").Elements("match_status"))
            {
                var id = int.Parse(xElement.Attribute("id").Value);
                var namedValue = _cache.GetAsync(id).Result;

                Assert.IsNotNull(namedValue);
                Assert.AreEqual(id, namedValue.Id);

                Assert.IsTrue(namedValue.Descriptions.ContainsKey(new CultureInfo("en")));
                Assert.IsTrue(namedValue.Descriptions.ContainsKey(new CultureInfo("de")));
                Assert.IsTrue(namedValue.Descriptions.ContainsKey(new CultureInfo("hu")));
                Assert.IsFalse(namedValue.Descriptions.ContainsKey(new CultureInfo("nl")));

                Assert.AreNotEqual(namedValue.GetDescription(new CultureInfo("en")), new CultureInfo("de"), "English and German translations must not be the same");
                Assert.AreNotEqual(namedValue.GetDescription(new CultureInfo("en")), new CultureInfo("hu"), "English and Hungarian translations must not be the same");
                Assert.AreNotEqual(namedValue.GetDescription(new CultureInfo("de")), new CultureInfo("hu"), "German and Hungarian translations must not be the same");
            }
        }

        [TestMethod]
        public void throwing_exception_strategy_is_respected()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            try
            {
                var value = _cache.GetAsync(1000).Result;
                Assert.Fail("The operation should throw an exception");
            }
            catch (AggregateException ex)
            {
                if (!(ex.InnerException is ArgumentOutOfRangeException))
                {
                    Assert.Fail("The operation should throw ArgumentOutOfRangeException exception");
                }
            }
        }

        [TestMethod]
        public void catching_exception_strategy_is_respected()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var value = _cache.GetAsync(1000).Result;

            Assert.AreEqual(1000, value.Id);
            Assert.IsNotNull(value.Descriptions);
            Assert.IsFalse(value.Descriptions.Any());
        }
    }
}
