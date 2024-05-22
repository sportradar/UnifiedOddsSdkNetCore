// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal interface IUofConfigurationSection
    {
        /// <summary>
        /// Gets the access token
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the URL of the messaging broker
        /// </summary>
        string RabbitHost { get; }

        /// <summary>
        /// Gets the name of the virtual host configured on the messaging broker
        /// </summary>
        string RabbitVirtualHost { get; }

        /// <summary>
        /// Gets the port used to connect to the messaging broker
        /// </summary>
        int RabbitPort { get; }

        /// <summary>
        /// Gets the username used to connect to the messaging broker
        /// </summary>
        string RabbitUsername { get; }

        /// <summary>
        /// Gets the password used to connect to the messaging broker
        /// </summary>
        string RabbitPassword { get; }

        /// <summary>
        /// Gets a value indicating whether a secure connection to the messaging broker should be used
        /// </summary>
        bool RabbitUseSsl { get; }

        /// <summary>
        /// Gets the URL of the API host
        /// </summary>
        string ApiHost { get; }

        /// <summary>
        /// Gets a value indicating whether a secure connection to the Sports API should be used
        /// </summary>
        bool ApiUseSsl { get; }

        /// <summary>
        /// Gets the 2-letter ISO string of default language
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        string DefaultLanguage { get; }

        /// <summary>
        /// Gets the comma delimited string of all wanted languages
        /// </summary>
        /// <example>https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</example>
        string Languages { get; }

        /// <summary>
        /// Gets a <see cref="ExceptionHandlingStrategy"/> enum member specifying how to handle exceptions thrown to outside callers
        /// </summary>
        ExceptionHandlingStrategy ExceptionHandlingStrategy { get; }

        /// <summary>
        /// Gets the comma delimited list of ids of disabled producers
        /// </summary>
        string DisabledProducers { get; }

        /// <summary>
        /// Gets the node id
        /// </summary>
        /// <remarks>MTS customer must set this value! Use only positive numbers; negative are reserved for internal use.</remarks>
        int NodeId { get; }

        /// <summary>
        /// Gets a value indicating to which unified feed environment sdk should connect
        /// </summary>
        /// <remarks>Dependent on the other configuration, it may set MQ and API host address and port</remarks>
        SdkEnvironment Environment { get; }
    }
}
