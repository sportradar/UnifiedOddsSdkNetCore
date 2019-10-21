/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common.Exceptions;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines extension methods for Func classes which catch pre-defined exception types
    /// </summary>
    public static class SafeInvoker
    {
        /// <summary>
        /// Handles the provided exception by logging it's data along with the provided message
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to be handled.</param>
        /// <param name="log">The <see cref="ILog"/> instance where the log should be written.</param>
        /// <param name="errorMessage">The error message to be written along with exception data.</param>
        /// <returns><c>true</c> True if the exception was of the following types: <see cref="CommunicationException"/>, <see cref="DeserializationException"/> or <see cref="MappingException"/>.</returns>
        private static bool HandleException(Exception ex, ILog log, string errorMessage)
        {
            Guard.Argument(ex).NotNull();
            Guard.Argument(log).NotNull();

            if (ex is CommunicationException || ex is DeserializationException || ex is MappingException)
            {
                log.Error(errorMessage, ex);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Safely invokes the provided asynchronous function
        /// </summary>
        /// <param name="method">A <see cref="Func{Task}"/> representing the method.</param>
        /// <param name="log">The <see cref="ILog"/> where potential exceptions should be logged.</param>
        /// <param name="errorMessage">The error message to be written along with the exception.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SafeInvokeAsync(this Func<Task> method, ILog log, string errorMessage)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(log).NotNull();

            try
            {
                await method.Invoke().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (HandleException(ex, log, errorMessage))
                {
                    return;
                }
                throw;
            }
        }

        /// <summary>
        /// Safely invokes the provided asynchronous function
        /// </summary>
        /// <param name="method">A <see cref="Func{T}"/> representing the method.</param>
        /// <param name="log">The <see cref="ILog"/> where potential exceptions should be logged.</param>
        /// <param name="errorMessage">The error message to be written along with the exception.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        public static async Task<TResult> SafeInvokeAsync<TResult>(this Func<Task<TResult>> method, ILog log, string errorMessage)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(log).NotNull();

            try
            {
                return await method().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (HandleException(ex, log, errorMessage))
                {
                    return default(TResult);
                }
                throw;
            }
        }

        /// <summary>
        /// Safely invokes the provided asynchronous function
        /// </summary>
        /// <param name="method">A <see cref="Func{Task}"/> representing the method.</param>
        /// <param name="arg">The function argument</param>
        /// <param name="log">The <see cref="ILog"/> where potential exceptions should be logged.</param>
        /// <param name="errorMessage">The error message to be written along with the exception.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
        public static async Task<TResult> SafeInvokeAsync<T, TResult>(this Func<T, Task<TResult>> method, T arg, ILog log, string errorMessage)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(log).NotNull();

            try
            {
                return await method(arg).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (HandleException(ex, log, errorMessage))
                {
                    return default(TResult);
                }
                throw;
            }
        }

        /// <summary>
        /// Safely invokes the provided asynchronous function
        /// </summary>
        /// <param name="method">A <see cref="Func{Task}"/> representing the method.</param>
        /// <param name="arg1">The function's first argument</param>
        /// <param name="arg2">The function's second argument</param>
        /// <param name="log">The <see cref="ILog"/> where potential exceptions should be logged.</param>
        /// <param name="errorMessage">The error message to be written along with the exception.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TResult> SafeInvokeAsync<T1, T2, TResult>(this Func<T1, T2, Task<TResult>> method, T1 arg1, T2 arg2, ILog log, string errorMessage)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(log).NotNull();

            try
            {
                return await method(arg1, arg2).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (HandleException(ex, log, errorMessage))
                {
                    return default(TResult);
                }
                throw;
            }
        }

        /// <summary>
        /// Safely invokes the provided asynchronous function
        /// </summary>
        /// <param name="method">A <see cref="Func{Task}"/> representing the method.</param>
        /// <param name="arg1">The function's first argument</param>
        /// <param name="arg2">The function's second argument</param>
        /// <param name="arg3">The function's second argument</param>
        /// <param name="log">The <see cref="ILog"/> where potential exceptions should be logged.</param>
        /// <param name="errorMessage">The error message to be written along with the exception.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<TResult> SafeInvokeAsync<T1, T2, T3, TResult>(this Func<T1, T2, T3, Task<TResult>>  method, T1 arg1, T2 arg2, T3 arg3, ILog log, string errorMessage)
        {
            Guard.Argument(method).NotNull();
            Guard.Argument(log).NotNull();

            try
            {
                return await method(arg1, arg2, arg3).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (HandleException(ex, log, errorMessage))
                {
                    return default(TResult);
                }
                throw;
            }
        }

    }
}
