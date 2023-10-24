using System;

// ReSharper disable once CheckNamespace
namespace Sportradar.OddsFeed.Tests.Common;

[Serializable]
internal class NonSdkClass
{
    public int Value { get; set; }

    public NonSdkClass(int value)
    {
        Value = value;
    }
}
