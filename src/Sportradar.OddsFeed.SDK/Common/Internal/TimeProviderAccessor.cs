/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Class providing access to <see cref="ITimeProvider"/>
    /// </summary>
    /// <remarks>
    /// The implementation is not thread safe and <see cref="ITimeProvider"/> provided by
    /// this type should not be switched in when the SDK is running (i.e. only in unit tests)
    /// </remarks>
    public static class TimeProviderAccessor
    {
        /// <summary>
        /// Gets a <see cref="ITimeProvider"/> providing access to the current time
        /// </summary>
        public static ITimeProvider Current { get; private set; }

        static TimeProviderAccessor()
        {
            Current = new RealTimeProvider();
        }

        /// <summary>
        /// Sets the <see cref="ITimeProvider"/> provided by the <see cref="TimeProviderAccessor"/>
        /// </summary>
        /// <param name="timeProvider"></param>
        public static void SetTimeProvider(ITimeProvider timeProvider)
        {
            Guard.Argument(timeProvider, nameof(timeProvider)).NotNull();

            Current = timeProvider;
        }
    }
}