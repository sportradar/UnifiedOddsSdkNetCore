/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IOddsFeedConfiguration))]
    internal abstract class OddsFeedConfigurationContract : IOddsFeedConfiguration
    {
        [Pure]
        public string AccessToken
        {
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                return Contract.Result<string>();
            }
        }

        [Pure]
        public int InactivitySeconds
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 20 && Contract.Result<int>() <= 180);
                return Contract.Result<int>();
            }
        }

        [Pure]
        public IEnumerable<CultureInfo> Locales
        {
            [Pure]
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<CultureInfo>>() != null);
                return Contract.Result<IEnumerable<CultureInfo>>();
            }
        }

        [Pure]
        public CultureInfo DefaultLocale
        {
            get
            {
                Contract.Ensures(Contract.Result<CultureInfo>() != null);
                return Contract.Result<CultureInfo>();
            }
        }

        [Pure] public SdkEnvironment Environment => Contract.Result<SdkEnvironment>();

        [Pure]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string VirtualHost { get; set; }

        [Pure]
        public string Host
        {
            [Pure]
            get
            {
                Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));
                return Contract.Result<string>();
            }
        }

        [Pure]
        public bool UseSsl => Contract.Result<bool>();

        [Pure]
        public string Username => Contract.Result<string>();

        [Pure]
        public string Password => Contract.Result<string>();

        [Pure]
        public int Port {
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);
                return Contract.Result<int>();
            }
        }

        [Pure] public string ApiHost => Contract.Result<string>();

        [Pure]
        public bool UseApiSsl => Contract.Result<bool>();

        [Pure]
        public bool AdjustAfterAge => Contract.Result<bool>();

        [Pure]
        public IEnumerable<int> DisabledProducers => Contract.Result<IEnumerable<int>>();

        [Pure]
        public int MaxRecoveryTime
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= SdkInfo.MinRecoveryExecutionInSeconds);
                return Contract.Result<int>();
            }
        }

        [Pure]
        public int NodeId => Contract.Result<int>();

        [Pure]
        public ExceptionHandlingStrategy ExceptionHandlingStrategy => Contract.Result<ExceptionHandlingStrategy>();
    }
}
