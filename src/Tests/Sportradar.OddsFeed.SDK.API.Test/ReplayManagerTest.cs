/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
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
        private TestDataFetcher _httpDataRestful;
        private IReplayManager _replayManager;

        [TestInitialize]
        public void Init()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var config = TestConfigurationInternal.GetConfig();

            object[] args =
            {
                new HttpClient(),
                config.AccessToken,
                new Deserializer<response>(),
                5,
                15
            };

            //_httpDataRestful = LogInterceptorFactory.Create<HttpDataRestful>(args, null, LoggerType.RestTraffic);
            //_httpDataRestful = LogInterceptorFactory.Create<TestDataFetcher>(args, null, LoggerType.RestTraffic);
            _httpDataRestful = new TestDataFetcher();

            object[] args2 =
            {
                "https://stgapi.betradar.com/v1/replay",
                _httpDataRestful,
                0
            };
            _replayManager = LogInterceptorFactory.Create<ReplayManager>(args2, m => m.Name.Contains("Async"), LoggerType.RestTraffic);
        }

        [TestMethod]
        public void AddMessageToReplayQueue()
        {
            var matchId = StaticRandom.Urn(9934843);
            _httpDataRestful.PutResponses.Add(new Tuple<string, int, HttpResponseMessage>($"/{matchId}", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));
            var replayResponse = _replayManager.AddMessagesToReplayQueue(matchId);
            Assert.IsNotNull(replayResponse);
        }
    }
}
