/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.REST;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes capable of dispatching only specific entities
    /// </summary>
    /// <typeparam name="T">Specifies the type of the entities that can be dispatched</typeparam>
    /// <seealso cref="IOpenable" />
    /// <seealso cref="IEntityDispatcher{T}" />
    public interface ISpecificEntityDispatcher<T> : IOpenable, IEntityDispatcher<T> where T : ISportEvent
    {

    }
}
