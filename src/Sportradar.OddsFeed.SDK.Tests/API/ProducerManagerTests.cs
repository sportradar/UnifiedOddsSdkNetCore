/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class ProducerManagerTests
    {
        [Fact]
        public void ProducerManagerInit()
        {
            var producerManager = TestProducerManager.Create();
            Assert.NotNull(producerManager);
            Assert.NotNull(producerManager.Producers);
            Assert.True(producerManager.Producers.Any());
        }

        [Fact]
        public void UnknownProducer()
        {
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(50);
            Assert.NotNull(producer);
            Assert.Equal(99, producer.Id);
            Assert.True(producer.IsAvailable);
            Assert.False(producer.IsDisabled);
            Assert.True(producer.IsProducerDown);
            Assert.Equal("Unknown", producer.Name);
        }

        [Fact]
        public void CompareProducers()
        {
            var producerManager = TestProducerManager.Create();

            var producer1 = producerManager.Get(1);
            var producer2 = new Producer(1, "Lo", "Live Odds", "lo", true, 60, 3600, "live", 600);
            Assert.NotNull(producer1);
            Assert.Equal(1, producer1.Id);
            Assert.NotNull(producer2);
            Assert.Equal(1, producer2.Id);
            Assert.Equal(producer1.IsAvailable, producer2.IsAvailable);
            Assert.Equal(producer1.IsDisabled, producer2.IsDisabled);
            Assert.Equal(producer1.IsProducerDown, producer2.IsProducerDown);
            Assert.Equal(producer1.Name, producer2.Name, true);
            Assert.True(producer1.Name.Equals(producer2.Name, StringComparison.InvariantCultureIgnoreCase));
            Assert.Equal(producer1, producer2);
        }

        [Fact]
        public void ProducerManagerGetById()
        {
            var producerManager = TestProducerManager.Create();

            var producer1 = producerManager.Get(1);
            Assert.NotNull(producer1);
            Assert.Equal(1, producer1.Id);
        }

        [Fact]
        public void ProducerManagerGetByName()
        {
            var producerManager = TestProducerManager.Create();

            var producer1 = producerManager.Get("lo");
            Assert.NotNull(producer1);
            Assert.Equal(1, producer1.Id);
        }

        [Fact]
        public void ProducerManagerExistsById()
        {
            var producerManager = TestProducerManager.Create();

            var result = producerManager.Exists(1);
            Assert.True(result);
        }

        [Fact]
        public void ProducerManagerExistsByName()
        {
            var producerManager = TestProducerManager.Create();

            var result = producerManager.Exists("lo");
            Assert.True(result);
        }

        [Fact]
        public void ProducerManagerNotExistsById()
        {
            var producerManager = TestProducerManager.Create();

            var result = producerManager.Exists(11);
            Assert.True(!result);
        }

        [Fact]
        public void ProducerManagerUpdateLocked01()
        {
            var producerId = 1;
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(producerId);
            CheckLiveOddsProducer(producer);
            ((ProducerManager)producerManager).Lock();

            Assert.Throws<InvalidOperationException>(() => producerManager.DisableProducer(producerId));
        }

        [Fact]
        public void ProducerManagerUpdateLocked02()
        {
            var producerId = 1;
            var date = DateTime.Now;
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(producerId);
            CheckLiveOddsProducer(producer);

            ((ProducerManager)producerManager).Lock();

            Assert.Throws<InvalidOperationException>(() => producerManager.AddTimestampBeforeDisconnect(producerId, date));
        }

        [Fact]
        public void ProducerManagerUpdate()
        {
            var producerId = 1;
            var date = DateTime.Now;
            var producerManager = TestProducerManager.Create();

            var producer = producerManager.Get(producerId);
            CheckLiveOddsProducer(producer);

            producerManager.DisableProducer(producerId);
            Assert.True(producer.IsDisabled);

            producerManager.AddTimestampBeforeDisconnect(producerId, date);
            Assert.Equal(date, producer.LastTimestampBeforeDisconnect);
        }

        // [Fact]
        // [ExpectedException(typeof(InvalidOperationException))]
        // public void ProducerManagerUpdateLocked01()
        // {
        //     var producerId = 1;
        //     var producerManager = TestProducerManager.Create();
        //
        //     var producer = producerManager.Get(producerId);
        //     CheckLiveOddsProducer(producer);
        //
        //     ((ProducerManager)producerManager).Lock();
        //     producerManager.DisableProducer(producerId);
        //     Assert.Equal(true, producer.IsDisabled);
        // }
        //
        // [Fact]
        // [ExpectedException(typeof(InvalidOperationException))]
        // public void ProducerManagerUpdateLocked02()
        // {
        //     var producerId = 1;
        //     var date = DateTime.Now;
        //     var producerManager = TestProducerManager.Create();
        //
        //     var producer = producerManager.Get(producerId);
        //     CheckLiveOddsProducer(producer);
        //
        //     ((ProducerManager)producerManager).Lock();
        //     producerManager.AddTimestampBeforeDisconnect(producerId, date);
        //     Assert.Equal(date, producer.LastTimestampBeforeDisconnect);
        // }

        private void CheckLiveOddsProducer(IProducer producer)
        {
            Assert.NotNull(producer);
            Assert.Equal(1, producer.Id);
            Assert.True(producer.IsAvailable);
            Assert.False(producer.IsDisabled);
            Assert.True(producer.IsProducerDown);
            Assert.Equal("LO", producer.Name);
            Assert.Equal(DateTime.MinValue, producer.LastTimestampBeforeDisconnect);
        }
    }
}
