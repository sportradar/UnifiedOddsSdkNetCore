/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// Maps <see cref="sportCategoriesEndpoint"/> instances to <see cref="SportCategoriesDTO" /> instance
    /// </summary>
    internal class SportCategoriesMapper : ISingleTypeMapper<SportCategoriesDTO>
    {
        /// <summary>
        /// A <see cref="sportCategoriesEndpoint"/> instance containing sport categories
        /// </summary>
        private readonly sportCategoriesEndpoint _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportCategoriesMapper"/> class
        /// </summary>
        /// <param name="data">>A <see cref="sportCategoriesEndpoint"/> instance containing sport categories</param>
        internal SportCategoriesMapper(sportCategoriesEndpoint data)
        {
            Contract.Requires(data != null);
            Contract.Requires(data.sport != null);

            _data = data;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
            Contract.Invariant(_data.sport != null);
        }

        /// <summary>
        /// Maps it's data to <see cref="SportCategoriesDTO"/> instance
        /// </summary>
        /// <returns>Constructed <see cref="SportCategoriesDTO"/> instance</returns>
        public SportCategoriesDTO Map()
        {
            return new SportCategoriesDTO(_data);
        }
    }
}
