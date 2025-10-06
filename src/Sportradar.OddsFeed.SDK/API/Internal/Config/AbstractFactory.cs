// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal static class AbstractFactoryExtension
    {
        public static void AddAbstractFactory<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TInterface, TImplementation>();
            services.AddSingleton<Func<TInterface>>(x => () => x.GetService<TInterface>());
            services.AddSingleton<IAbstractFactory<TInterface>, AbstractFactory<TInterface>>();
        }

        public static void AddAbstractFactory<TInterface, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddScoped<TInterface, TImplementation>(implementationFactory);
            services.AddSingleton<Func<TInterface>>(x => () => x.GetService<TInterface>());
            services.AddSingleton<IAbstractFactory<TInterface>, AbstractFactory<TInterface>>();
        }

        public static void AddAbstractFactoryForSingleton<TInterface, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services.AddSingleton<TInterface, TImplementation>(implementationFactory);
            services.AddSingleton<Func<TInterface>>(x => () => x.GetService<TInterface>());
            services.AddSingleton<IAbstractFactory<TInterface>, AbstractFactory<TInterface>>();
        }
    }

    internal class AbstractFactory<T> : IAbstractFactory<T> where T : class
    {
        private readonly Func<T> _factory;

        public AbstractFactory(Func<T> factory)
        {
            _factory = factory;
        }

        public T Create()
        {
            return _factory();
        }
    }

    internal interface IAbstractFactory<T> where T : class
    {
        T Create();
    }
}
