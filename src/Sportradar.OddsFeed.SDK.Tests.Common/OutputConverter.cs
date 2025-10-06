// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class OutputConverter : TextWriter
{
    private readonly ITestOutputHelper _outputHelper;

    public OutputConverter(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public override Encoding Encoding
    {
        get => Encoding.Unicode;
    }

    public override void WriteLine(string value)
    {
        _outputHelper.WriteLine(value);
    }

    public override void WriteLine(string format, params object[] arg)
    {
        _outputHelper.WriteLine(format, arg);
    }

    public override void Write(char value)
    {
        _outputHelper.WriteLine(value.ToString());
    }
}
