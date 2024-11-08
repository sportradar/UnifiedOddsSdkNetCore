// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess
{
    /// <summary>
    /// An implementation of <see cref="IDataFetcher"/> and <see cref="IDataPoster"/> which uses the HTTP requests to fetch or post the requested data. All request are logged.
    /// </summary>
    /// <seealso cref="IDataFetcher" />
    /// <remarks>ALL, DEBUG, INFO, WARN, ERROR, FATAL, OFF - the levels are defined in order of increasing priority</remarks>
    internal class LogHttpDataFetcher : HttpDataFetcher, ILogHttpDataFetcher
    {
        private readonly ILogger _restLog;

        private readonly ISequenceGenerator _sequenceGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHttpDataFetcher"/> class.
        /// </summary>
        /// <param name="sdkHttpClient">A <see cref="ISdkHttpClient"/> used to invoke HTTP requests</param>
        /// <param name="sequenceGenerator">A <see cref="ISequenceGenerator"/> used to identify requests</param>
        /// <param name="responseDeserializer">The deserializer for unexpected response</param>
        /// <param name="logger">Logger to log rest requests</param>
        /// <param name="connectionFailureLimit">Indicates the limit of consecutive request failures, after which it goes in "blocking mode"</param>
        /// <param name="connectionFailureTimeout">indicates the timeout after which comes out of "blocking mode" (in seconds)</param>
        public LogHttpDataFetcher(ISdkHttpClient sdkHttpClient, ISequenceGenerator sequenceGenerator, IDeserializer<response> responseDeserializer, ILogger logger, int connectionFailureLimit = 50, int connectionFailureTimeout = 15)
            : base(sdkHttpClient, responseDeserializer, connectionFailureLimit, connectionFailureTimeout)
        {
            Guard.Argument(sequenceGenerator, nameof(sequenceGenerator)).NotNull();
            Guard.Argument(logger, nameof(logger)).NotNull();

            _sequenceGenerator = sequenceGenerator;

            _restLog = logger;
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Stream" /> containing data fetched from the provided <see cref="Uri" />
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be fetched</param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="Stream" /> containing fetched data</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        public override async Task<Stream> GetDataAsync(Uri uri)
        {
            var dataId = _sequenceGenerator.GetNext().ToString("D7", CultureInfo.InvariantCulture); // because request can take long time, there may be several request at the same time; Id to know what belongs together.

            var watch = Stopwatch.StartNew();
            Stream responseStream;
            try
            {
                responseStream = await base.GetDataAsync(uri).ConfigureAwait(false);
                watch.Stop();
            }
            catch (Exception ex)
            {
                watch.Stop();
                if (!(ex is CommunicationException commException))
                {
                    throw new CommunicationException("Failed to execute http get", uri.ToString(), ex);
                }
                const string msgErrorTemplate = "Id:{DataId} GET: {GetUri} Response: {ResponseStatusCode} took {Elapsed} ms Response: {ResponseContent}";
                _restLog.LogError(msgErrorTemplate, dataId, uri.AbsoluteUri, commException.ResponseCode, watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture), GetCommunicationExceptionContent(commException));
                throw;
            }

            if (!_restLog.IsEnabled(LogLevel.Debug))
            {
                const string msgSuccessTemplate = "Id:{DataId} GET: {GetUri} took {Elapsed} ms";
                _restLog.LogInformation(msgSuccessTemplate, dataId, uri.AbsoluteUri, watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
                return responseStream;
            }

            const string msgSuccessWithContentTemplate = "Id:{DataId} GET: {GetUri} took {Elapsed} ms Response: {ResponseContent}";
            var responseContent = await new StreamReader(responseStream).ReadToEndAsync();
            responseContent = responseContent.Replace("\n", string.Empty);

            _restLog.LogDebug(msgSuccessWithContentTemplate, dataId, uri.AbsoluteUri, watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture), responseContent);

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            await writer.WriteAsync(responseContent);
            await writer.FlushAsync();
            memoryStream.Position = 0;
            return memoryStream;
        }

        /// <summary>
        /// Gets a <see cref="Stream" /> containing data fetched from the provided <see cref="Uri" />
        /// </summary>
        /// <param name="uri">The <see cref="Uri" /> of the resource to be fetched</param>
        /// <returns>A <see cref="Task" /> which, when completed will return a <see cref="Stream" /> containing fetched data</returns>
        /// <exception cref="CommunicationException">Failed to execute http get</exception>
        public override Stream GetData(Uri uri)
        {
            return GetDataAsync(uri).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously gets a <see cref="Stream"/> containing data fetched from the provided <see cref="Uri"/>
        /// </summary>
        /// <param name="uri">The <see cref="Uri"/> of the resource to be fetched</param>
        /// <param name="content">A <see cref="HttpContent"/> to be posted to the specific <see cref="Uri"/></param>
        /// <returns>A <see cref="Task"/> which, when successfully completed will return a <see cref="HttpResponseMessage"/></returns>
        /// <exception cref="CommunicationException">Failed to execute http post</exception>
        public override async Task<HttpResponseMessage> PostDataAsync(Uri uri, HttpContent content = null)
        {
            var dataId = _sequenceGenerator.GetNext().ToString("D7", CultureInfo.InvariantCulture);

            await LogPostRequestHttpContent(dataId, uri, content);

            var watch = Stopwatch.StartNew();

            HttpResponseMessage response = null;
            try
            {
                response = await base.PostDataAsync(uri, content).ConfigureAwait(false);
                watch.Stop();
            }
            catch (Exception ex)
            {
                watch.Stop();
                _restLog.LogError("Id:{PostRequestId} POST: {PostUri} Response: {ResponseStatusCode} took {Elapsed} ms", dataId, uri.AbsoluteUri, response?.StatusCode, watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
                if (ex.GetType() != typeof(ObjectDisposedException) && ex.GetType() != typeof(TaskCanceledException))
                {
                    _restLog.LogError(ex, "{ErrorMessage}", ex.Message);
                }
                throw;
            }

            await LogPostResponseHttpContent(dataId, uri, response, watch);

            return response;
        }

        private async Task LogPostRequestHttpContent(string dataId, Uri uri, HttpContent content)
        {
            if (content != null)
            {
                try
                {
                    if (_restLog.IsEnabled(LogLevel.Debug))
                    {
                        var s = await content.ReadAsStringAsync().ConfigureAwait(false);
                        _restLog.LogDebug("Id:{PostRequestId} POST url: {PostUri} {PostContent}", dataId, uri.AbsoluteUri, s);
                    }
                    else
                    {
                        _restLog.LogInformation("Id:{PostRequestId} POST url: {PostUri}", dataId, uri.AbsoluteUri);
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                _restLog.LogInformation("Id:{PostRequestId} POST url: {PostUri}", dataId, uri.AbsoluteUri);
            }
        }

        private async Task LogPostResponseHttpContent(string dataId, Uri uri, HttpResponseMessage response, Stopwatch watch)
        {
            var responseContent = string.Empty;
            if (response.Content != null)
            {
                try
                {
                    responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }

            var wantedLogLevel = SdkLoggerFactory.GetWriteLogLevel(_restLog, LogLevel.Debug);
            if (!response.IsSuccessStatusCode)
            {
                wantedLogLevel = LogLevel.Warning;
            }
            else if (!_restLog.IsEnabled(LogLevel.Debug))
            {
                responseContent = string.Empty;
            }
            const string msgTemplate = "Id:{DataId} POST: {PostUri} took {Elapsed} ms. Response: {ResponseStatusCode}-{ResponseReasonPhrase} {ResponseContent}";
            _restLog.Log(wantedLogLevel,
                msgTemplate,
                dataId,
                uri.AbsoluteUri,
                watch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture),
                ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture),
                response.ReasonPhrase,
                responseContent);
        }

        private static string GetCommunicationExceptionContent(CommunicationException commException)
        {
            if (commException.Message.Contains("NotFound"))
            {
                return commException.Message;
            }
            if (commException.InnerException != null)
            {
                return commException.InnerException.Message;
            }
            return commException.Response == null
                ? commException.Message
                : commException.Response.Replace("\n", string.Empty);
        }
    }
}
