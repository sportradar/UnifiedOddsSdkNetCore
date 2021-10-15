using System;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Test
{
    [TestClass]
    public class SdkConnectionTests
    {
        private ILogger _log;

        private const string RabbitIp = "192.168.64.101";
        private RabbitProducer _rabbitProducer;
        private TestDataRouterManager _dataRouterManager;
        private TestTimer _timer;
        private CacheManager _cacheManager;
        private TestProducersProvider _producersProvider;
        private IOddsFeed _feed;

        [TestInitialize]
        public void Init()
        {
            _log = SdkLoggerFactory.GetLogger(typeof(SdkConnectionTests));
            
            _timer = new TestTimer(true);
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);
            _producersProvider = new TestProducersProvider();
            var config = Feed.GetConfigurationBuilder()
                             .SetAccessToken("testuser")
                             .SelectCustom()
                             .SetMessagingUsername("testuser")
                             .SetMessagingPassword("testpass")
                             .SetMessagingHost(RabbitIp)
                             .UseMessagingSsl(false)
                             .SetApiHost(RabbitIp)
                             .SetDefaultLanguage(new CultureInfo("en"))
                             .Build();
            _feed = new TestFeed(_dataRouterManager, _producersProvider, config);


            _rabbitProducer = new RabbitProducer(RabbitIp);
            _rabbitProducer.Start();
            _rabbitProducer.Send("Hello world", "what.-.88.-");
        }

        [TestCleanup]
        public void Cleanup()
        {
            _rabbitProducer.ProducersAlive.Clear();
            DetachFromFeedEvents(_feed);
            _feed.Close();
            _rabbitProducer.Stop();
        }

        [TestMethod]
        public void SdkNormalRunTest()
        {
            _rabbitProducer.AddProducersAlive(1);
            _rabbitProducer.AddProducersAlive(3);

            _feed.ProducerManager.AddTimestampBeforeDisconnect(1, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(3, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(4, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(5, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(6, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(7, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(8, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(9, DateTime.Now.AddMinutes(-100));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(10, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(11, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(12, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(13, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(14, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(15, DateTime.Now.AddMinutes(-10));
            _feed.ProducerManager.AddTimestampBeforeDisconnect(16, DateTime.Now.AddMinutes(-2));

            //_feed.ProducerManager.DisableProducer(1); // LO
            _feed.ProducerManager.DisableProducer(2);
            _feed.ProducerManager.DisableProducer(3); // Ctrl - Prematch
            _feed.ProducerManager.DisableProducer(4); // BetPal
            _feed.ProducerManager.DisableProducer(5); // PC - PremiumCricket
            _feed.ProducerManager.DisableProducer(6); // VF - Virtual football
            _feed.ProducerManager.DisableProducer(7); // WNS - Numbers Betting
            _feed.ProducerManager.DisableProducer(8); // VBL - Virtual Basketball League
            _feed.ProducerManager.DisableProducer(9); // VTO - Virtual Tennis Open
            _feed.ProducerManager.DisableProducer(10); // VDR - Virtual Dog Racing
            _feed.ProducerManager.DisableProducer(11); // VHC - Virtual Horse Racing
            _feed.ProducerManager.DisableProducer(12); // VTI - Virtual Tennis In-Play
            _feed.ProducerManager.DisableProducer(13);
            _feed.ProducerManager.DisableProducer(14); // C-Odds - Competition Odds
            _feed.ProducerManager.DisableProducer(15); // VBI - Virtual Baseball In-Play
            _feed.ProducerManager.DisableProducer(16); // PB - Performance betting
            _feed.ProducerManager.DisableProducer(17); // VCI - Virtual Cricket In-Play

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.LiveMessagesOnly).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();
            var allSession2 = _feed.CreateBuilder().SetMessageInterest(MessageInterest.PrematchMessagesOnly).Build();
            var simpleMessageProcessor2 = new SimpleMessageProcessor(allSession2);
            simpleMessageProcessor2.Open();
            var allSession3 = _feed.CreateBuilder().SetMessageInterest(MessageInterest.VirtualSportMessages).Build();
            var simpleMessageProcessor3 = new SimpleMessageProcessor(allSession3);
            simpleMessageProcessor3.Open();
            _feed.Open();
            Thread.Sleep(120000);
            simpleMessageProcessor.Close();
            simpleMessageProcessor2.Close();
            simpleMessageProcessor3.Close();
        }
        
        private void AttachToFeedEvents(IOddsFeed oddsFeed)
        {
            oddsFeed.ProducerUp += ProducerUp;
            oddsFeed.ProducerDown += ProducerDown;
            oddsFeed.Disconnected += OnDisconnected;
            oddsFeed.Closed += OnClosed;
        }
        
        private void DetachFromFeedEvents(IOddsFeed oddsFeed)
        {
            oddsFeed.ProducerUp -= ProducerUp;
            oddsFeed.ProducerDown -= ProducerDown;
            oddsFeed.Disconnected -= OnDisconnected;
            oddsFeed.Closed -= OnClosed;
        }
        
        private void ProducerUp(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.LogWarning($"Producer {e.GetProducerStatusChange().Producer.Name} is up");
        }
        
        private void ProducerDown(object sender, ProducerStatusChangeEventArgs e)
        {
            _log.LogWarning($"Producer {e.GetProducerStatusChange().Producer.Name} is down");
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            _log.LogWarning("Connection to the feed lost");
        }

        private void OnClosed(object sender, FeedCloseEventArgs feedCloseEventArgs)
        {
            _log.LogWarning($"Feed closed with the reason: {feedCloseEventArgs.GetReason()}");
        }
    }
}
