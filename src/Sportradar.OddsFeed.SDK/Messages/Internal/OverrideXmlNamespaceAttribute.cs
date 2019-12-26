/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Messages.Internal
{
    /// <summary>
    /// Attributes providing additional information to deserializers. This is only required to overcome problems
    /// caused by XSD schema issues and will be removed when the schemes are fixed
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OverrideXmlNamespaceAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating whether the xml namespace should be ignored when deserializing the
        /// xml message
        /// </summary>
        public bool IgnoreNamespace
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the document namespace.
        /// </summary>
        /// <value>The document namespace.</value>
        public string DocumentNamespace { get; set; }

        /// <summary>
        /// Gets or sets the name of the root xml element in the associated xml messages
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OverrideXmlNamespaceAttribute"/> class
        /// </summary>
        public OverrideXmlNamespaceAttribute()
        {
            IgnoreNamespace = true;
        }
    }
}
