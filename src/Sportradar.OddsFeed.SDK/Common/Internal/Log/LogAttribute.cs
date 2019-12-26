/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Log
{
    /// <summary>
    /// A method or class attribute indicating if the method input and output parameters should be logged
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LogAttribute : Attribute
    {
        /// <summary>
        /// The <see cref="LoggerType"/> of the attribute
        /// </summary>
        public readonly LoggerType LoggerType;

        /// <summary>
        /// Initializes new Log attribute with specified <see cref="LoggerType"/>
        /// </summary>
        /// <param name="loggerType">A <see cref="LoggerType"/> used to create new log attribute</param>
        public LogAttribute(LoggerType loggerType = LoggerType.Execution)
        {
            LoggerType = loggerType;
        }

        //public bool Verify()
        //{
        //    return LoggerType.ToString().StartsWith("D");
        //}
    }
}
