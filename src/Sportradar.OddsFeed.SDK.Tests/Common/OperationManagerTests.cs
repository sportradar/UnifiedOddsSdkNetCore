using System;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public class OperationManagerTests
    {
        private const int DefaultSportEventStatusCacheTimeout = 5;
        private const int DefaultProfileCacheTimeout = 24;
        private const int DefaultVariantMarketDescriptionCacheTimeout = 3;
        private const int DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout = 3;

        [Fact]
        public void InitializationTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Assert.False(OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(30));
            Assert.Equal(30, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(1));
            Assert.Equal(1, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(60));
            Assert.Equal(60, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Action action = () => OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SportEventStatusCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Action action = () => OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(61));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ProfileCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(30));
            Assert.Equal(30, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [Fact]
        public void ProfileCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(1));
            Assert.Equal(1, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [Fact]
        public void ProfileCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(48));
            Assert.Equal(48, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [Fact]
        public void ProfileCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ProfileCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(49));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(10));
            Assert.Equal(10, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(1));
            Assert.Equal(1, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(24));
            Assert.Equal(24, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(25));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(10));
            Assert.Equal(10, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(1));
            Assert.Equal(1, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(24));
            Assert.Equal(24, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(25));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusTest()
        {
            ResetOperationManager();
            Assert.False(OperationManager.IgnoreBetPalTimelineSportEventStatus);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatus(true);
            Assert.True(OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        private void ResetOperationManager()
        {
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(DefaultSportEventStatusCacheTimeout));
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(DefaultProfileCacheTimeout));
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(DefaultVariantMarketDescriptionCacheTimeout));
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout));
            OperationManager.SetIgnoreBetPalTimelineSportEventStatus(false);
        }
    }
}
