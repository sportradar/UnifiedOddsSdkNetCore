// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

// Ignore Spelling: Ssl
// Ignore Spelling: Uf
public class TestSection : IUofConfigurationSection
{
    public string AccessToken { get; set; }
    public string RabbitHost { get; set; }
    public string RabbitVirtualHost { get; set; }
    public int RabbitPort { get; set; }
    public string RabbitUsername { get; set; }
    public string RabbitPassword { get; set; }
    public bool RabbitUseSsl { get; set; }
    public string ApiHost { get; set; }
    public bool ApiUseSsl { get; set; }
    public string DefaultLanguage { get; set; }
    public string Languages { get; set; }
    public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; set; }
    public string DisabledProducers { get; set; }
    public int NodeId { get; set; }
    public SdkEnvironment Environment { get; set; }

    public TestSection(string accessToken,
                       string host,
                       string virtualHost,
                       int port,
                       string username,
                       string password,
                       string apiHost,
                       bool useSsl,
                       bool useApiSsl,
                       string supportedLanguages,
                       string defaultLanguage,
                       ExceptionHandlingStrategy exceptionHandlingStrategy,
                       string disabledProducers,
                       int nodeId,
                       SdkEnvironment environment)
    {
        AccessToken = accessToken;
        RabbitHost = host;
        RabbitVirtualHost = virtualHost;
        RabbitPort = port;
        RabbitUsername = username;
        RabbitPassword = password;
        ApiHost = apiHost;
        RabbitUseSsl = useSsl;
        ApiUseSsl = useApiSsl;
        Languages = supportedLanguages;
        DefaultLanguage = defaultLanguage;
        ExceptionHandlingStrategy = exceptionHandlingStrategy;
        DisabledProducers = disabledProducers;
        NodeId = nodeId;
        Environment = environment;
    }

    internal static IUofConfigurationSection MinimalIntegrationSection = new TestSection(TestData.AccessToken,
                                                                                         null,
                                                                                         null,
                                                                                         0,
                                                                                         null,
                                                                                         null,
                                                                                         null,
                                                                                         true,
                                                                                         true,
                                                                                         "en",
                                                                                         null,
                                                                                         ExceptionHandlingStrategy.Catch,
                                                                                         null,
                                                                                         0,
                                                                                         SdkEnvironment.Integration);

    internal static IUofConfigurationSection MinimalProductionSection = new TestSection(TestData.AccessToken,
                                                                                        null,
                                                                                        null,
                                                                                        0,
                                                                                        null,
                                                                                        null,
                                                                                        null,
                                                                                        true,
                                                                                        true,
                                                                                        "en",
                                                                                        null,
                                                                                        ExceptionHandlingStrategy.Catch,
                                                                                        null,
                                                                                        0,
                                                                                        SdkEnvironment.Production);

    internal static IUofConfigurationSection GetCustomSection()
    {
        return new TestSection(TestData.AccessToken,
                               "stgmq.localhost.com",
                               "virtual_host",
                               5000,
                               "username",
                               "password",
                               "stgapi.localhost.com",
                               false,
                               false,
                               "en",
                               "en",
                               ExceptionHandlingStrategy.Throw,
                               "1,3",
                               11,
                               SdkEnvironment.Custom);
    }

    internal static IUofConfigurationSection CustomProductionSection = new TestSection(TestData.AccessToken,
                                                                                       "mq.localhost.com",
                                                                                       "virtual_host",
                                                                                       5000,
                                                                                       "username",
                                                                                       "password",
                                                                                       "api.localhost.com",
                                                                                       false,
                                                                                       false,
                                                                                       "en,de,it",
                                                                                       "en",
                                                                                       ExceptionHandlingStrategy.Throw,
                                                                                       "1,3",
                                                                                       11,
                                                                                       SdkEnvironment.Custom);

    internal static IUofConfigurationSection GetDefaultSection()
    {
        return new TestSection(TestData.AccessToken,
                               null,
                               null,
                               0,
                               null,
                               string.Empty,
                               null,
                               true,
                               true,
                               "en",
                               null,
                               ExceptionHandlingStrategy.Catch,
                               null,
                               0,
                               SdkEnvironment.Integration);
    }
}
