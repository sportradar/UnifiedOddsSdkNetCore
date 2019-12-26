/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Common.Exceptions
{
    /// <summary>
    /// An exception when the name for a market or outcome could not be generated
    /// </summary>
    /// <seealso cref="FeedSdkException" />
    [Serializable]
    public class NameGenerationException : FeedSdkException
    {
        /// <summary>
        /// Gets the id of the market associated with name generation that caused the exception
        /// </summary>
        public int MarketId { get; }

        /// <summary>
        /// Gets the specifiers of the market associated with name generation that caused the exception
        /// </summary>
        /// <value>The market specifiers.</value>
        public IReadOnlyDictionary<string, string> MarketSpecifiers { get; }

        /// <summary>
        /// Gets the id of the outcome whose name generation caused the exception, or a null reference if the name generation was invoked on market
        /// </summary>
        public string OutcomeId { get; }

        /// <summary>
        /// Gets the name descriptor used when generating the name, or a null reference if name descriptor could not be found
        /// </summary>
        public string NameDescriptor { get; }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> instance specifying the language associated with the name generation
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameGenerationException"/> class.
        /// </summary>
        public NameGenerationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameGenerationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="marketId">The id of the market associated with name generation that caused the exception</param>
        /// <param name="marketSpecifiers">The specifiers of the market associated with name generation that caused the exception</param>
        /// <param name="outcomeId">The id of the outcome whose name generation caused the exception, or a null reference if the name generation was invoked on market</param>
        /// <param name="nameDescriptor">The name descriptor used when generating the name, or a null reference if name descriptor could not be found</param>
        /// <param name="culture">The <see cref="CultureInfo"/> instance specifying the language associated with the name generation</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public NameGenerationException(string message, int marketId, IReadOnlyDictionary<string, string> marketSpecifiers, string outcomeId, string nameDescriptor, CultureInfo culture, Exception innerException)
            : base(message, innerException)
        {
            MarketId = marketId;
            MarketSpecifiers = marketSpecifiers;
            OutcomeId = outcomeId;
            NameDescriptor = nameDescriptor;
            Culture = culture;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NameGenerationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        public NameGenerationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            MarketId = info.GetInt32("sdkMarketId");
            MarketSpecifiers = (IReadOnlyDictionary<string, string>)info.GetValue("sdkMarketSpecifiers", typeof(IReadOnlyDictionary<string, string>));
            OutcomeId = info.GetString("sdkOutcomeId");
            NameDescriptor = info.GetString("sdkNameDescriptor");
            Culture = (CultureInfo) info.GetValue("sdkCulture", typeof(CultureInfo));
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sdkMarketId", MarketId);
            info.AddValue("sdkMarketSpecifiers", MarketSpecifiers, typeof(IReadOnlyDictionary<string, string>));
            info.AddValue("sdkOutcomeId", OutcomeId);
            info.AddValue("sdkNameDescriptor", NameDescriptor);
            info.AddValue("sdkCulture", Culture, typeof(CultureInfo));
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var specifiersString = MarketSpecifiers == null
                ? "null"
                : string.Join(SdkInfo.SpecifiersDelimiter, MarketSpecifiers.Select(k => $"{k.Key}={k.Value}"));

            var sb = new StringBuilder(base.ToString());
            sb.Append(" MarketId=").Append(MarketId)
                .Append(" MarketSpecifiers=").Append(specifiersString)
                .Append(" OutcomeId=").Append(OutcomeId ?? "null")
                .Append(" NameDescriptor=").Append(NameDescriptor ?? "null")
                .Append(" Culture=").Append(Culture?.TwoLetterISOLanguageName ?? "null");
            return sb.ToString();

        }
    }
}
