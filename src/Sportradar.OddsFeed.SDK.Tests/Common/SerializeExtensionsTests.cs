// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Runtime.Serialization;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class SerializeExtensionsTests
{
    [Fact]
    public void SerializeToByteArray()
    {
        var fake = new FakeClass(12345);
        var serialized = fake.SerializeToByteArray();

        Assert.NotNull(serialized);
        Assert.NotEmpty(serialized);
    }

    [Fact]
    public void SerializeToByteArray_ForNull()
    {
        var fake = (FakeClass)null;
        var serialized = fake.SerializeToByteArray();

        Assert.Null(serialized);
    }

    [Fact]
    public void DeserializeFromByteArray()
    {
        var fake = new FakeClass(12345);
        var serialized = fake.SerializeToByteArray();

        var retrievedFake = serialized.Deserialize<FakeClass>();

        Assert.NotNull(retrievedFake);
        Assert.Equal(fake.Value, retrievedFake.Value);
    }

    [Fact]
    public void DeserializeFromByteArray_FromNull()
    {
        var serialized = (byte[])null;

        var retrievedFake = serialized.Deserialize<FakeClass>();

        Assert.Null(retrievedFake);
    }

    [Fact]
    public void Deserialize_NonSdkClass_Throws()
    {
        var nonSdkClass = new NonSdkClass(12345);
        var serialized = nonSdkClass.SerializeToByteArray();

        Assert.Throws<SerializationException>(() => serialized.Deserialize<NonSdkClass>());
    }

    [Serializable]
    private sealed class FakeClass
    {
        public int Value { get; set; }

        public FakeClass(int value)
        {
            Value = value;
        }
    }
}
