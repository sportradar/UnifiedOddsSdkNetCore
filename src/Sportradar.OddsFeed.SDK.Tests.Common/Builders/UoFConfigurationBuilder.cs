// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

public class UofConfigurationBuilder
{
    private readonly AdditionalConfigBuilder _additionalBuilder = new();
    private readonly ApiConfigBuilder _apiBuilder = new();
    private readonly CacheConfigBuilder _cacheBuilder = new();
    private readonly Mock<IUofConfiguration> _configMock = new();
    private readonly ProducerConfigBuilder _producerBuilder = new();
    private readonly RabbitConfigBuilder _rabbitBuilder = new();
    private readonly UsageConfigBuilder _usageBuilder = new();
    private readonly AuthConfigBuilder _authBuilder = new();

    public UofConfigurationBuilder WithAccessToken(string token)
    {
        _configMock.SetupGet(x => x.AccessToken).Returns(token);
        return this;
    }

    public UofConfigurationBuilder WithEnvironment(SdkEnvironment env)
    {
        _configMock.SetupGet(x => x.Environment).Returns(env);
        return this;
    }

    public UofConfigurationBuilder WithNodeId(int id)
    {
        _configMock.SetupGet(x => x.NodeId).Returns(id);
        return this;
    }

    public UofConfigurationBuilder WithDefaultLanguage(CultureInfo culture)
    {
        _configMock.SetupGet(x => x.DefaultLanguage).Returns(culture);
        return this;
    }

    public UofConfigurationBuilder WithLanguages(List<CultureInfo> cultures)
    {
        _configMock.SetupGet(x => x.Languages).Returns(cultures);
        return this;
    }

    public UofConfigurationBuilder WithExceptionStrategy(ExceptionHandlingStrategy strategy)
    {
        _configMock.SetupGet(x => x.ExceptionHandlingStrategy).Returns(strategy);
        return this;
    }

    public UofConfigurationBuilder WithBookmakerDetails(IBookmakerDetails details)
    {
        _configMock.SetupGet(x => x.BookmakerDetails).Returns(details);
        return this;
    }

    public UofConfigurationBuilder WithApiConfiguration(Action<ApiConfigBuilder> config)
    {
        config(_apiBuilder);
        return this;
    }

    public UofConfigurationBuilder WithCacheConfiguration(Action<CacheConfigBuilder> config)
    {
        config(_cacheBuilder);
        return this;
    }

    public UofConfigurationBuilder WithRabbitConfiguration(Action<RabbitConfigBuilder> config)
    {
        config(_rabbitBuilder);
        return this;
    }

    public UofConfigurationBuilder WithProducerConfiguration(Action<ProducerConfigBuilder> config)
    {
        config(_producerBuilder);
        return this;
    }

    public UofConfigurationBuilder WithUsageConfiguration(Action<UsageConfigBuilder> config)
    {
        config(_usageBuilder);
        return this;
    }

    public UofConfigurationBuilder WithAdditionalConfiguration(Action<AdditionalConfigBuilder> config)
    {
        config(_additionalBuilder);
        return this;
    }

    public UofConfigurationBuilder WithAuthenticationConfiguration(Action<AuthConfigBuilder> config)
    {
        config(_authBuilder);
        return this;
    }

    public IUofConfiguration Build()
    {
        _configMock.SetupGet(x => x.Api).Returns(_apiBuilder.Build());
        _configMock.SetupGet(x => x.Cache).Returns(_cacheBuilder.Build());
        _configMock.SetupGet(x => x.Rabbit).Returns(_rabbitBuilder.Build());
        _configMock.SetupGet(x => x.Producer).Returns(_producerBuilder.Build());
        _configMock.SetupGet(x => x.Usage).Returns(_usageBuilder.Build());
        _configMock.SetupGet(x => x.Additional).Returns(_additionalBuilder.Build());
        _configMock.SetupGet(x => x.Authentication).Returns(_authBuilder.Build());

        return _configMock.Object;
    }

    public class ApiConfigBuilder
    {
        private readonly Mock<IUofApiConfiguration> _apiMock = new();

        public ApiConfigBuilder WithBaseUrl(string url)
        {
            return Set(x => x.BaseUrl, url);
        }

        public ApiConfigBuilder WithHost(string host)
        {
            return Set(x => x.Host, host);
        }

        public ApiConfigBuilder WithUseSsl(bool useSsl)
        {
            return Set(x => x.UseSsl, useSsl);
        }

