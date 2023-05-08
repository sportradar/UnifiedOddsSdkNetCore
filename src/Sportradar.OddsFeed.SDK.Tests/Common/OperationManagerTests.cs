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
        public void Initialization()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Assert.False(OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        [Fact]
        public void SportEventStatusCacheTimeout()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(30));
            Assert.Equal(30, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(1));
            Assert.Equal(1, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(60));
            Assert.Equal(60, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [Fact]
        public void SportEventStatusCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Action action = () => OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void SportEventStatusCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Action action = () => OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(61));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ProfileCacheTimeout()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(30));
            Assert.Equal(30, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [Fact]
        public void ProfileCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(1));
            Assert.Equal(1, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [Fact]
        public void ProfileCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(48));
            Assert.Equal(48, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [Fact]
        public void ProfileCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void ProfileCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(49));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeout()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(10));
            Assert.Equal(10, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(1));
            Assert.Equal(1, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(24));
            Assert.Equal(24, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void VariantMarketDescriptionCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(25));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeout()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(10));
            Assert.Equal(10, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(1));
            Assert.Equal(1, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(24));
            Assert.Equal(24, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutBelowMin()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(0));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutAboveMax()
        {
            ResetOperationManager();
            Assert.Equal(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Action action = () => OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(25));
            action.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void IgnoreBetPalTimelineSportEventStatus()
        {
            ResetOperationManager();
            Assert.False(OperationManager.IgnoreBetPalTimelineSportEventStatus);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatus(true);
            Assert.True(OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        [Fact]
        public void SetMaxConnectionsPerServerSavedValidNumber()
        {
            const int newMaxConnections = 1000;
            ResetOperationManager();
            Assert.Equal(int.MaxValue, OperationManager.MaxConnectionsPerServer);
            OperationManager.SetMaxConnectionsPerServer(newMaxConnections);
            Assert.Equal(newMaxConnections, OperationManager.MaxConnectionsPerServer);
        }

        [Fact]
        public void SetMaxConnectionsPerServerInvalidNumberThrows()
        {
            const int newMaxConnections = 0;
            Assert.Equal(int.MaxValue, OperationManager.MaxConnectionsPerServer);
            Assert.Throws<InvalidOperationException>(() => OperationManager.SetMaxConnectionsPerServer(newMaxConnections));
            Assert.Equal(int.MaxValue, OperationManager.MaxConnectionsPerServer);
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
