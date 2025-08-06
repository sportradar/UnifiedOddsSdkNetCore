// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;
using BookmakerDetailsProvider = Sportradar.OddsFeed.SDK.Entities.Rest.Internal.BookmakerDetailsProvider;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class UofSdkBootstrapBase
{
    protected readonly IUofConfiguration UofConfig;
    protected readonly IServiceCollection ServiceCollection;
    protected readonly IServiceScope ServiceScope1;
    protected readonly IServiceScope ServiceScope2;

    public UofSdkBootstrapBase()
    {
        UofConfig = TestConfiguration.GetConfig();
        // we need to override initial loading of bookmaker details and producers
        var bookmakerDetailsProviderMock = new Mock<BookmakerDetailsProvider>("bookmakerDetailsUriFormat",
            new TestDataFetcher(),
            new Deserializer<bookmaker_details>(),
            new BookmakerDetailsMapperFactory());
        bookmakerDetailsProviderMock.Setup(x => x.GetData(It.IsAny<string>())).Returns(TestBookmakerDetailsProvider.GetBookmakerDetails());

        ServiceCollection = new ServiceCollection();
        ServiceCollection.AddLogging(configure =>
        {
            configure.AddDebug();
            configure.AddConsole();
        });
        ServiceCollection.AddUofSdkServices(UofConfig);

        var serviceProvider = ServiceCollection.BuildServiceProvider(true);
        ServiceScope1 = serviceProvider.CreateScope();
        ServiceScope2 = serviceProvider.CreateScope();
    }

    protected void CheckTransientType<T>(bool registeredOnce = true) where T : class
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<T>();
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<T>();
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<T>();
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.NotEqual(service1, service1A);
        Assert.NotEqual(service1, service2);
        Assert.NotStrictEqual(service1, service1A);
        Assert.NotStrictEqual(service1, service2);

        if (registeredOnce)
        {
            var servicesOfT = ServiceScope1.ServiceProvider.GetServices(typeof(T));
            Assert.Single(servicesOfT);
        }
    }

    protected void CheckScopedType<T>(bool registeredOnce = true) where T : class
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<T>();
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<T>();
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<T>();
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.Equal(service1, service1A);
        Assert.NotEqual(service1, service2);
        Assert.StrictEqual(service1, service1A);
        Assert.NotStrictEqual(service1, service2);

        if (registeredOnce)
        {
            var servicesOfT = ServiceScope1.ServiceProvider.GetServices(typeof(T));
            Assert.Single(servicesOfT);
        }
    }

    protected void CheckSingletonType<T>(bool registeredOnce = true) where T : class
    {
        var service1 = ServiceScope1.ServiceProvider.GetRequiredService<T>();
        var service1A = ServiceScope1.ServiceProvider.GetRequiredService<T>();
        var service2 = ServiceScope2.ServiceProvider.GetRequiredService<T>();
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.Equal(service1, service1A);
        Assert.Equal(service1, service2);
        Assert.StrictEqual(service1, service1A);
        Assert.StrictEqual(service1, service2);

        if (registeredOnce)
        {
            var servicesOfT = ServiceScope1.ServiceProvider.GetServices(typeof(T));
            Assert.Single(servicesOfT);
        }
    }

    protected void CheckSingletonDataProviderNamed<T>(string name) where T : class
    {
        var service1 = ServiceScope1.ServiceProvider.GetDataProviderNamed<T>(name);
        var service1A = ServiceScope1.ServiceProvider.GetDataProviderNamed<T>(name);
        var service2 = ServiceScope2.ServiceProvider.GetDataProviderNamed<T>(name);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.Equal(service1, service1A);
        Assert.Equal(service1, service2);
        Assert.StrictEqual(service1, service1A);
        Assert.StrictEqual(service1, service2);
    }

    protected void CheckSingletonNamedValueCache(string name)
    {
        var service1 = ServiceScope1.ServiceProvider.GetNamedValueCache(name);
        var service1A = ServiceScope1.ServiceProvider.GetNamedValueCache(name);
        var service2 = ServiceScope2.ServiceProvider.GetNamedValueCache(name);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.Equal(service1, service1A);
        Assert.Equal(service1, service2);
        Assert.StrictEqual(service1, service1A);
        Assert.StrictEqual(service1, service2);
    }

    protected void CheckSingletonLocalizedNamedValueCache(string name)
    {
        var service1 = ServiceScope1.ServiceProvider.GetLocalizedNamedValueCache(name);
        var service1A = ServiceScope1.ServiceProvider.GetLocalizedNamedValueCache(name);
        var service2 = ServiceScope2.ServiceProvider.GetLocalizedNamedValueCache(name);
        Assert.NotNull(service1);
        Assert.NotNull(service1A);
        Assert.NotNull(service2);
        Assert.Equal(service1, service1A);
        Assert.Equal(service1, service2);
        Assert.StrictEqual(service1, service1A);
        Assert.StrictEqual(service1, service2);
    }
}
