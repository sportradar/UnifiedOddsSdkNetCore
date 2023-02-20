using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class EnvironmentManagerTests
    {
        [Fact]
        public void GlobalAndNonGlobalApiSitUnderSameIpsButReplayShouldPointToNonGlobalAsItIsLongTermStrategy()
        {
            const string nonGlobalApiHost = "stgapi.betradar.com";
            Assert.Equal(nonGlobalApiHost, FindReplayEnvironmentSetting().ApiHost);
            Assert.Equal(nonGlobalApiHost, EnvironmentManager.GetSetting(SdkEnvironment.Replay).ApiHost);
            Assert.Equal(nonGlobalApiHost, EnvironmentManager.GetApiHost(SdkEnvironment.Replay));
        }

        [Fact]
        public void ReplayShouldPointToNonGlobalMessagingEndpointAsItIsLongTermStrategy()
        {
            const string nonGlobalMessagingHost = "replaymq.betradar.com";
            Assert.Equal(nonGlobalMessagingHost, FindReplayEnvironmentSetting().MqHost);
            Assert.Equal(nonGlobalMessagingHost, EnvironmentManager.GetSetting(SdkEnvironment.Replay).MqHost);
            Assert.Equal(nonGlobalMessagingHost, EnvironmentManager.GetMqHost(SdkEnvironment.Replay));
        }

        [Fact]
        public void ReplayShouldSupportSslOnlyConnections()
        {
            Assert.True(FindReplayEnvironmentSetting().OnlySsl);
            Assert.True(EnvironmentManager.GetSetting(SdkEnvironment.Replay).OnlySsl);
        }

        [Fact]
        public void RetryListShouldBeDeprecatedAsItIsNotOnlyNeverUsedInReplayButItIsExposedThroughStaticContextToCustomerSusceptableToBreakingChange()
        {
            Assert.NotNull(FindReplayEnvironmentSetting().EnvironmentRetryList);
            Assert.NotNull(EnvironmentManager.GetSetting(SdkEnvironment.Replay).EnvironmentRetryList);
        }

        private EnvironmentSetting FindReplayEnvironmentSetting()
        {
            var replaySettings = EnvironmentManager.EnvironmentSettings.Find(e => e.Environment == SdkEnvironment.Replay);
            Assert.NotNull(replaySettings);
            return replaySettings;
        }
    }
}
