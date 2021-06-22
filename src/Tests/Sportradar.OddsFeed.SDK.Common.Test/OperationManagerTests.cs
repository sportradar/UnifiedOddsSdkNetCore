using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sportradar.OddsFeed.SDK.Common.Test
{
    [TestClass]
    public class OperationManagerTests
    {
        private const int DefaultSportEventStatusCacheTimeout = 5;
        private const int DefaultProfileCacheTimeout = 24;
        private const int DefaultVariantMarketDescriptionCacheTimeout = 3;
        private const int DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout = 3;

        [TestMethod]
        public void InitializationTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            Assert.AreEqual(false, OperationManager.IgnoreBetPalTimelineSportEventStatus);
        }

        [TestMethod]
        public void SportEventStatusCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(30));
            Assert.AreEqual(30, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [TestMethod]
        public void SportEventStatusCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(1));
            Assert.AreEqual(1, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [TestMethod]
        public void SportEventStatusCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(60));
            Assert.AreEqual(60, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SportEventStatusCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void SportEventStatusCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultSportEventStatusCacheTimeout, OperationManager.SportEventStatusCacheTimeout.TotalMinutes);
            OperationManager.SetSportEventStatusCacheTimeout(TimeSpan.FromMinutes(61));
        }

        [TestMethod]
        public void ProfileCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(30));
            Assert.AreEqual(30, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void ProfileCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(1));
            Assert.AreEqual(1, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void ProfileCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(48));
            Assert.AreEqual(48, OperationManager.ProfileCacheTimeout.TotalHours);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ProfileCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void ProfileCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultProfileCacheTimeout, OperationManager.ProfileCacheTimeout.TotalHours);
            OperationManager.SetProfileCacheTimeout(TimeSpan.FromHours(49));
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(10));
            Assert.AreEqual(10, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(1));
            Assert.AreEqual(1, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void VariantMarketDescriptionCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(24));
            Assert.AreEqual(24, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void VariantMarketDescriptionCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void VariantMarketDescriptionCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultVariantMarketDescriptionCacheTimeout, OperationManager.VariantMarketDescriptionCacheTimeout.TotalHours);
            OperationManager.SetVariantMarketDescriptionCacheTimeout(TimeSpan.FromHours(25));
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(10));
            Assert.AreEqual(10, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(1));
            Assert.AreEqual(1, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(24));
            Assert.AreEqual(24, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutBelowMinTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(0));
        }

        [TestMethod, ExpectedException(typeof(InvalidOperationException))]
        public void IgnoreBetPalTimelineSportEventStatusCacheTimeoutAboveMaxTest()
        {
            ResetOperationManager();
            Assert.AreEqual(DefaultIgnoreBetPalTimelineSportEventStatusCacheTimeout, OperationManager.IgnoreBetPalTimelineSportEventStatusCacheTimeout.TotalHours);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatusCacheTimeout(TimeSpan.FromHours(25));
        }

        [TestMethod]
        public void IgnoreBetPalTimelineSportEventStatusTest()
        {
            ResetOperationManager();
            Assert.IsFalse(OperationManager.IgnoreBetPalTimelineSportEventStatus);
            OperationManager.SetIgnoreBetPalTimelineSportEventStatus(true);
            Assert.IsTrue(OperationManager.IgnoreBetPalTimelineSportEventStatus);
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
