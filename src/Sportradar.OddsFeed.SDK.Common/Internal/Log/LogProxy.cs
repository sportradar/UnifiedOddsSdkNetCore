/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Common.Logging;

namespace Sportradar.OddsFeed.SDK.Common.Internal.Log
{
    /// <summary>
    /// A log proxy used to log input and output parameters of a method
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LogProxy<T> : DispatchProxy
    {
        private Predicate<MethodInfo> _filter;
        private LoggerType _defaultLoggerType;
        private bool _canOverrideLoggerType;

        private T _decorated;

        private struct LogProxyPerm
        {
            public bool LogEnabled;
            public MethodInfo MethodInfo;
            public object Result;
            public ILog Logger;
            public Stopwatch Watch;
        }

        private readonly IDictionary<int, LogProxyPerm> _proxyPerms;

        /// <summary>
        /// Initializes new instance of the <see cref="LogProxy{T}"/>
        /// </summary>
        /// <param name="decorated">A base class to be decorated</param>
        /// <param name="loggerType">A <see cref="LoggerType"/> to be used within the proxy</param>
        /// <param name="canOverrideLoggerType">A value indicating if the <see cref="LoggerType"/> can be overridden with <see cref="LogAttribute"/> on a method or class</param>
        public LogProxy(T decorated, LoggerType loggerType = LoggerType.Execution, bool canOverrideLoggerType = true)
        {
            _decorated = decorated;
            _defaultLoggerType = loggerType;
            _canOverrideLoggerType = canOverrideLoggerType;

            _proxyPerms = new ConcurrentDictionary<int, LogProxyPerm>();
        }

        ///// <summary>
        ///// A Predicate used to filter which class methods may be logged
        ///// </summary>
        //public Predicate<MethodInfo> Filter
        //{
        //    get => _filter;
        //    set { _filter = value ?? (m => true); }
        //}



        //protected override object Invoke(MethodInfo targetMethod, object[] args)
        //{
        //    try
        //    {
        //        LogBefore(targetMethod, args);

        //        var result = targetMethod.Invoke(_decorated, args);

        //        LogAfter(targetMethod, args, result);
        //        return result;
        //    }
        //    catch (Exception ex) when (ex is TargetInvocationException)
        //    {
        //        LogException(ex.InnerException ?? ex, targetMethod);
        //        throw ex.InnerException ?? ex;
        //    }
        //}

        public static T Create(T decorated, Predicate<MethodInfo> filter, LoggerType loggerType = LoggerType.Execution, bool canOverrideLoggerType = true)
        {
            if (decorated == null)
            {
                throw new ArgumentNullException(nameof(decorated));
            }
            object proxy = Create<T, LogProxy<T>>();
            ((LogProxy<T>)proxy)._decorated = decorated;
            ((LogProxy<T>)proxy)._filter = filter;
            ((LogProxy<T>)proxy)._defaultLoggerType = loggerType;
            ((LogProxy<T>)proxy)._canOverrideLoggerType = canOverrideLoggerType;

            return (T)proxy;
        }

        //private void LogException(Exception exception, MethodInfo methodInfo = null)
        //{
        //    Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} threw exception:\n{exception}");
        //}

        //private void LogAfter(MethodInfo methodInfo, object[] args, object result)
        //{
        //    Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} executed, Output: {result}");
        //}

        //private void LogBefore(MethodInfo methodInfo, object[] args)
        //{
        //    Console.WriteLine($"Class {_decorated.GetType().FullName}, Method {methodInfo.Name} is executing");
        //}




        /// <summary>Whenever any method on the generated proxy type is called, this method is invoked to dispatch control.</summary>
        /// <param name="methodInfo">The method the caller invoked.</param>
        /// <param name="args">The arguments the caller passed to the method.</param>
        /// <returns>The object to return to the caller, or <see langword="null" /> for void methods.</returns>
        protected override object Invoke(MethodInfo methodInfo, object[] args)
        {
            var logEnabled = false;
            if (methodInfo == null)
            {
                throw new ArgumentException("Input parameter 'msg' does not have MethodBase as MethodInfo.");
            }

            var logger = SdkLoggerFactory.GetLogger(methodInfo.ReflectedType, SdkLoggerFactory.SdkLogRepositoryName, _defaultLoggerType);

            if (_filter != null && _filter(methodInfo))
            {
                logEnabled = true;
            }

            if (!logEnabled || _canOverrideLoggerType)
            {
                var attributes = methodInfo.GetCustomAttributes(true).ToList();
                if (methodInfo.DeclaringType != null)
                {
                    attributes.AddRange(methodInfo.DeclaringType.GetCustomAttributes(true));
                }

                if (attributes.Count > 0)
                {
                    foreach (var t in attributes)
                    {
                        if (!(t is LogAttribute))
                        {
                            continue;
                        }
                        logEnabled = true;
                        if (_canOverrideLoggerType)
                        {
                            logger = SdkLoggerFactory.GetLogger(methodInfo.ReflectedType, SdkLoggerFactory.SdkLogRepositoryName, ((LogAttribute) t).LoggerType);
                        }
                        break;
                    }
                }
            }

            var watch = new Stopwatch();
            watch.Start();

            try
            {
                if (methodInfo.Name == "GetType")
                {
                    logEnabled = false;
                }
                if (logEnabled)
                {
                    logger.Info($"Starting executing '{methodInfo.Name}' ...");
                }

                var methodCall = $"{methodInfo.Name}()";
                if (logEnabled && args != null && args.Any())
                {
                    logger.Debug($"{methodInfo.Name} arguments:");
                    for (var i = 0; i < args.Length; i++)
                    {
                        methodCall += $",{methodInfo.GetGenericArguments()[i].Name}={args[i]}";
                        logger.Debug($"\t{methodInfo.GetGenericArguments()[i].Name}={args[i]}");
                    }

                    methodCall = $"{methodInfo.Name}({methodCall.Substring(1)})";
                    logger.Debug($"Invoking '{methodCall}' ...");
                }

                var result = methodInfo.Invoke(_decorated, args); // MAIN EXECUTION

                var task = result as Task;
                if (task != null)
                {
                    var perm = new LogProxyPerm
                               {
                                   LogEnabled = logEnabled,
                                   Logger = logger,
                                   MethodInfo = methodInfo,
                                   Result = result,
                                   Watch = watch
                               };
                    _proxyPerms.Add(task.Id, perm);
                    if (logEnabled)
                    {
                        logger.Debug($"TaskId:{task.Id} is executing and we wait to finish ...");
                    }
                    task.ContinueWith(TaskExecutionFinished);
                    return task; // new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
                }

                FinishExecution(logEnabled, methodInfo, result?.GetType().Name, result, logger, watch);
                return result;
            }
            catch (Exception e)
            {
                watch.Stop();
                if (logEnabled)
                {
                    logger.Error($"Exception during executing '{methodInfo.Name}': {Environment.NewLine}", e);
                }

                throw;
            }
        }

        private void TaskExecutionFinished(Task task)
        {
            LogProxyPerm perm;
            if (!_proxyPerms.TryGetValue(task.Id, out perm))
            {
                Debug.WriteLine($"No perm for task. Id: {task.Id}");
                return;
            }
            var underlyingResultType = "Task->" + perm.Result.GetType().GetProperty("Result")?.PropertyType.Name;

            if (task.IsFaulted)
            {
                var exceptionMsg = "EXCEPTION: ";
                if (task.Exception != null)
                {
                    if (task.Exception.InnerExceptions != null)
                    {
                        exceptionMsg += task.Exception.InnerExceptions[0].ToString();
                    }
                    else
                    {
                        exceptionMsg += task.Exception.ToString();
                    }
                }
                FinishExecution(logEnabled: perm.LogEnabled,
                                methodInfo: perm.MethodInfo,
                                resultTypeName: underlyingResultType,
                                result: exceptionMsg,
                                logger: perm.Logger,
                                watch: perm.Watch,
                                taskId: $"TaskId:{task.Id}, ");
                return;
            }
            var value = perm.Result.GetType().GetProperty("Result")?.GetValue(task);

            FinishExecution(logEnabled: perm.LogEnabled,
                            methodInfo: perm.MethodInfo,
                            resultTypeName: underlyingResultType,
                            result: value,
                            logger: perm.Logger,
                            watch: perm.Watch,
                            taskId: $"TaskId:{task.Id}, ");
            _proxyPerms.Remove(task.Id);
        }

        private static void FinishExecution(bool logEnabled,
                                            MethodInfo methodInfo,
                                            string resultTypeName,
                                            object result,
                                            ILog logger,
                                            Stopwatch watch,
                                            string taskId = null)
        {
            watch.Stop();

            if (logEnabled)
            {
                logger.Info($"{taskId}Finished executing '{methodInfo.Name}'. Time: {watch.ElapsedMilliseconds} ms.");
            }

            if (logEnabled && !string.Equals(methodInfo.ReturnType.FullName, "System.Void"))
            {
                var responseMessage = result as HttpResponseMessage;
                if (responseMessage != null)
                {
                    logger.Debug($"{taskId}{methodInfo.Name} result: {resultTypeName}={WriteHttpResponseMessage(responseMessage)}");
                }
                else
                {
                    logger.Debug($"{taskId}{methodInfo.Name} result: {resultTypeName}={result};");
                }
            }
        }

        private static string WriteHttpResponseMessage(HttpResponseMessage message)
        {
            if (message == null)
            {
                return null;
            }
            return $"StatusCode: {message.StatusCode}, ReasonPhrase: '{message.ReasonPhrase}', Version: {message.Version}, Content: {message.Content}";
        }
    }
}
