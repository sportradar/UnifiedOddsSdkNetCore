/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.API;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils
{
    /// <summary>
    /// Defines a contract implemented by classes used to process entities dispatched by SDK's <see cref="IEntityDispatcher{T}"/> instances
    /// </summary>
    internal interface IEntityProcessor
    {
        /// <summary>
        /// Opens the current processor so it will start processing dispatched entities.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the current processor so it will no longer process dispatched entities
        /// </summary>
        void Close();
    }
}
