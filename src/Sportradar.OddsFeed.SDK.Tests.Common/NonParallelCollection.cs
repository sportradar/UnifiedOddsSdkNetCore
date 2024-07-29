// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

[CollectionDefinition(NonParallelCollectionFixture.NonParallelTestCollection, DisableParallelization = true)]
public class NonParallelCollection : ICollectionFixture<NonParallelCollectionFixture>
{
}

// This class serves as the fixture for the collection. It can contain setup/teardown logic if needed.
public class NonParallelCollectionFixture
{
    public const string NonParallelTestCollection = "NonParallelTestCollection";
}