        public ApiConfigBuilder WithHttpClientTimeout(TimeSpan timeout)
        {
            return Set(x => x.HttpClientTimeout, timeout);
        }

        public ApiConfigBuilder WithHttpClientRecoveryTimeout(TimeSpan timeout)
        {
            return Set(x => x.HttpClientRecoveryTimeout, timeout);
        }

        public ApiConfigBuilder WithHttpClientFastFailingTimeout(TimeSpan timeout)
        {
            return Set(x => x.HttpClientFastFailingTimeout, timeout);
        }

        public ApiConfigBuilder WithReplayHost(string host)
        {
            return Set(x => x.ReplayHost, host);
        }

        public ApiConfigBuilder WithReplayBaseUrl(string url)
        {
            return Set(x => x.ReplayBaseUrl, url);
        }

        public ApiConfigBuilder WithMaxConnectionsPerServer(int max)
        {
            return Set(x => x.MaxConnectionsPerServer, max);
        }

        public IUofApiConfiguration Build()
        {
            return _apiMock.Object;
        }

        private ApiConfigBuilder Set<T>(Expression<Func<IUofApiConfiguration, T>> prop, T value)
        {
            _apiMock.SetupGet(prop).Returns(value);
            return this;
        }
    }

    public class CacheConfigBuilder
    {
        public IUofCacheConfiguration Build()
        {
            return new Mock<IUofCacheConfiguration>().Object;
        }
    }

    public class RabbitConfigBuilder
    {
        public IUofRabbitConfiguration Build()
        {
            return new Mock<IUofRabbitConfiguration>().Object;
        }
    }

    public class ProducerConfigBuilder
    {
        public IUofProducerConfiguration Build()
        {
            return new Mock<IUofProducerConfiguration>().Object;
        }
    }

    public class AdditionalConfigBuilder
    {
        public IUofAdditionalConfiguration Build()
        {
            return new Mock<IUofAdditionalConfiguration>().Object;
        }
    }

    public class UsageConfigBuilder
    {
        private readonly Mock<IUofUsageConfiguration> _usageMock = new();

        public UsageConfigBuilder WithExportEnabled(bool value)
        {
            return Set(x => x.IsExportEnabled, value);
        }

        public UsageConfigBuilder WithExportIntervalInSec(int seconds)
        {
            return Set(x => x.ExportIntervalInSec, seconds);
        }

        public UsageConfigBuilder WithExportTimeoutInSec(int seconds)
        {
            return Set(x => x.ExportTimeoutInSec, seconds);
        }

        public UsageConfigBuilder WithHost(string host)
        {
            return Set(x => x.Host, host);
        }

        public IUofUsageConfiguration Build()
        {
            return _usageMock.Object;
        }

        private UsageConfigBuilder Set<T>(Expression<Func<IUofUsageConfiguration, T>> prop, T value)
        {
            _usageMock.SetupGet(prop).Returns(value);
            return this;
        }
    }

    public class AuthConfigBuilder
    {
        private readonly Mock<UofClientAuthentication.IPrivateKeyJwt> _authMock = new();

        public AuthConfigBuilder WithHost(string host)
        {
            return Set(x => x.Host, host);
        }

        public AuthConfigBuilder WithPort(int port)
        {
            return Set(x => x.Port, port);
        }

        public AuthConfigBuilder WithSsl(bool value)
        {
            return Set(x => x.UseSsl, value);
        }

        public AuthConfigBuilder WithClientId(string clientId)
        {
            return Set(x => x.ClientId, clientId);
        }

        public AuthConfigBuilder WithSigningKeyId(string signingKeyId)
        {
            return Set(x => x.SigningKeyId, signingKeyId);
        }

        public AuthConfigBuilder WithPrivateKey(AsymmetricSecurityKey privateKey)
        {
            return Set(x => x.PrivateKey, privateKey);
        }

        public AuthConfigBuilder WithAnyPrivateKey()
        {
            AsymmetricSecurityKey testPrivateKey = new RsaSecurityKey(RSA.Create(2056));
            return Set(x => x.PrivateKey, testPrivateKey);
        }

        public UofClientAuthentication.IPrivateKeyJwt Build()
        {
            return _authMock.Object;
        }

        private AuthConfigBuilder Set<T>(Expression<Func<UofClientAuthentication.IPrivateKeyJwt, T>> prop, T value)
        {
            _authMock.SetupGet(prop).Returns(value);
            return this;
        }
    }
}
