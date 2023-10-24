/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Represents SDK configuration
    /// </summary>
    [SuppressMessage("ReSharper", "TooManyChainedReferences")]
    internal class UofConfiguration : IUofConfiguration
    {
        private readonly IUofConfigurationSectionProvider _uofConfigurationSectionProvider;

        public string AccessToken { get; internal set; }
        public CultureInfo DefaultLanguage { get; internal set; }
        public List<CultureInfo> Languages { get; internal set; }
        public IBookmakerDetails BookmakerDetails { get; private set; }
        public IUofRabbitConfiguration Rabbit { get; internal set; }
        public IUofApiConfiguration Api { get; internal set; }
        public IUofProducerConfiguration Producer { get; internal set; }
        public IUofCacheConfiguration Cache { get; internal set; }
        public IUofAdditionalConfiguration Additional { get; internal set; }
        public int NodeId { get; internal set; }
        public SdkEnvironment Environment { get; internal set; }
        public ExceptionHandlingStrategy ExceptionHandlingStrategy { get; internal set; }

        public UofConfiguration(IUofConfigurationSectionProvider uofConfigurationSectionProvider)
        {
            _uofConfigurationSectionProvider = uofConfigurationSectionProvider ?? throw new ArgumentNullException(nameof(uofConfigurationSectionProvider));

            BookmakerDetails = null;
            Languages = new List<CultureInfo>();
            Api = new UofApiConfiguration();
            Cache = new UofCacheConfiguration();
            Producer = new UofProducerConfiguration();
            Rabbit = new UofRabbitConfiguration();
            Additional = new UofAdditionalConfiguration();
            ExceptionHandlingStrategy = ExceptionHandlingStrategy.Catch;
        }

        public void UpdateSdkEnvironment(SdkEnvironment environment)
        {
            if (Environment == environment && !Rabbit.Host.IsNullOrEmpty() && !Api.Host.IsNullOrEmpty())
            {
                return;
            }

            Environment = environment;

            if (Environment != SdkEnvironment.Custom || Rabbit.Host.IsNullOrEmpty())
            {
                var rabbitConfig = (UofRabbitConfiguration)Rabbit;
                rabbitConfig.Host = EnvironmentManager.GetMqHost(Environment);
                Rabbit = rabbitConfig;
            }

            if (Environment != SdkEnvironment.Custom || Api.Host.IsNullOrEmpty())
            {
                var apiConfig = (UofApiConfiguration)Api;
                apiConfig.Host = EnvironmentManager.GetApiHost(Environment);
                Api = apiConfig;
            }

            CheckAndUpdateConnectionSettings();
        }

        public void UpdateFromAppConfigSection(bool loadAll)
        {
            UpdateFromAppConfigSectionInternal(_uofConfigurationSectionProvider.GetSection(), loadAll);
        }

        private void UpdateFromAppConfigSectionInternal(IUofConfigurationSection section, bool loadAll)
        {
            if (section == null)
            {
                return;
            }

            UpdateCommonPropertiesFromSectionInternal(section, loadAll);

            if (loadAll)
            {
                if (AccessToken.IsNullOrEmpty())
                {
                    AccessToken = section.AccessToken;
                }

                UpdateSdkEnvironment(section.Environment);

                UpdateApiPropertiesFromSectionInternal(section);
                UpdateRabbitPropertiesFromSectionInternal(section);
            }

            CheckAndUpdateConnectionSettings();
            ValidateMinimumSettings();
        }

        private void UpdateCommonPropertiesFromSectionInternal(IUofConfigurationSection section, bool loadAll)
        {
            if (string.IsNullOrEmpty(section.AccessToken))
            {
                throw new InvalidOperationException("Missing access token in configuration section");
            }

            if (!string.IsNullOrEmpty(section.Languages))
            {
                var langCodes = section.Languages.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                Languages = langCodes.Select(langCode => new CultureInfo(langCode.Trim())).ToList();
            }

            if (!string.IsNullOrEmpty(section.DefaultLanguage))
            {
                DefaultLanguage = new CultureInfo(section.DefaultLanguage);
            }

            if (!string.IsNullOrEmpty(section.DisabledProducers))
            {
                var producerIds = section.DisabledProducers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                Producer.DisabledProducers.AddRange(producerIds.Select(producerId => int.Parse(producerId.Trim(), CultureInfo.InvariantCulture)));
            }

            if (section.NodeId > 0)
            {
                NodeId = section.NodeId;
            }

            ExceptionHandlingStrategy = section.ExceptionHandlingStrategy;
        }

        private void UpdateApiPropertiesFromSectionInternal(IUofConfigurationSection section)
        {
            var apiConfig = (UofApiConfiguration)Api;
            apiConfig.Host = string.IsNullOrEmpty(section.ApiHost)
                ? EnvironmentManager.GetApiHost(Environment)
                : section.ApiHost;
            if (section.ApiUseSsl != apiConfig.UseSsl)
            {
                apiConfig.UseSsl = section.ApiUseSsl;
            }
            Api = apiConfig;
        }

        private void UpdateRabbitPropertiesFromSectionInternal(IUofConfigurationSection section)
        {
            var rabbitConfig = (UofRabbitConfiguration)Rabbit;
            rabbitConfig.Host = string.IsNullOrEmpty(section.RabbitHost)
                ? EnvironmentManager.GetMqHost(Environment)
                : section.RabbitHost;
            if (section.RabbitPort > 0)
            {
                rabbitConfig.Port = section.RabbitPort;
            }
            if (section.RabbitUseSsl != rabbitConfig.UseSsl)
            {
                rabbitConfig.UseSsl = section.RabbitUseSsl;
            }
            if (!section.RabbitUsername.IsNullOrEmpty())
            {
                rabbitConfig.Username = section.RabbitUsername;
            }
            if (!section.RabbitPassword.IsNullOrEmpty())
            {
                rabbitConfig.Password = section.RabbitPassword;
            }
            if (!section.RabbitVirtualHost.IsNullOrEmpty())
            {
                rabbitConfig.VirtualHost = section.RabbitVirtualHost;
            }
            Rabbit = rabbitConfig;
        }

        internal void UpdateBookmakerDetails(IBookmakerDetails bookmakerDetails, string apiHostName)
        {
            BookmakerDetails = bookmakerDetails;

            if (Environment != SdkEnvironment.Custom)
            {
                var apiConfig = (UofApiConfiguration)Api;
                apiConfig.Host = apiHostName;
                apiConfig.UseSsl = true;
                Api = apiConfig;
            }

            if (Environment != SdkEnvironment.Custom || Rabbit.VirtualHost.IsNullOrEmpty())
            {
                var rabbitConfig = (UofRabbitConfiguration)Rabbit;
                rabbitConfig.VirtualHost = bookmakerDetails.VirtualHost;
                Rabbit = rabbitConfig;
            }

            CheckAndUpdateConnectionSettings();
        }

        private void CheckAndUpdateConnectionSettings()
        {
            var apiConfig = (UofApiConfiguration)Api;

            if (apiConfig.Host.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                apiConfig.Host = apiConfig.Host.Substring(7);  //remove leading http://
                apiConfig.UseSsl = false;
                Api = apiConfig;
            }
            else if (apiConfig.Host.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                apiConfig.Host = apiConfig.Host.Substring(8); //remove leading https://
                apiConfig.UseSsl = true;
                Api = apiConfig;
            }

            if (Environment != SdkEnvironment.Custom)
            {
                var rabbitConfig = (UofRabbitConfiguration)Rabbit;
                rabbitConfig.Port = rabbitConfig.UseSsl ? EnvironmentManager.DefaultMqHostPort : 5672;
                rabbitConfig.Username = AccessToken;
                rabbitConfig.Password = null;
                Rabbit = rabbitConfig;
            }
        }

        public void ValidateMinimumSettings()
        {
            if (DefaultLanguage == null && Languages.Any())
            {
                DefaultLanguage = Languages.First();
            }
            if (!Languages.Contains(DefaultLanguage))
            {
                Languages.Insert(0, DefaultLanguage);
            }

            if (DefaultLanguage == null)
            {
                throw new InvalidOperationException("Missing default language");
            }
            if (string.IsNullOrEmpty(AccessToken))
            {
                throw new InvalidOperationException("Missing access token");
            }
        }

        public override string ToString()
        {
            var languagesStr = Languages.IsNullOrEmpty()
                ? string.Empty
                : string.Join(",", Languages.Select(s => s.TwoLetterISOLanguageName));
            var sanitizedToken = SdkInfo.ClearSensitiveData(AccessToken);
            var bookmakerDetailsId = BookmakerDetails == null
                ? "0"
                : string.Intern(BookmakerDetails.BookmakerId.ToString(CultureInfo.InvariantCulture));

            var summaryValues = new Dictionary<string, string>
            {
                { "AccessToken", sanitizedToken },
                { "NodeId", NodeId.ToString(CultureInfo.InvariantCulture) },
                { "DefaultLanguage", DefaultLanguage?.TwoLetterISOLanguageName },
                { "Languages", languagesStr },
                { "Environment", string.Intern(Environment.ToString()) },
                { "ExceptionHandlingStrategy", string.Intern(ExceptionHandlingStrategy.ToString()) },
                { "BookmakerId", bookmakerDetailsId }
            };

            var sb = new StringBuilder();
            sb.Append(", ").Append(Api)
                .Append(", ").Append(Rabbit)
                .Append(", ").Append(Cache)
                .Append(", ").Append(Producer)
                .Append(", ").Append(Additional);

            return "UofConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + sb + "}";
        }
    }
}
