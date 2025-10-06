// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Text;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using WireMock.Admin.Requests;
using WireMock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Api;

public class WireMockLogger : IWireMockLogger
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly int _limitBodyLength;
    private readonly bool _ignoreBaseResponses;

    public WireMockLogger(ITestOutputHelper outputHelper, int limitBodyLength = 0, bool ignoreBaseResponses = true)
    {
        _outputHelper = outputHelper;
        _limitBodyLength = limitBodyLength;
        _ignoreBaseResponses = ignoreBaseResponses;
    }

    public void Debug(string formatString, params object[] args)
    {
        _outputHelper.WriteLine("DEBUG " + formatString, args);
    }

    public void Info(string formatString, params object[] args)
    {
        _outputHelper.WriteLine("INFO " + formatString, args);
    }

    public void Warn(string formatString, params object[] args)
    {
        _outputHelper.WriteLine("WARN " + formatString, args);
    }

    public void Error(string formatString, params object[] args)
    {
        _outputHelper.WriteLine("ERROR " + formatString, args);
    }

    public void Error(string formatString, Exception exception)
    {
        _outputHelper.WriteLine("ERROR " + formatString);
    }

    public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminRequest)
    {
        _outputHelper.WriteLine(FormatLogEntry(logEntryModel));
    }

    private string FormatLogEntry(LogEntryModel logEntryModel)
    {
        var sb = new StringBuilder();
        var body = logEntryModel.Request.Body.IsNullOrEmpty() ? string.Empty : $" Body: {logEntryModel.Request.Body}";
        sb.AppendLine($"Request: {logEntryModel.Request.DateTime.ToLongTimeString()} {logEntryModel.Request.Method} {logEntryModel.Request.Url}{body}");
        sb.AppendLine($"Response StatusCode: {logEntryModel.Response.StatusCode?.ToString()} Body: {FormatResponseBody(logEntryModel.Request.Url, logEntryModel.Response.Body)}");
        return sb.ToString();
    }

    private string FormatResponseBody(string requestUrl, string responseBody)
    {
        if (responseBody.IsNullOrEmpty())
        {
            return string.Empty;
        }
        if (_ignoreBaseResponses)
        {
            foreach (var xmlSubstring in WireMockMappingBuilder.LogIgnoreUris)
            {
                if (requestUrl.Contains(xmlSubstring, StringComparison.InvariantCultureIgnoreCase))
                {
                    return $"{responseBody.Length} characters of xml response";
                }
            }
        }
        if (_limitBodyLength == 0)
        {
            return responseBody;
        }
        return responseBody.Length <= _limitBodyLength
                   ? responseBody
                   : string.Concat(responseBody.AsSpan(0, _limitBodyLength), "...");
    }
}
