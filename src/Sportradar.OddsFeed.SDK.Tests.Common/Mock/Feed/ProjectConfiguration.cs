// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.Configuration;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class ProjectConfiguration
{
    public string RabbitHost { get; set; }
    public int RabbitPort { get; set; }
    public string SdkRabbitUsername { get; set; }
    public string SdkRabbitPassword { get; set; }
    public string DefaultAdminRabbitUserName { get; set; }
    public string DefaultAdminRabbitPassword { get; set; }
    public string VirtualHostName { get; set; }
    public string UfExchange { get; set; }
    public IConfiguration Configuration { get; set; }
}
