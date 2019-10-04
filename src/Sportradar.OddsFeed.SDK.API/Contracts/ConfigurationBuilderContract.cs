/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(ITokenSetter))]
    abstract class TokenSetterContract : ITokenSetter
    {
        public IEnvironmentSelectorV1 SetAccessTokenFromConfigFile()
        {
            Contract.Ensures(Contract.Result<IEnvironmentSelectorV1>() != null);
            return Contract.Result<IEnvironmentSelectorV1>();
        }

        public IEnvironmentSelectorV1 SetAccessToken(string accessToken)
        {
            Contract.Requires(!string.IsNullOrEmpty(accessToken));
            Contract.Ensures(Contract.Result<IEnvironmentSelectorV1>() != null);
            return Contract.Result<IEnvironmentSelectorV1>();
        }
    }

    [ContractClassFor(typeof(IEnvironmentSelector))]
    abstract class EnvironmentSelectorContract : IEnvironmentSelector
    {
        public IConfigurationBuilder SelectStaging()
        {
            Contract.Ensures(Contract.Result<IConfigurationBuilder>() != null);
            return Contract.Result<IConfigurationBuilder>();
        }

        public IConfigurationBuilder SelectProduction()
        {
            Contract.Ensures(Contract.Result<IConfigurationBuilder>() != null);
            return Contract.Result<IConfigurationBuilder>();
        }

        public IReplayConfigurationBuilder SelectReplay()
        {
            Contract.Ensures(Contract.Result<IReplayConfigurationBuilder>() != null);
            return Contract.Result<IReplayConfigurationBuilder>();
        }

        public ICustomConfigurationBuilder SelectCustom()
        {
            Contract.Ensures(Contract.Result<ICustomConfigurationBuilder>() != null);
            return Contract.Result<ICustomConfigurationBuilder>();
        }
    }

    [ContractClassFor(typeof(IEnvironmentSelectorV1))]
    abstract class EnvironmentSelectorV1Contract : IEnvironmentSelectorV1
    {
        public IConfigurationBuilder SelectStaging()
        {
            Contract.Ensures(Contract.Result<IConfigurationBuilder>() != null);
            return Contract.Result<IConfigurationBuilder>();
        }

        public IConfigurationBuilder SelectProduction()
        {
            Contract.Ensures(Contract.Result<IConfigurationBuilder>() != null);
            return Contract.Result<IConfigurationBuilder>();
        }

        public IReplayConfigurationBuilder SelectReplay()
        {
            Contract.Ensures(Contract.Result<IReplayConfigurationBuilder>() != null);
            return Contract.Result<IReplayConfigurationBuilder>();
        }

        public ICustomConfigurationBuilder SelectCustom()
        {
            Contract.Ensures(Contract.Result<ICustomConfigurationBuilder>() != null);
            return Contract.Result<ICustomConfigurationBuilder>();
        }

        public IConfigurationBuilder SelectIntegration()
        {
            Contract.Ensures(Contract.Result<IConfigurationBuilder>() != null);
            return Contract.Result<IConfigurationBuilder>();
        }
    }
}
