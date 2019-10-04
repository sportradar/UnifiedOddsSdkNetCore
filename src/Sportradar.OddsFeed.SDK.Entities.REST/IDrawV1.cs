/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Entities.REST
{
    /// <summary>
    /// Defines a contract implemented by classes, which represent information about a lottery draw
    /// </summary>
    public interface IDrawV1 : IDraw
    {
        /// <summary>
        /// Asynchronously gets a <see cref="int"/> representing display id
        /// </summary>
        /// <returns>The display id</returns>
        Task<int?> GetDisplayIdAsync();
    }
}
