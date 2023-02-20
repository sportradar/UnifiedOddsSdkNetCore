/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Net;
using System.Net.Http;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal.Replay;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal.Log;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class ReplayManagerTests

    {
        private readonly TestDataFetcher _httpDataRestful;
        private readonly IReplayManager _replayManager;

        public ReplayManagerTests()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            _httpDataRestful = new TestDataFetcher();

            object[] args2 =
            {
                "https://stgapi.betradar.com/v1/replay",
                _httpDataRestful,
                0
            };
            _replayManager = LogInterceptorFactory.Create<ReplayManager>(args2, m => m.Name.Contains("Async"), LoggerType.RestTraffic);
        }

        [Fact]
        public void AddMessageToReplayQueue()
        {
            var matchId = StaticRandom.Urn(9934843);
            _httpDataRestful.PutResponses.Add(new Tuple<string, int, HttpResponseMessage>($"/{matchId}", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));
            var replayResponse = _replayManager.AddMessagesToReplayQueue(matchId);
            Assert.NotNull(replayResponse);
        }
    }
}
