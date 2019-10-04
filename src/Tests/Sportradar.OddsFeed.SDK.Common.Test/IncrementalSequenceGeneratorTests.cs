/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class IncrementalSequenceGeneratorTests
    {
        [TestMethod]
        public void TestIfValueIsIncrementedByOne()
        {
            var generator = new IncrementalSequenceGenerator();
            Assert.AreEqual(1, generator.GetNext(), "First value should be 1");
            Assert.AreEqual(2, generator.GetNext(), "First value should be 2");
            Assert.AreEqual(3, generator.GetNext(), "First value should be 3");
        }

        [TestMethod]
        public void TestMinAndMaxAreRespected()
        {
            var min = 3;
            var max = 11;
            var generator = new IncrementalSequenceGenerator(min, max);

            long value = 0;
            for (var i = min; i < max; i++)
            {
                value = generator.GetNext();
            }
            Assert.AreEqual(min, value, $"The value should be {min}");
        }

        [TestMethod]
        public void TestGetNextIsThreadSafe()
        {
            const int min = -11;
            const int max = 543;
            const int iterations = 8791;

            var generator = new IncrementalSequenceGenerator(min, max);
            var tasks = new List<Task>();
            for (var i = 0; i < iterations; i++)
            {
                tasks.Add(Task.Run(() => generator.GetNext()));
            }

            Task.WaitAll(tasks.ToArray());

            const int range = max - min;
            const int reminder = iterations%range;
            var value = generator.GetNext();
            Assert.AreEqual(reminder + min + 1, value, $"The value should be {reminder + min + 1}");
        }
    }
}
