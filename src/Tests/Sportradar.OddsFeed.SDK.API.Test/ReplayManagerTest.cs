/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class ReplayManagerTest
    {
        private IDataRestful _httpDataRestful;
        private IReplayManager _replayManager;

        [TestInitialize]
        public void Init()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var config = TestConfigurationInternal.GetConfig();

            object[] args =
            {
                new HttpClient(),
                config.AccessToken,
                new Deserializer<response>(),
                5,
                15
            };

            _httpDataRestful = LogInterceptorFactory.Create<HttpDataRestful>(args, null, LoggerType.RestTraffic);

            object[] args2 =
            {
                "https://api.betradar.com/v1/replay",
                _httpDataRestful,
                0
            };
            _replayManager = LogInterceptorFactory.Create<ReplayManager>(args2, m => m.Name.Contains("Async"), LoggerType.RestTraffic);
        }

        [TestMethod]
        public void AddMessageToReplayQueue()
        {
            _replayManager.AddMessagesToReplayQueue(StaticRandom.Urn(9934843));
        }
    }
}
