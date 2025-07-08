// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// Encapsulates metadata about a request.
    /// </summary>
    internal class RequestOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestOptions"/> class with the specified execution path.
        /// </summary>
        /// <param name="executionPath">The execution path of the request.</param>
        public RequestOptions(ExecutionPath executionPath)
        {
            ExecutionPath = executionPath;
        }

        /// <summary>
        /// Gets the execution path indicating the execution path of the request.
        /// </summary>
        public ExecutionPath ExecutionPath { get; }
    }
}
