// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Config;

public class UofUsageConfigurationStub : IUofUsageConfiguration
{
    public bool IsExportEnabled
    {
        get;
    }
    public int ExportIntervalInSec
    {
        get;
    }
    public int ExportTimeoutInSec
    {
        get;
    }
    public string Host
    {
        get;
    }

    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Pipeline format fails with primary constructor")]
    public UofUsageConfigurationStub(bool isExportEnabled, string customUsageHost, int exportIntervalInSec = 60, int exportTimeoutInSec = 30)
    {
        IsExportEnabled = isExportEnabled;
        ExportIntervalInSec = exportIntervalInSec;
        ExportTimeoutInSec = exportTimeoutInSec;
        Host = customUsageHost;
    }
}
