using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Castle.Core.Internal;
using EasyNetQ.Management.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.API.Extended;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.Test
{
    [TestClass]
    public class SdkConnectionTests
    {
        private const string SdkUsername = "testuser";
        private const string SdkPassword = "testpass";
        private RabbitProducer _rabbitProducer;
        private TestDataRouterManager _dataRouterManager;
        private CacheManager _cacheManager;
        private TestProducersProvider _producersProvider;
        private TestFeed _feed;
        private FMessageBuilder _fMessageBuilder;
        private List<string> _calledEvents;
        private IOddsFeedConfiguration _config;

        [TestInitialize]
        public void Init()
        {
            // setup classes for simulating outside data sources (api access, ...)
            // setup connection to test rabbit server
            _calledEvents = new List<string>();
            _cacheManager = new CacheManager();
            _dataRouterManager = new TestDataRouterManager(_cacheManager);
            _producersProvider = new TestProducersProvider();
            _config = Feed.GetConfigurationBuilder()
                             .SetAccessToken("testuser")
                             .SelectCustom()
                             .SetMessagingUsername(SdkUsername)
                             .SetMessagingPassword(SdkPassword)
                             .SetMessagingHost(RabbitProducer.RabbitIp)
                             .UseMessagingSsl(false)
                             .SetApiHost(RabbitProducer.RabbitIp)
                             .SetDefaultLanguage(new CultureInfo("en"))
                             .SetMinIntervalBetweenRecoveryRequests(20)
                             .Build();
            _feed = new TestFeed(_dataRouterManager, _producersProvider, _config);
            _fMessageBuilder = new FMessageBuilder(1);

            // establish connection to the test rabbit for rabbit producer
            _rabbitProducer = new RabbitProducer(_producersProvider);
            _rabbitProducer.Start();
            _rabbitProducer.RabbitChangeUserPassword(SdkUsername, SdkPassword); // reset sdk user
        }

        [TestCleanup]
        public void Cleanup()
        {
            // cleanup of services initialized in Init method (the rest should be cleaned within specific Test method)
            _rabbitProducer.ProducersAlive.Clear();
            DetachFromFeedEvents(_feed);
            _feed.Close();
            _rabbitProducer.Stop();
        }

        [TestMethod]
        public void NormalRunTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            _rabbitProducer.AddProducersAlive(1);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            _feed.Open();
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null");

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.IsFalse(recoveryCalled.First().Contains("after"));
            Assert.IsTrue(recoveryCalled.First().Contains(producer.RecoveryInfo.RequestId.ToString()));

            // send 2 changeOdds and snapshotComplete
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);
            
            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer is not down", 250, 5000);
            Assert.IsFalse(producer.IsProducerDown);
            //Assert.AreEqual(0, producer.RecoveryInfo.RequestId); // maybe it should reset RequestId

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(2, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void NormalStartWithSetAfterTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            _rabbitProducer.AddProducersAlive(1);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            var afters = new Dictionary<int, DateTime>
                         {
                             { 1, DateTime.Now.AddMinutes(-10) },
                             { 3, DateTime.Now.AddMinutes(-10) }
                         };
            SetAfterTimestamp(afters, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 250, 5000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.IsTrue(recoveryCalled.First().Contains("after"));
            Assert.IsTrue(recoveryCalled.First().Contains(producer.RecoveryInfo.RequestId.ToString()));

            // send changeOdds and snapshotComplete
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer is not down", 250, 5000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(1, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void MultipleProducersWithAfterTest()
        {
            // setup for producer 1 and 3
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            _rabbitProducer.AddProducersAlive(1);
            _rabbitProducer.AddProducersAlive(3);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/prematch/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            var afters = new Dictionary<int, DateTime>
                         {
                             { 1, DateTime.Now.AddMinutes(-10) },
                             { 3, DateTime.Now.AddMinutes(-10) }
                         };
            SetAfterTimestamp(afters, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer1 = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer1);
            Assert.IsNull(producer1.RecoveryInfo);
            Assert.IsTrue(producer1.IsProducerDown);
            var producer3 = _feed.ProducerManager.Get(3);
            Assert.IsNotNull(producer3);
            Assert.IsNull(producer3.RecoveryInfo);
            Assert.IsTrue(producer3.IsProducerDown);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer1.RecoveryInfo != null, "Producer 1 recovery info is not null", 250);
            WaitAndCheckTillTimeout(() => producer3.RecoveryInfo != null, "Producer 3 recovery info is not null", 250);

            Assert.IsNotNull(producer1.RecoveryInfo);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsNotNull(producer3.RecoveryInfo);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            var recoveryCalled1 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled1.Count);
            Assert.IsTrue(recoveryCalled1.First().Contains("after"));
            Assert.IsTrue(recoveryCalled1.First().Contains(producer1.RecoveryInfo.RequestId.ToString())); 
            var recoveryCalled3 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/pre/recovery/"));
            Assert.AreEqual(1, recoveryCalled3.Count);
            Assert.IsTrue(recoveryCalled3.First().Contains("after"));
            Assert.IsTrue(recoveryCalled3.First().Contains(producer3.RecoveryInfo.RequestId.ToString()));

            // send changeOdds and snapshotComplete
            var oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 3, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            Assert.AreNotEqual(producer1.RecoveryInfo.RequestId, producer3.RecoveryInfo.RequestId);
            var snapshotComplete1 = _fMessageBuilder.BuildSnapshotComplete(producer1.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete1);
            var snapshotComplete3 = _fMessageBuilder.BuildSnapshotComplete(producer3.RecoveryInfo.RequestId, 3);
            _rabbitProducer.Send(snapshotComplete3);

            WaitAndCheckTillTimeout(() => !producer1.IsProducerDown, "Producer 1 is not down", 250, 5000);
            WaitAndCheckTillTimeout(() => !producer3.IsProducerDown, "Producer 3 is not down", 250, 5000);
            Assert.IsFalse(producer1.IsProducerDown);
            Assert.IsFalse(producer3.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer Ctrl is up")));

            Assert.AreEqual(3, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void MultipleProducersMultiSessionTest()
        {
            // setup for producer 1 and 3, with 3 sessions (Live, Pre and Virtuals)
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            _rabbitProducer.AddProducersAlive(1);
            _rabbitProducer.AddProducersAlive(3);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/prematch/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            var afters = new Dictionary<int, DateTime>
                         {
                             { 1, DateTime.Now.AddMinutes(-10) },
                             { 3, DateTime.Now.AddMinutes(-10) }
                         };
            SetAfterTimestamp(afters, _feed);

            AttachToFeedEvents(_feed);

            var liveSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.LiveMessagesOnly).Build();
            var liveMessageProcessor = new SimpleMessageProcessor(liveSession);
            liveMessageProcessor.Open();
            var prematchSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.PrematchMessagesOnly).Build();
            var prematchMessageProcessor = new SimpleMessageProcessor(prematchSession);
            prematchMessageProcessor.Open();
            var virtualSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.VirtualSportMessages).Build();
            var virtualMessageProcessor = new SimpleMessageProcessor(virtualSession);
            virtualMessageProcessor.Open();

            var producer1 = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer1);
            Assert.IsNull(producer1.RecoveryInfo);
            Assert.IsTrue(producer1.IsProducerDown);
            var producer3 = _feed.ProducerManager.Get(3);
            Assert.IsNotNull(producer3);
            Assert.IsNull(producer3.RecoveryInfo);
            Assert.IsTrue(producer3.IsProducerDown);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer1.RecoveryInfo != null, "Producer 1 recovery info is not null", 250);
            WaitAndCheckTillTimeout(() => producer3.RecoveryInfo != null, "Producer 3 recovery info is not null", 250);

            Assert.AreEqual(2, _feed.RecoveryDataPoster.CalledUrls.Count);
            Assert.IsNotNull(producer1.RecoveryInfo);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsNotNull(producer3.RecoveryInfo);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            var recoveryCalled1 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled1.Count);
            Assert.IsTrue(recoveryCalled1.First().Contains("after"));
            Assert.IsTrue(recoveryCalled1.First().Contains(producer1.RecoveryInfo.RequestId.ToString()));
            var recoveryCalled3 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/pre/recovery/"));
            Assert.AreEqual(1, recoveryCalled3.Count);
            Assert.IsTrue(recoveryCalled3.First().Contains("after"));
            Assert.IsTrue(recoveryCalled3.First().Contains(producer3.RecoveryInfo.RequestId.ToString()));

            // send changeOdds and snapshotComplete
            var oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 3, producer3.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            Assert.AreNotEqual(producer1.RecoveryInfo.RequestId, producer3.RecoveryInfo.RequestId);
            var snapshotComplete1 = _fMessageBuilder.BuildSnapshotComplete(producer1.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete1);
            var snapshotComplete3 = _fMessageBuilder.BuildSnapshotComplete(producer3.RecoveryInfo.RequestId, 3);
            _rabbitProducer.Send(snapshotComplete3);

            WaitAndCheckTillTimeout(() => !producer1.IsProducerDown, "Producer 1 is not down", 250, 5000);
            WaitAndCheckTillTimeout(() => !producer3.IsProducerDown, "Producer 3 is not down", 250, 5000);
            Assert.IsFalse(producer1.IsProducerDown);
            Assert.IsFalse(producer3.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(6, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer Ctrl is up")));

            Assert.AreEqual(2, liveMessageProcessor.FeedMessages.Count);
            Assert.AreEqual(1, prematchMessageProcessor.FeedMessages.Count);
            Assert.AreEqual(0, virtualMessageProcessor.FeedMessages.Count);
            liveMessageProcessor.Close();
            virtualMessageProcessor.Close();
            virtualMessageProcessor.Close();
        }

        [TestMethod]
        public void RecoveryUnsuccessfulRetryTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done (at first unsuccessful - testing if it retries)
            // wait till snapshotComplete arrives and check if all good
            //_rabbitProducer.AddProducersAlive(1);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.BadGateway)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            _feed.Open();

            Thread.Sleep(2000);

            // setup for recovery request and fails
            Helper.WriteToOutput("Waiting for first recovery call");
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 1000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/")); // failed one
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.IsTrue(recoveryCalled.First().Contains(producer.RecoveryInfo.RequestId.ToString()));
            Assert.AreEqual(1, _calledEvents.Count(c=>c.Contains($"RequestId={producer.RecoveryInfo.RequestId}")));
            Assert.AreEqual(1, _calledEvents.Count(c=> c.Contains("Full odds recovery") && c.Contains("failed")));
            var oldRecoveryRequestId = producer.RecoveryInfo.RequestId;

            Thread.Sleep(_config.MinIntervalBetweenRecoveryRequests * 1000);

            // call second one and fail
            Helper.WriteToOutput("Waiting for second recovery call");
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            var id = oldRecoveryRequestId;
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo.RequestId != id, "Producer recovery info is not null", 1000, 20000);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/")); // failed one
            Assert.AreEqual(2, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.IsTrue(recoveryCalled.Any(c => c.Contains(producer.RecoveryInfo.RequestId.ToString())), $"RequestId={ producer.RecoveryInfo.RequestId}");
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Full odds recovery") && c.Contains("failed")));
            oldRecoveryRequestId = producer.RecoveryInfo.RequestId;

            // send 2 changeOdds and snapshotComplete for old recovery request id - should not trigger producerUp
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, oldRecoveryRequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, oldRecoveryRequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(oldRecoveryRequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer is not down", 250, 5000);
            Assert.IsTrue(producer.IsProducerDown);

            Thread.Sleep(_config.MinIntervalBetweenRecoveryRequests * 1000);

            // now get the valid recovery and complete it
            _feed.RecoveryDataPoster.PostResponses.Clear();
            //_feed.RecoveryDataPoster.CalledUrls.Clear();
            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            Helper.WriteToOutput("Waiting for third recovery call");
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo.RequestId != oldRecoveryRequestId, "Producer recovery info is not null", 1000, 20000);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/")); // ok one
            Assert.AreEqual(3, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Full odds recovery") && c.Contains("failed")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains(producer.RecoveryInfo.RequestId.ToString()) && c.Contains("After=0")));

            // send 2 changeOdds and snapshotComplete for new recovery request id - should trigger producerUp
            oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer 1 is not down", 1000, 20000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(4, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(4, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void ProducerNoAlivesMakeRecoveryTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done (at first unsuccessful - testing if it retries)
            // wait till snapshotComplete arrives and check if all good
            //_rabbitProducer.AddProducersAlive(1);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            _feed.Open();

            // check if any recovery is done (without alive message)
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 10000, 75000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(1, recoveryCalled.Count(c => c.Contains(producer.RecoveryInfo.RequestId.ToString())));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"RequestId={producer.RecoveryInfo.RequestId}")));

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer 1 is not down", 250);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(2, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void UnsuccessfulRecoveryDoesNotRepeatToOftenTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done (at first unsuccessful - testing if it retries)
            // wait till snapshotComplete arrives and check if all good
            _rabbitProducer.AddProducersAlive(1, 2000);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.NotAcceptable)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            _feed.Open();
            
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 500, 5000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(1, recoveryCalled.Count(c => c.Contains(producer.RecoveryInfo.RequestId.ToString())));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"RequestId={producer.RecoveryInfo.RequestId}")));

            Thread.Sleep(30000); // wait till new recovery request can be done
            _feed.RecoveryDataPoster.PostResponses.Clear();
            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));
            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(2, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(1, recoveryCalled.Count(c=>c.Contains(producer.RecoveryInfo.RequestId.ToString())));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Recovery initiated") && c.Contains("failed")));

            WaitAndCheckTillTimeout(() => _calledEvents.Count(c => c.Contains($"RequestId={producer.RecoveryInfo.RequestId}")) == 3, "Producer recovery info is not null", 500, 30000);

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer 1 is not down", 250);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(2, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void AliveSubscribedFalseTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            // when alive message with subscribed=0 comes, producer goes down
            _rabbitProducer.AddProducersAlive(1, 5000);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 1000, 5000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}")));

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer 1 is not down", 1000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            var oldRequestId = producer.RecoveryInfo.RequestId;

            _rabbitProducer.ProducersAlive.Clear();
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1, null, false));

            WaitAndCheckTillTimeout(() => producer.IsProducerDown, "Producer 1 is down", 3000, 30000);
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is down")));

            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1, null, false));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));

            // reset alives and check all
            _rabbitProducer.AddProducersAlive(1, 5000);
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));

            WaitAndCheckTillTimeout(() => _calledEvents.Count(c => c.Contains("Recovery initiated")) == 2, "Producer second recovery not initiated", 3000, 60000);
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreNotEqual(oldRequestId, producer.RecoveryInfo.RequestId);

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, $"Producer 1 is not down ({producer.IsProducerDown})", 2000, 30000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(4, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated") && c.Contains(oldRequestId.ToString()) && c.Contains("After=0,"))); // initial request
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}") && !c.Contains("After=0,"))); // second request
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(4, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void ConnectionBreakTest()
        {
            // setup for producer 1
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            // then connection drops and should reconnect
            _rabbitProducer.AddProducersAlive(1, 5000);
            var userChanged = _rabbitProducer.RabbitChangeUserPassword(SdkUsername, SdkPassword); // reset sdk rabbit connection user
            Assert.IsTrue(userChanged);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            OperationManager.SetRabbitHeartbeat(20);
            OperationManager.SetRabbitConnectionTimeout(10);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 1000, 20000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}")));

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer 1 is not down", 1000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            var oldRequestId = producer.RecoveryInfo.RequestId;

            //var user = _rabbitProducer.ManagementClient.ChangeUserPassword("testuser", "testpassChanged");
            //Assert.IsNotNull(user);
            var connections = _rabbitProducer.ManagementClient.GetConnections();
            Assert.AreEqual(2, connections.Count); // producer and sdk connection
            var sdkConnection = connections.First(f => f.User.Equals(SdkUsername));
            var channels = _rabbitProducer.ManagementClient.GetChannelsAsync(sdkConnection).Result;
            Assert.AreEqual(2, channels.Count);

            // close connection and wait to auto restart
            userChanged = _rabbitProducer.RabbitChangeUserPassword(SdkUsername, "testpass67"); // disable sdk rabbit connection user
            Assert.IsTrue(userChanged);
            _rabbitProducer.ManagementClient.CloseConnection(sdkConnection);
            Thread.Sleep(2000);
            connections = _rabbitProducer.ManagementClient.GetConnectionsAsync().Result;
            Assert.AreEqual(1, connections.Count); // producer connection only

            WaitAndCheckTillTimeout(() => _calledEvents.Count(c => c.Contains("Connection to the feed lost")) == 1, "Connection drop event called", 3000, 60000);
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is down"))); // producers are marked down
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Connection to the feed lost"))); //Connection to the feed lost
            
            _calledEvents.Clear();

            // reset user so sdk connection can be made
            userChanged = _rabbitProducer.RabbitChangeUserPassword(SdkUsername, SdkPassword);
            Assert.IsTrue(userChanged);

            // reset alives and check all
            WaitAndCheckTillTimeout(() => _calledEvents.Count(c => c.Contains("Recovery initiated")) == 1, "Producer second recovery not initiated", 3000, 60000);
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreNotEqual(oldRequestId, producer.RecoveryInfo.RequestId);

            // check for new connection is done and new alives arrive
            connections = _rabbitProducer.ManagementClient.GetConnectionsAsync().Result;
            Assert.AreEqual(2, connections.Count); // producer and sdk connection
            sdkConnection = connections.First(f => f.User.Equals(SdkUsername));
            channels = _rabbitProducer.ManagementClient.GetChannelsAsync(sdkConnection).Result;
            Assert.AreEqual(2, channels.Count);

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, $"Producer 1 is not down ({producer.IsProducerDown})", 2000, 30000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(0, _calledEvents.Count(c => c.Contains("Recovery initiated") && c.Contains(oldRequestId.ToString()) && c.Contains("After=0,"))); // initial request
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}") && !c.Contains("After=0,"))); // second request
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(4, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        [TestMethod]
        public void ConnectionBreakMultiSessionTest()
        {
            // setup for producer 1, 3, 6 and 3 different session
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            // then connection drops and should reconnect
            _rabbitProducer.AddProducersAlive(1, 5000);
            _rabbitProducer.AddProducersAlive(3, 6000);
            _rabbitProducer.AddProducersAlive(6, 7000);
            var userChanged = _rabbitProducer.RabbitChangeUserPassword(SdkUsername, SdkPassword); // reset sdk rabbit connection user
            Assert.IsTrue(userChanged);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));
            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/pre/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));
            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/vf/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var liveSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.LiveMessagesOnly).Build();
            var liveMessageProcessor = new SimpleMessageProcessor(liveSession);
            liveMessageProcessor.Open();
            var prematchSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.PrematchMessagesOnly).Build();
            var prematchMessageProcessor = new SimpleMessageProcessor(prematchSession);
            prematchMessageProcessor.Open();
            var virtualSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.VirtualSportMessages).Build();
            var virtualMessageProcessor = new SimpleMessageProcessor(virtualSession);
            virtualMessageProcessor.Open();

            var producer1 = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer1);
            Assert.IsNull(producer1.RecoveryInfo);
            Assert.IsTrue(producer1.IsProducerDown);
            var producer3 = _feed.ProducerManager.Get(3);
            Assert.IsNotNull(producer3);
            Assert.IsNull(producer3.RecoveryInfo);
            Assert.IsTrue(producer3.IsProducerDown);

            var producer6 = _feed.ProducerManager.Get(6);
            Assert.IsNotNull(producer6);
            Assert.IsNull(producer6.RecoveryInfo);
            Assert.IsTrue(producer6.IsProducerDown);

            OperationManager.SetRabbitHeartbeat(20);
            OperationManager.SetRabbitConnectionTimeout(10);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer1.RecoveryInfo != null, "Producer 1 recovery info is not null", 1000, 20000);
            WaitAndCheckTillTimeout(() => producer3.RecoveryInfo != null, "Producer 3 recovery info is not null", 1000, 20000);
            WaitAndCheckTillTimeout(() => producer6.RecoveryInfo != null, "Producer 6 recovery info is not null", 1000, 20000);

            Assert.AreEqual(3, _feed.RecoveryDataPoster.CalledUrls.Count);
            Assert.IsNotNull(producer1.RecoveryInfo);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsNotNull(producer3.RecoveryInfo);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            Assert.IsNotNull(producer6.RecoveryInfo);
            Assert.IsTrue(producer6.IsProducerDown);
            Assert.IsTrue(producer6.RecoveryInfo.RequestId > 0);
            var recoveryCalled1 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled1.Count);
            Assert.AreEqual(1, recoveryCalled1.Count(c => c.Contains($"request_id={producer1.RecoveryInfo.RequestId}") && !c.Contains("after")));
            var recoveryCalled3 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/pre/recovery/"));
            Assert.AreEqual(1, recoveryCalled3.Count);
            Assert.AreEqual(1, recoveryCalled3.Count(c => c.Contains($"request_id={producer3.RecoveryInfo.RequestId}") && !c.Contains("after")));
            var recoveryCalled6 = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/vf/recovery/"));
            Assert.AreEqual(1, recoveryCalled6.Count);
            Assert.AreEqual(1, recoveryCalled6.Count(c => c.Contains($"request_id={producer6.RecoveryInfo.RequestId}") && !c.Contains("after")));

            // send changeOdds and snapshotComplete
            var oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 3, producer3.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 6, producer6.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            Assert.IsTrue(producer6.IsProducerDown);
            Assert.IsTrue(producer6.RecoveryInfo.RequestId > 0);
            Assert.AreNotEqual(producer1.RecoveryInfo.RequestId, producer3.RecoveryInfo.RequestId);
            Assert.AreNotEqual(producer3.RecoveryInfo.RequestId, producer6.RecoveryInfo.RequestId);
            Assert.AreNotEqual(producer1.RecoveryInfo.RequestId, producer6.RecoveryInfo.RequestId);
            var snapshotComplete1 = _fMessageBuilder.BuildSnapshotComplete(producer1.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete1);
            var snapshotComplete3 = _fMessageBuilder.BuildSnapshotComplete(producer3.RecoveryInfo.RequestId, 3);
            _rabbitProducer.Send(snapshotComplete3);
            var snapshotComplete6 = _fMessageBuilder.BuildSnapshotComplete(producer6.RecoveryInfo.RequestId, 6);
            _rabbitProducer.Send(snapshotComplete6);

            WaitAndCheckTillTimeout(() => !producer1.IsProducerDown, "Producer 1 is not down", 1000);
            Assert.IsFalse(producer1.IsProducerDown);
            WaitAndCheckTillTimeout(() => !producer3.IsProducerDown, "Producer 3 is not down", 1000);
            Assert.IsFalse(producer3.IsProducerDown);
            WaitAndCheckTillTimeout(() => !producer6.IsProducerDown, "Producer 6 is not down", 1000);
            Assert.IsFalse(producer6.IsProducerDown);
            var oldRequestId1 = producer1.RecoveryInfo.RequestId;

            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer Ctrl is up")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer VF is up")));
            
            var connections = _rabbitProducer.ManagementClient.GetConnections();
            Assert.AreEqual(2, connections.Count); // producer and sdk connection
            var sdkConnection = connections.First(f => f.User.Equals(SdkUsername));
            var channels = _rabbitProducer.ManagementClient.GetChannelsAsync(sdkConnection).Result;
            Assert.AreEqual(4, channels.Count);

            // close connection and wait to auto restart
            userChanged = _rabbitProducer.RabbitChangeUserPassword(SdkUsername, "testpass67"); // disable sdk rabbit connection user
            Assert.IsTrue(userChanged);
            _rabbitProducer.ManagementClient.CloseConnection(sdkConnection);
            Thread.Sleep(2000);
            connections = _rabbitProducer.ManagementClient.GetConnectionsAsync().Result;
            Assert.AreEqual(1, connections.Count); // producer connection only

            WaitAndCheckTillTimeout(() => _calledEvents.Count(c => c.Contains("Connection to the feed lost")) == 1, "Connection drop event called", 3000, 60000);
            Thread.Sleep(1000); // so all events can be called
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is down"))); // producers are marked down
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer Ctrl is down")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer VF is down")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Connection to the feed lost"))); //Connection to the feed lost

            _calledEvents.Clear();

            // reset user so sdk connection can be made
            userChanged = _rabbitProducer.RabbitChangeUserPassword(SdkUsername, SdkPassword);
            Assert.IsTrue(userChanged);

            // reset alives and check all
            WaitAndCheckTillTimeout(() => _calledEvents.Count(c => c.Contains("Recovery initiated")) == 3, "Producers second recovery not initiated", 3000, 60000);
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer1.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer3.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer6.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreNotEqual(oldRequestId1, producer1.RecoveryInfo.RequestId);

            // check for new connection is done and new alives arrive
            connections = _rabbitProducer.ManagementClient.GetConnectionsAsync().Result;
            Assert.AreEqual(2, connections.Count); // producer and sdk connection
            sdkConnection = connections.First(f => f.User.Equals(SdkUsername));
            channels = _rabbitProducer.ManagementClient.GetChannelsAsync(sdkConnection).Result;
            Assert.AreEqual(4, channels.Count);

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 3, producer3.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer1.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 6, producer6.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer1.IsProducerDown);
            Assert.IsTrue(producer1.RecoveryInfo.RequestId > 0);
            Assert.IsTrue(producer3.IsProducerDown);
            Assert.IsTrue(producer3.RecoveryInfo.RequestId > 0);
            Assert.IsTrue(producer6.IsProducerDown);
            Assert.IsTrue(producer6.RecoveryInfo.RequestId > 0);
            Assert.AreNotEqual(producer1.RecoveryInfo.RequestId, producer3.RecoveryInfo.RequestId);
            Assert.AreNotEqual(producer3.RecoveryInfo.RequestId, producer6.RecoveryInfo.RequestId);
            Assert.AreNotEqual(producer1.RecoveryInfo.RequestId, producer6.RecoveryInfo.RequestId);
            snapshotComplete1 = _fMessageBuilder.BuildSnapshotComplete(producer1.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete1);
            snapshotComplete3 = _fMessageBuilder.BuildSnapshotComplete(producer3.RecoveryInfo.RequestId, 3);
            _rabbitProducer.Send(snapshotComplete3);
            snapshotComplete6 = _fMessageBuilder.BuildSnapshotComplete(producer6.RecoveryInfo.RequestId, 6);
            _rabbitProducer.Send(snapshotComplete6);

            WaitAndCheckTillTimeout(() => !producer1.IsProducerDown, "Producer 1 is not down", 1000);
            Assert.IsFalse(producer1.IsProducerDown);
            WaitAndCheckTillTimeout(() => !producer3.IsProducerDown, "Producer 3 is not down", 1000);
            Assert.IsFalse(producer3.IsProducerDown);
            WaitAndCheckTillTimeout(() => !producer6.IsProducerDown, "Producer 6 is not down", 1000);
            Assert.IsFalse(producer6.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(4, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change"))); // because we deleted previous called events
            Assert.AreEqual(9, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete"))); // comes on all sessions
            Assert.AreEqual(0, _calledEvents.Count(c => c.Contains("Recovery initiated") && c.Contains(oldRequestId1.ToString()) && c.Contains("After=0,"))); // initial request
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer1.RecoveryInfo.RequestId}") && !c.Contains("After=0,"))); // second request
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer3.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer6.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer Ctrl is up")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer VF is up")));

            Assert.AreEqual(4, liveMessageProcessor.FeedMessages.Count);
            Assert.AreEqual(2, prematchMessageProcessor.FeedMessages.Count);
            Assert.AreEqual(2, virtualMessageProcessor.FeedMessages.Count);
            liveMessageProcessor.Close();
            virtualMessageProcessor.Close();
            virtualMessageProcessor.Close();
        }

        [TestMethod]
        public void ChannelNoMessagesTest()
        {
            // connection started but no message arrive on channel
            // open feed and check that recovery was done
            // wait till snapshotComplete arrives and check if all good
            // then connection drops and should reconnect
            var producerId = 1;
            _rabbitProducer.AddProducersAlive(producerId, 5000);

            Assert.IsTrue(_calledEvents.IsNullOrEmpty());

            _feed.RecoveryDataPoster.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>("/liveodds/", 0, new HttpResponseMessage(HttpStatusCode.Accepted)));

            var disabledProducers = new List<int> { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
            DisableProducers(disabledProducers, _feed);

            AttachToFeedEvents(_feed);

            var allSession = _feed.CreateBuilder().SetMessageInterest(MessageInterest.AllMessages).Build();
            var simpleMessageProcessor = new SimpleMessageProcessor(allSession);
            simpleMessageProcessor.Open();

            var producer = _feed.ProducerManager.Get(1);
            Assert.IsNotNull(producer);
            Assert.IsNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);

            OperationManager.SetRabbitHeartbeat(20);
            OperationManager.SetRabbitConnectionTimeout(10);

            _feed.Open();

            WaitAndCheckTillTimeout(() => producer.RecoveryInfo != null, "Producer recovery info is not null", 1000, 20000);

            Assert.IsNotNull(producer.RecoveryInfo);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var recoveryCalled = _feed.RecoveryDataPoster.CalledUrls.FindAll(f => f.Contains("/liveodds/recovery/"));
            Assert.AreEqual(1, recoveryCalled.Count);
            Assert.AreEqual(0, recoveryCalled.Count(c => c.Contains("after")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}")));
            var recoveryRequestId1 = producer.RecoveryInfo.RequestId;

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            _rabbitProducer.Send(_fMessageBuilder.BuildAlive(1));
            var oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            var snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, "Producer is not down", 1000);
            Assert.IsFalse(producer.IsProducerDown);

            _rabbitProducer.ProducersAlive.Clear();

            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is up")));
            
            var connections = _rabbitProducer.ManagementClient.GetConnections();
            Assert.AreEqual(2, connections.Count); // producer and sdk connection
            var sdkConnection = connections.First(f => f.User.Equals(SdkUsername));
            var channels = _rabbitProducer.ManagementClient.GetChannelsAsync(sdkConnection).Result;
            Assert.AreEqual(2, channels.Count);
            var channelId1 = channels[0].Name;
            var channelId2 = channels[1].Name;
            Assert.AreNotEqual(channelId1, channelId2);

            Thread.Sleep(TimeSpan.FromSeconds(200)); // timeout for creating new channel
            //_rabbitProducer.AddProducersAlive(1, 5000);
            //Thread.Sleep(TimeSpan.FromSeconds(30));

            // new channel should be made
            connections = _rabbitProducer.ManagementClient.GetConnectionsAsync().Result;
            Assert.AreEqual(2, connections.Count); // producer connection only
            sdkConnection = connections.First(f => f.User.Equals(SdkUsername));
            channels = _rabbitProducer.ManagementClient.GetChannelsAsync(sdkConnection).Result;
            Assert.AreEqual(2, channels.Count);
            Assert.AreNotEqual(channels[0].Name, channels[1].Name);
            
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Producer LO is down"))); // producers are marked down
            Assert.AreEqual(0, _calledEvents.Count(c => c.Contains("Connection to the feed lost"))); //Connection to the feed lost

            // reset alives and wait for recovery
            _rabbitProducer.AddProducersAlive(producerId, 3000);
            WaitAndCheckTillTimeout(() => producer.RecoveryInfo.RequestId != recoveryRequestId1, "Producer new recovery is done", 1000, 20000);
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}") && !c.Contains("After=0,")));
            Assert.AreNotEqual(recoveryRequestId1, producer.RecoveryInfo.RequestId);

            // send 2 changeOdds and snapshotComplete for recovery request id - should not trigger producerUp because recovery was interrupted because of alive timeout
            oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, $"Producer is not down ({producer.IsProducerDown})", 2000, 8000);
            Assert.IsTrue(producer.IsProducerDown); // true because previous recovery was interrupted because of alive timeout

            // send 2 changeOdds and snapshotComplete for recovery request id - should trigger producerUp
            oddsChange = _fMessageBuilder.BuildOddsChange(null, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            oddsChange = _fMessageBuilder.BuildOddsChange(-1, 1, producer.RecoveryInfo.RequestId);
            _rabbitProducer.Send(oddsChange);
            Assert.IsTrue(producer.IsProducerDown);
            Assert.IsTrue(producer.RecoveryInfo.RequestId > 0);
            snapshotComplete = _fMessageBuilder.BuildSnapshotComplete(producer.RecoveryInfo.RequestId, 1);
            _rabbitProducer.Send(snapshotComplete);

            WaitAndCheckTillTimeout(() => !producer.IsProducerDown, $"Producer is not down ({producer.IsProducerDown})", 2000, 30000);
            Assert.IsFalse(producer.IsProducerDown);

            Assert.IsFalse(_calledEvents.IsNullOrEmpty());
            Assert.AreEqual(6, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("odds_change")));
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Raw feed data") && c.Contains("snapshot_complete")));
            Assert.AreEqual(3, _calledEvents.Count(c => c.Contains("Recovery initiated")));
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains("Recovery initiated") && c.Contains(recoveryRequestId1.ToString()) && c.Contains("After=0,"))); // initial request
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Recovery initiated. RequestId=") && !c.Contains("After=0,"))); 
            Assert.AreEqual(1, _calledEvents.Count(c => c.Contains($"Recovery initiated. RequestId={producer.RecoveryInfo.RequestId}") && !c.Contains("After=0,"))); // second request
            Assert.AreEqual(2, _calledEvents.Count(c => c.Contains("Producer LO is up")));

            Assert.AreEqual(6, simpleMessageProcessor.FeedMessages.Count);
            simpleMessageProcessor.Close();
        }

        private void WaitAndCheckTillTimeout(Func<bool> condition, string message = null, int checkPeriod = 500, int timeout= 10000)
        {
            WaitAndCheckTillTimeout(condition, message, TimeSpan.FromMilliseconds(checkPeriod), TimeSpan.FromMilliseconds(timeout));
        }

        private void WaitAndCheckTillTimeout(Func<bool> condition, string message, TimeSpan checkPeriod, TimeSpan timeout)
        {
            if (!message.IsNullOrEmpty())
            {
                message = $"'{message}' ";
            }
            var start = DateTime.Now;
            while (DateTime.Now - start < timeout)
            {
                try
                {
                    if (condition.Invoke())
                    {
                        return;
                    }
                    Helper.WriteToOutput($"Condition {message}not true (yet) ({(DateTime.Now-start).TotalMilliseconds:0000}ms)");
                }
                catch
                {
                    Helper.WriteToOutput($"Condition {message}not true yet (error) ({(DateTime.Now - start).TotalMilliseconds:0000}ms)");
                }
                Thread.Sleep(checkPeriod);
            }
            Helper.WriteToOutput($"Condition {message}not true (final) ({(DateTime.Now - start).TotalMilliseconds:0000}ms)");
        }

        private void DisableProducers(List<int> producerIds, IOddsFeed feed)
        {
            if (producerIds.IsNullOrEmpty())
            {
                return;
            }

            foreach (var producerId in producerIds)
            {
                feed.ProducerManager.DisableProducer(producerId);
            }
        }

        private void SetAfterTimestamp(Dictionary<int, DateTime> afters, IOddsFeed feed)
        {
            if (afters.IsNullOrEmpty())
            {
                return;
            }

            foreach (var after in afters)
            {
                feed.ProducerManager.AddTimestampBeforeDisconnect(after.Key, after.Value);
            }
        }

        private void AttachToFeedEvents(IOddsFeedExt oddsFeed)
        {
            oddsFeed.ProducerUp += OnProducerUp;
            oddsFeed.ProducerDown += OnProducerDown;
            oddsFeed.Disconnected += OnDisconnected;
            oddsFeed.Closed += OnClosed;
            oddsFeed.EventRecoveryCompleted += OnEventRecoveryCompleted;
            oddsFeed.RawFeedMessageReceived += OnRawFeedMessageReceived;
            oddsFeed.RawApiDataReceived += OnRawApiDataReceived;
            oddsFeed.RecoveryInitiated += OnRecoveryInitiated;
        }

        private void DetachFromFeedEvents(IOddsFeedExt oddsFeed)
        {
            oddsFeed.ProducerUp -= OnProducerUp;
            oddsFeed.ProducerDown -= OnProducerDown;
            oddsFeed.Disconnected -= OnDisconnected;
            oddsFeed.Closed -= OnClosed;
            oddsFeed.EventRecoveryCompleted -= OnEventRecoveryCompleted;
            oddsFeed.RawFeedMessageReceived -= OnRawFeedMessageReceived;
            oddsFeed.RawApiDataReceived -= OnRawApiDataReceived;
            oddsFeed.RecoveryInitiated -= OnRecoveryInitiated;
        }
        
        private void OnProducerUp(object sender, ProducerStatusChangeEventArgs e)
        {
            var message = $"Producer {e.GetProducerStatusChange().Producer.Name} is up";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnProducerUp: {message}");
        }
        
        private void OnProducerDown(object sender, ProducerStatusChangeEventArgs e)
        {
            var message = $"Producer {e.GetProducerStatusChange().Producer.Name} is down";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnProducerDown: {message}");
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            var message = "Connection to the feed lost";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnDisconnected: {message}");
        }

        private void OnClosed(object sender, FeedCloseEventArgs feedCloseEventArgs)
        {
            var message = $"Feed closed with the reason: {feedCloseEventArgs.GetReason()}";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnClosed: {message}");
        }

        private void OnEventRecoveryCompleted(object sender, EventRecoveryCompletedEventArgs e)
        {
            var message = $"Event recovery completed. EventId={e.GetEventId()}, RequestId={e.GetRequestId()}";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnEventRecoveryCompleted: {message}");
        }

        private void OnRawFeedMessageReceived(object sender, RawFeedMessageEventArgs e)
        {
            var message = $"Raw feed data [{e.MessageInterest}]: key={e.RoutingKey}, data={e.FeedMessage}";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnRawFeedMessageReceived: {message}");
        }

        private void OnRawApiDataReceived(object sender, RawApiDataEventArgs e)
        {
            var message = $"Raw api data: uri={e.Uri}, data={e.RestMessage}";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnRawApiDataReceived: {message}");
        }

        private void OnRecoveryInitiated(object sender, RecoveryInitiatedEventArgs e)
        {
            var message = $"Recovery initiated. RequestId={e.GetRequestId()}, Producer={e.GetProducerId()}, After={e.GetAfterTimestamp()}, EventId={e.GetEventId()}, Message={e.GetMessage()}";
            _calledEvents.Add(message);
            Helper.WriteToOutput($"Called event OnRecoveryInitiated: {message}");
        }
    }
}
