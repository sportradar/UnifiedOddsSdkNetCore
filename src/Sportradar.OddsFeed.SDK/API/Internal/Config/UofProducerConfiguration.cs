using System;
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    internal class UofProducerConfiguration : IUofProducerConfiguration
    {
        public bool AdjustAfterAge { get; set; }
        public TimeSpan InactivitySeconds { get; set; }
        public TimeSpan InactivitySecondsPrematch { get; set; }
        public List<int> DisabledProducers { get; set; }
        public TimeSpan MaxRecoveryTime { get; set; }
        public TimeSpan MinIntervalBetweenRecoveryRequests { get; set; }
        public IReadOnlyCollection<IProducer> Producers { get; set; }

        public UofProducerConfiguration()
        {
            AdjustAfterAge = true;
            DisabledProducers = new List<int>();
            InactivitySeconds = TimeSpan.FromSeconds(ConfigLimit.InactivitySecondsDefault);
            InactivitySecondsPrematch = TimeSpan.FromSeconds(ConfigLimit.InactivitySecondsPrematchDefault);
            MaxRecoveryTime = TimeSpan.FromSeconds(ConfigLimit.MaxRecoveryTimeDefault);
            MinIntervalBetweenRecoveryRequests = TimeSpan.FromSeconds(ConfigLimit.MinIntervalBetweenRecoveryRequestDefault);
            Producers = null;
        }

        public override string ToString()
        {
            var disabledProducers = DisabledProducers.IsNullOrEmpty()
                ? string.Empty
                : string.Join(",", DisabledProducers);

            var summaryValues = new Dictionary<string, string>
            {
                { "InactivitySeconds", InactivitySeconds.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "InactivitySecondsPrematch", InactivitySecondsPrematch.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "MaxRecoveryTime", MaxRecoveryTime.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "MinIntervalBetweenRecoveryRequests", MinIntervalBetweenRecoveryRequests.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
                { "DisabledProducers", disabledProducers },
                { "AdjustAfterAge", AdjustAfterAge.ToString(CultureInfo.InvariantCulture) }
            };
            return "ProducerConfiguration{" + SdkInfo.DictionaryToString(summaryValues) + "}";
        }
    }
}
