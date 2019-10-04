/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IConfigurationAccessTokenSetter))]
    abstract class ConfigurationAccessTokenSetterContract : IConfigurationAccessTokenSetter
    {
        public IConfigurationInactivitySecondsSetter SetAccessToken(string accessToken)
        {
            Contract.Requires(!string.IsNullOrEmpty(accessToken));
            Contract.Ensures(Contract.Result<IConfigurationInactivitySecondsSetter>() != null);
            return Contract.Result<IConfigurationInactivitySecondsSetter>();
        }
    }

    [ContractClassFor(typeof(IConfigurationInactivitySecondsSetter))]
    abstract class ConfigurationInactivitySecondsSetterContract : IConfigurationInactivitySecondsSetter
    {
        public IOddsFeedConfigurationBuilder SetInactivitySeconds(int inactivitySeconds)
        {
            Contract.Requires(inactivitySeconds >= SdkInfo.MinInactivitySeconds && inactivitySeconds <= SdkInfo.MaxInactivitySeconds);
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }
    }

    [ContractClassFor(typeof(IOddsFeedConfigurationBuilder))]
    abstract class OddsFeedConfigurationBuilderContract : IOddsFeedConfigurationBuilder
    {
        public IOddsFeedConfigurationBuilder AddLocale(CultureInfo culture)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder RemoveLocale(CultureInfo culture)
        {
            Contract.Requires(culture != null);
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetApiHost(string apiHost)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetHost(string host)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetVirtualHost(string virtualHost)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetUseSsl(bool useSsl)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetMaxRecoveryTime(int maxRecoveryTime)
        {
            Contract.Requires(maxRecoveryTime >= SdkInfo.MinRecoveryExecutionInSeconds && maxRecoveryTime <= SdkInfo.MaxRecoveryExecutionInSeconds);
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetUseStagingEnvironment(bool useStagingEnvironment)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetUseIntegrationEnvironment(bool useIntegrationEnvironment)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfigurationBuilder SetNodeId(int nodeId)
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfigurationBuilder>() != null);
            return Contract.Result<IOddsFeedConfigurationBuilder>();
        }

        public IOddsFeedConfiguration Build()
        {
            Contract.Ensures(Contract.Result<IOddsFeedConfiguration>() != null);
            return Contract.Result<IOddsFeedConfiguration>();
        }
    }
}
