/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A factory used to build <see cref="IMappingValidator"/> instances
    /// </summary>
    public interface IMappingValidatorFactory
    {
        /// <summary>
        /// Builds and returns a <see cref="IMappingValidator"/> from the provided string
        /// </summary>
        /// <param name="value">A value defining the <see cref="IMappingValidator"/> to be constructed.</param>
        /// <returns>A <see cref="IMappingValidator"/> build from the provided string.</returns>
        IMappingValidator Build(string value);
    }
}