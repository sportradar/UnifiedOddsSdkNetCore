// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.ApiAccess;

public class ConnectValidatorTests
{
    private static ConnectionValidator Validator;

    public ConnectValidatorTests()
    {
        var config = TestConfiguration.GetConfig();
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
