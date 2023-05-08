using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class SdkInfoTests
    {
        [Fact]
        public void MinInactivitySecondsPositive()
        {
            Assert.True(SdkInfo.MinInactivitySeconds > 0);
        }

        [Fact]
        public void MaxInactivitySecondsPositive()
        {
            Assert.True(SdkInfo.MaxInactivitySeconds > 0);
        }

        [Fact]
        public void MinInactivitySecondsLessThenMaxInactivitySeconds()
        {
            Assert.True(SdkInfo.MinInactivitySeconds < SdkInfo.MaxInactivitySeconds);
        }

        [Theory]
        [InlineData(0, "0")]
        [InlineData(1, "+1")]
        [InlineData(-1, "-1")]
        [InlineData(0.05, "+0.05")]
        [InlineData(-1.01, "-1.01")]
        [InlineData(1.25, "+1.25")]
        public void DecimalToStringWithSignReturnsCorrect(decimal inputValue, string expectedResult)
        {
            Assert.Equal(expectedResult, SdkInfo.DecimalToStringWithSign(inputValue));
        }

        [Fact]
        public void GetRandomReturnsInt()
        {
            var value = SdkInfo.GetRandom();
            Assert.True(value >= 0);
        }

        [Fact]
        public void GetRandomReturnsCorrectBetweenMinMax()
        {
            const int min = 100;
            const int max = 1000;
            for (var i = 0; i < 10000; i++)
            {
                var value = SdkInfo.GetRandom(min, max);
                Assert.True(value >= min);
                Assert.True(value < max);
            }
        }

        [Fact]
        public void GetVariableNumberForIntReturnsVariableBetweenMinMax()
        {
            const int baseValue = 100;
            const int variablePercent = 10;
            const double min = ((100 - (double)variablePercent) / 100) * baseValue;
            const double max = ((100 + (double)variablePercent) / 100) * baseValue;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.GetVariableNumber(baseValue, variablePercent);
                Assert.True(value >= min, $"{value} must be greater then {min}");
                Assert.True(value < max, $"{value} must be less then {max}");
            }
        }

        [Fact]
        public void GetVariableNumberForTimeSpanReturnsVariableBetweenMinMax()
        {
            var baseValue = TimeSpan.FromSeconds(100);
            const int variablePercent = 10;
            var min = ((100 - (double)variablePercent) / 100) * baseValue.TotalSeconds;
            var max = ((100 + (double)variablePercent) / 100) * baseValue.TotalSeconds;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.GetVariableNumber(baseValue, variablePercent);
                Assert.True(value.TotalSeconds >= min, $"{value.TotalSeconds} must be greater then {min}");
                Assert.True(value.TotalSeconds < max, $"{value.TotalSeconds} must be less then {max}");
            }
        }

        [Fact]
        public void AddVariableNumberForTimeSpanReturnsVariableBetweenMinMax()
        {
            var baseValue = TimeSpan.FromSeconds(100);
            const int variablePercent = 10;
            var max = ((100 + (double)variablePercent) / 100) * baseValue.TotalSeconds;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.AddVariableNumber(baseValue, variablePercent);
                Assert.True(value.TotalSeconds >= baseValue.TotalSeconds, $"{value.TotalSeconds} must be greater then {baseValue.TotalSeconds}");
                Assert.True(value.TotalSeconds < max, $"{value.TotalSeconds} must be less then {max}");
            }
        }

        [Fact]
        public void AddVariableNumberForProfileCacheTimeoutReturnsVariableBetweenMinMax()
        {
            var baseValue = OperationManager.ProfileCacheTimeout;
            const int variablePercent = 10;
            var max = ((100 + (double)variablePercent) / 100) * baseValue.TotalSeconds;
            for (var i = 0; i < 100; i++)
            {
                var value = SdkInfo.AddVariableNumber(baseValue, variablePercent);
                Assert.True(value.TotalSeconds >= baseValue.TotalSeconds, $"{value.TotalSeconds} must be greater then {baseValue.TotalSeconds}");
                Assert.True(value.TotalSeconds < max, $"{value.TotalSeconds} must be less then {max}");
                Assert.True(value.TotalSeconds < baseValue.TotalSeconds * 1.1, $"{value.TotalSeconds} must be less then {baseValue.TotalSeconds * 1.1}");
            }
        }

        [Fact]
        public void CultureInfoTest()
        {
            const int iteration = 1000000;
            var cultureList = new List<CultureInfo>();
            for (var i = 0; i < iteration; i++)
            {
                cultureList.Add(new CultureInfo("en"));
            }

            for (var i = 0; i < 1000; i++)
            {
                var r1 = SdkInfo.GetRandom(iteration);
                var r2 = SdkInfo.GetRandom(iteration);
                var culture1 = cultureList[r1];
                var culture2 = cultureList[r2];
                Assert.NotNull(culture1);
                Assert.NotNull(culture2);
                Assert.Equal(culture1, culture2);
                Assert.StrictEqual(culture1, culture2);
            }
        }

        [Fact]
        public void CultureInfoAsStringTest()
        {
            const int iteration = 1000000;
            var cultureList = new List<string>();
            for (var i = 0; i < iteration; i++)
            {
                cultureList.Add("en");
            }

            for (var i = 0; i < 1000; i++)
            {
                var r1 = SdkInfo.GetRandom(iteration);
                var r2 = SdkInfo.GetRandom(iteration);
                var culture1 = cultureList[r1];
                var culture2 = cultureList[r2];
                Assert.NotNull(culture1);
                Assert.NotNull(culture2);
                Assert.Equal(culture1, culture2);
                Assert.StrictEqual(culture1, culture2);
            }
        }

        [Fact]
        public void CultureInfoInDictionaryTest()
        {
            const int iteration = 10000;
            var dictList = new List<Dictionary<CultureInfo, string>>();
            var cultureList = new Dictionary<CultureInfo, string>();
            for (var i = 0; i < iteration; i++)
            {
                cultureList.Clear();
                cultureList.Add(new CultureInfo("en"), SdkInfo.GetGuid(20));
                dictList.Add(cultureList);
            }

            for (var i = 0; i < 1000; i++)
            {
                var r1 = SdkInfo.GetRandom(iteration);
                var r2 = SdkInfo.GetRandom(iteration);
                var culture1 = dictList[r1].Keys.First();
                var culture2 = dictList[r2].Keys.First();
                Assert.NotNull(culture1);
                Assert.NotNull(culture2);
                Assert.Equal(culture1, culture2);
                Assert.StrictEqual(culture1, culture2);
            }
        }
    }
}
