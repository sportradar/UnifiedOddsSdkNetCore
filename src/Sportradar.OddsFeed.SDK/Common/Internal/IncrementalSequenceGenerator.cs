// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading;
using Dawn;
using Microsoft.Extensions.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// A <see cref="ISequenceGenerator"/> which generates incremental sequence numbers
    /// </summary>
    internal class IncrementalSequenceGenerator : ISequenceGenerator
    {
        /// <summary>
        /// The minimum allowed value for generated sequence numbers
        /// </summary>
        private readonly long _minValue;

        /// <summary>
        /// The maximum allowed value for generated sequence numbers
        /// </summary>
        private readonly long _maxValue;

        /// <summary>
        /// Current sequence number
        /// </summary>
        private long _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalSequenceGenerator"/> which generates sequence numbers between specified min and max value
        /// </summary>
        /// <param name="logger">Logger used to log exec messages</param>
        /// <param name="minValue">The minimum allowed value for generated sequence numbers</param>
        /// <param name="maxValue">The maximum allowed value for generated sequence numbers</param>
        public IncrementalSequenceGenerator(ILogger<IncrementalSequenceGenerator> logger, long minValue = 0, long maxValue = long.MaxValue)
        {
            Guard.Argument(logger).NotNull();
            Guard.Argument(maxValue, nameof(maxValue)).Require(maxValue > minValue);

            if (minValue == -1)
            {
                var seed = (int)DateTime.Now.Ticks;
                minValue = SdkInfo.GetRandom(Math.Abs(seed));
            }

            _minValue = minValue;
            _maxValue = maxValue;
            _value = minValue;

            logger.LogInformation("Initializing sequence generator with MinValue={minValue}, MaxValue={maxValue}", _minValue, _maxValue);
        }

        /// <summary>
        /// Gets the next available sequence number
        /// </summary>
        /// <returns>the next available sequence number</returns>
        public long GetNext()
        {
            long currentValue;
            long newValue;
            do
            {
                currentValue = Interlocked.Read(ref _value);
                newValue = currentValue + 1 < _maxValue
                               ? currentValue + 1
                               : _minValue;
            } while (currentValue != Interlocked.CompareExchange(ref _value, newValue, currentValue));
            return newValue;
        }
    }
}
