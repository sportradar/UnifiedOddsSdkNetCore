// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

public class ProjectConfiguration
{
    private string LocalRabbitIp { get; }
    public int DefaultRabbitPort { get; }
    public string SdkRabbitUsername { get; }
    public string SdkRabbitPassword { get; }
    public string DefaultAdminRabbitUserName { get; }
    public string DefaultAdminRabbitPassword { get; }
    public string VirtualHostName { get; }
    public string UfExchange { get; }
    public IConfiguration Configuration { get; }

    public ProjectConfiguration()
    {
        Configuration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();

        SdkRabbitUsername = Configuration["Rabbit::SdkUsername"] ?? "testuser";
        SdkRabbitPassword = Configuration["Rabbit::SdkPassword"] ?? "testpass";
        DefaultAdminRabbitUserName = Configuration["Rabbit::AdminUsername"] ?? "guest";
        DefaultAdminRabbitPassword = Configuration["Rabbit::AdminPassword"] ?? "guest";
        LocalRabbitIp = Configuration["Rabbit::LocalHost"] ?? "localhost";
        DefaultRabbitPort = Configuration.GetValue("Rabbit::Port", 5672);
        VirtualHostName = Configuration["Rabbit::VirtualHostName"] ?? "/virtualhost";
        UfExchange = Configuration["Rabbit::ExchangeName"] ?? "unifiedfeed";
    }

    public string GetRabbitIp()
    {
        var envRabbitIp = Environment.GetEnvironmentVariable("RABBITMQ_IP");

        return envRabbitIp ?? LocalRabbitIp ?? GetLocalIpAddress();
        //return envRabbitIp ?? GetLocalIpAddress() ?? LocalRabbitIp;
    }

    private static string GetLocalIpAddress()
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                                                .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                                              nic.NetworkInterfaceType != NetworkInterfaceType.Loopback);

        foreach (var networkInterface in networkInterfaces)
        {
            var ipProperties = networkInterface.GetIPProperties();
            var ipAddressInfo = ipProperties.UnicastAddresses
                                            .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);

            if (ipAddressInfo != null)
            {
                return ipAddressInfo.Address.ToString();
            }
        }

        return null;
    }
}
