// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockFeed;

public class MqUser
{
    // ReSharper disable once InconsistentNaming
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "needed for rabbit")]
    public string password;

    // ReSharper disable once InconsistentNaming
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "needed for rabbit")]
    public string tags;
}
