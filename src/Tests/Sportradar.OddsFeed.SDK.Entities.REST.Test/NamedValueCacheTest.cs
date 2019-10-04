/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
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
    public class NamedValueCacheTest
    {
        private Mock<IDataFetcher> _fetcherMock;
        private Uri _betStopReasonsUri;
        private NamedValueCache _cache;

        private void Setup(ExceptionHandlingStrategy exceptionStrategy)
        {
            var dataFetcher = new TestDataFetcher();
            _fetcherMock = new Mock<IDataFetcher>();

            _betStopReasonsUri = new Uri(TestData.RestXmlPath + @"\betstop_reasons.xml", UriKind.Absolute);
            _fetcherMock.Setup(args => args.GetDataAsync(_betStopReasonsUri))
                .Returns(dataFetcher.GetDataAsync(_betStopReasonsUri));

            var uriFormat = TestData.RestXmlPath + @"\betstop_reasons.xml";
            _cache = new NamedValueCache(new NamedValueDataProvider(uriFormat, _fetcherMock.Object, "betstop_reason"),
                exceptionStrategy);

        }

        [TestMethod]
        public void data_is_fetched_only_once()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var item = _cache.GetNamedValue(0);
            item = _cache.GetNamedValue(1);
            item = _cache.GetNamedValue(2);
            item = _cache.GetNamedValue(1000);

            Assert.IsNotNull(item);

            _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Once);
        }

        [TestMethod]
        public void data_is_fetched_only_when_requested()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            _fetcherMock.Verify(x => x.GetDataAsync(_betStopReasonsUri), Times.Never);
        }

        [TestMethod]
        public void correct_value_are_loaded()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            var doc = XDocument.Load(TestData.RestXmlPath + @"\betstop_reasons.xml");

            foreach (var xElement in doc.Element("betstop_reasons_descriptions").Elements("betstop_reason"))
            {
                var id = int.Parse(xElement.Attribute("id").Value);
                var namedValue = _cache.GetNamedValue(id);

                Assert.IsNotNull(namedValue);
                Assert.AreEqual(id, namedValue.Id);
                Assert.IsFalse(string.IsNullOrEmpty(namedValue.Description));
            }
        }

        [TestMethod]
        public void throwing_exception_strategy_is_respected()
        {
            Setup(ExceptionHandlingStrategy.THROW);
            try
            {
                var value = _cache.GetNamedValue(1000);
                Assert.Fail("The operation should throw an exception");
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }

        [TestMethod]
        public void catching_exception_strategy_is_respected()
        {
            Setup(ExceptionHandlingStrategy.CATCH);
            var value = _cache.GetNamedValue(1000);

            Assert.AreEqual(1000, value.Id);
            Assert.IsNull(value.Description);
        }
    }
}
