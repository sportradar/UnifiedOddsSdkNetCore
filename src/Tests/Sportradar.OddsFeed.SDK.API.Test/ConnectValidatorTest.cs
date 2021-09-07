/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class ConnectValidatorTest
    {
        private static ConnectionValidator _validator;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var config = TestConfigurationInternal.GetConfig();
            _validator = new ConnectionValidator(config, new HttpDataFetcher(new HttpClient(), TestData.AccessToken, new Deserializer<response>()));
        }

        [TestMethod]
        public void ConnectionIsValidated()
        {
            var result = _validator.ValidateConnection();
            Assert.AreEqual(ConnectionValidationResult.Success, result, "The connection validation should return success");

            result = _validator.ValidateConnection();
            Assert.AreEqual(ConnectionValidationResult.Success, result, "The connection validation should return success");
        }

        [TestMethod]
        public void PublicIpIsRetrieved()
        {
            var publicIp = _validator.GetPublicIp();
            Assert.IsNotNull(publicIp);
        }
    }
}
