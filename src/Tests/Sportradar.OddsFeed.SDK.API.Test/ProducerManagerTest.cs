/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class ProducerManagerTest
    {
        [TestMethod]
        public void ProducerManagerInitTest()
        {
            var producerManager = TestProducerManager.Create();
            Assert.IsNotNull(producerManager);
            Assert.IsNotNull(producerManager.Producers);
            Assert.IsTrue(producerManager.Producers.Any());
        }

        [TestMethod]
        public void UnknownProducerTest()
        {
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(50);
            Assert.IsNotNull(producer);
            Assert.AreEqual(99, producer.Id);
            Assert.AreEqual(true, producer.IsAvailable);
            Assert.AreEqual(false, producer.IsDisabled);
            Assert.AreEqual(true, producer.IsProducerDown);
            Assert.AreEqual("Unknown", producer.Name);
        }

        [TestMethod]
        public void CompareProducersTest()
        {
            var producerManager = TestProducerManager.Create();

            var producer1 = producerManager.Get(1);
            var producer2 = new Producer(1, "Lo", "Live Odds", "lo", true, 60, 3600, "live", 600);
            Assert.IsNotNull(producer1);
            Assert.AreEqual(1, producer1.Id);
            Assert.IsNotNull(producer2);
            Assert.AreEqual(1, producer2.Id);
            Assert.AreEqual(producer1.IsAvailable, producer2.IsAvailable);
            Assert.AreEqual(producer1.IsDisabled, producer2.IsDisabled);
            Assert.AreEqual(producer1.IsProducerDown, producer2.IsProducerDown);
            Assert.AreEqual(producer1.Name, producer2.Name, true);
            Assert.IsTrue(producer1.Name.Equals(producer2.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.AreEqual(producer1, producer2);
        }

        [TestMethod]
        public void ProducerManagerGetByIdTest()
        {
            var producerManager = TestProducerManager.Create();

            var producer1 = producerManager.Get(1);
            Assert.IsNotNull(producer1);
            Assert.AreEqual(1, producer1.Id);
        }

        [TestMethod]
        public void ProducerManagerGetByNameTest()
        {
            var producerManager = TestProducerManager.Create();

            var producer1 = producerManager.Get("lo");
            Assert.IsNotNull(producer1);
            Assert.AreEqual(1, producer1.Id);
        }

        [TestMethod]
        public void ProducerManagerExistsByIdTest()
        {
            var producerManager = TestProducerManager.Create();

            var result = producerManager.Exists(1);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ProducerManagerExistsByNameTest()
        {
            var producerManager = TestProducerManager.Create();

            var result = producerManager.Exists("lo");
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ProducerManagerNotExistsByIdTest()
        {
            var producerManager = TestProducerManager.Create();

            var result = producerManager.Exists(11);
            Assert.IsTrue(!result);
        }

        [TestMethod]
        public void ProducerManagerUpdateTest()
        {
            var producerId = 1;
            var date = DateTime.Now;
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(producerId);
            CheckLiveOddsProducer(producer);

            producerManager.DisableProducer(producerId);
            Assert.AreEqual(true, producer.IsDisabled);

            producerManager.AddTimestampBeforeDisconnect(producerId, date);
            Assert.AreEqual(date, producer.LastTimestampBeforeDisconnect);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProducerManagerUpdateLocked01Test()
        {
            var producerId = 1;
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(producerId);
            CheckLiveOddsProducer(producer);

            ((ProducerManager)producerManager).Lock();
            producerManager.DisableProducer(producerId);
            Assert.AreEqual(true, producer.IsDisabled);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ProducerManagerUpdateLocked02Test()
        {
            var producerId = 1;
            var date = DateTime.Now;
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(producerId);
            CheckLiveOddsProducer(producer);

            ((ProducerManager)producerManager).Lock();
            producerManager.AddTimestampBeforeDisconnect(producerId, date);
            Assert.AreEqual(date, producer.LastTimestampBeforeDisconnect);
        }

        private void CheckLiveOddsProducer(IProducer producer)
        {
            Assert.IsNotNull(producer);
            Assert.AreEqual(1, producer.Id);
            Assert.AreEqual(true, producer.IsAvailable);
            Assert.AreEqual(false, producer.IsDisabled);
            Assert.AreEqual(true, producer.IsProducerDown);
            Assert.AreEqual("LO", producer.Name);
            Assert.AreEqual(DateTime.MinValue, producer.LastTimestampBeforeDisconnect);
        }
    }
}
