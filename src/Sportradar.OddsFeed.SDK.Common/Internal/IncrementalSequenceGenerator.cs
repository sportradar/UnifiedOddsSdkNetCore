/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using System.Threading;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// A <see cref="ISequenceGenerator"/> which generates incremental sequence numbers
    /// </summary>
    public class IncrementalSequenceGenerator : ISequenceGenerator
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
        /// Initializes a new instance of the <see cref="IncrementalSequenceGenerator"/> which generates positive sequence numbers
        /// </summary>
        public IncrementalSequenceGenerator()
            : this(0, long.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncrementalSequenceGenerator"/> which generates sequence numbers between specified min and max value
        /// </summary>
        /// <param name="minValue">The minimum allowed value for generated sequence numbers</param>
        /// <param name="maxValue">The maximum allowed value for generated sequence numbers</param>
        public IncrementalSequenceGenerator(long minValue, long maxValue)
        {
            Guard.Argument(maxValue).Require(maxValue > minValue);

            _minValue = minValue;
            _maxValue = maxValue;
            _value = minValue;
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