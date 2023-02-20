/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class SportSummaryTests
    {
        [Fact]
        public void WhenCreatingNewIdCantBeNull()
        {
            Action action = () => _ = new SportSummary(null, TestData.Cultures.ToDictionary(c => c, c => string.Empty));
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenCreatingNewNamesCantBeNull()
        {
            Action action = () => _ = new SportSummary(TestData.SportId, null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WhenCreatingNewNamesCantBeEmpty()
        {
            Action action = () => _ = new SportSummary(TestData.SportId, new Dictionary<CultureInfo, string>());
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ReturnValidName()
        {
            var item = new SportSummary(TestData.SportId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName));
            var name = item.GetName(TestData.Culture);
            Assert.Equal(3, item.Names.Count);
            Assert.True(!string.IsNullOrEmpty(name));
        }

        [Fact]
        public void ReturnInvalidName()
        {
            var item = new SportSummary(TestData.SportId, TestData.Cultures.ToDictionary(c => c, c => c.EnglishName));
            var name = item.GetName(new CultureInfo("gl"));
            Assert.Equal(3, item.Names.Count);
            Assert.Null(name);
        }
    }
}
