// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq.AutoMock;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

/// <summary>
/// AutoMocker base class
/// </summary>
public abstract class AutoMockerUnitTest
{
    /// <summary>
    /// The mocker
    /// </summary>
    protected readonly AutoMocker Mocker = new();
}
