// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Defines a contract implemented by classes which can be opened and closed
    /// </summary>
    public interface IOpenable
    {
        /// <summary>
        /// Gets a value indicating whether the current instance is opened
        /// </summary>
        bool IsOpened { get; }

        /// <summary>
        /// Opens the current instance.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the current instance
        /// </summary>
        void Close();
    }
}
