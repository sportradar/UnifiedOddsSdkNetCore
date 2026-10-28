// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Xunit;
using FakeTimeProvider = Microsoft.Extensions.Time.Testing.FakeTimeProvider;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class RequestCircuitBreakerTests
{
    // SonarQube new_coverage: the Open() branch that keeps the longer existing open-until when a shorter
    // duration is requested is otherwise unreachable via DI sociable tests without incorrectly resolving
    // IRequestCircuitBreaker from the container. This isolated test covers that branch so the quality gate passes.
    [Fact]
    public void OpenWhenAlreadyOpenForLongerDurationThenShorterOpenDoesNotReduceOpenDuration()
    {
        var fakeTimeProvider = new FakeTimeProvider();
        var circuitBreaker = new RequestCircuitBreaker(fakeTimeProvider);

        circuitBreaker.Open(TimeSpan.FromMinutes(5));
        circuitBreaker.Open(TimeSpan.FromSeconds(5));

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(4));

        circuitBreaker.IsOpen().ShouldBeTrue("Shorter Open must not reduce the existing 5-minute open window");
    }
}
