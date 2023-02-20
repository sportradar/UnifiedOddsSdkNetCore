using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class ConnectValidatorTests
    {
        private static ConnectionValidator Validator;

        public ConnectValidatorTests()
        {
            var config = TestConfigurationInternal.GetConfig();
            Validator = new ConnectionValidator(config, new HttpDataFetcher(new TestHttpClient(), new Deserializer<response>()));
        }

        //TODO: requires network
        //[Fact]
        public void ConnectionIsValidated()
        {
            var result = Validator.ValidateConnection();
            Assert.True(ConnectionValidationResult.Success.Equals(result));

            result = Validator.ValidateConnection();
            Assert.True(ConnectionValidationResult.Success.Equals(result));
        }

        //[Fact]
        public void PublicIpIsRetrieved()
        {
            var publicIp = Validator.GetPublicIp();
            Assert.NotNull(publicIp);
        }
    }
}
