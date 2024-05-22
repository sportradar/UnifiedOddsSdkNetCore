// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Text;
using WireMock.Admin.Requests;
using WireMock.Logging;
using Xunit.Abstractions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.MockApi;

public class WireMockLogger : IWireMockLogger
{
    private readonly ITestOutputHelper _outputHelper;

    public WireMockLogger(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    public void Debug(string formatString, params object[] args)
    {
        _outputHelper.WriteLine(formatString, args);
    }

    public void Info(string formatString, params object[] args)
    {
        _outputHelper.WriteLine(formatString, args);
    }

    public void Warn(string formatString, params object[] args)
    {
        _outputHelper.WriteLine(formatString, args);
    }

    public void Error(string formatString, params object[] args)
    {
        _outputHelper.WriteLine(formatString, args);
    }

    public void Error(string formatString, Exception exception)
    {
        _outputHelper.WriteLine(formatString);
    }

    public void DebugRequestResponse(LogEntryModel logEntryModel, bool isAdminRequest)
    {
        _outputHelper.WriteLine("DebugRequestResponse: " + logEntryModel);
    }

    public static string FormatLogEntry(ILogEntry logEntry)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Request: {logEntry.RequestMessage.DateTime.ToLongTimeString()} {logEntry.RequestMessage.Method} {logEntry.RequestMessage.Url}");
        sb.AppendLine($"Request Body: {logEntry.RequestMessage.Body}");
        sb.AppendLine($"Response StatusCode: {logEntry.ResponseMessage.StatusCode}");
        sb.AppendLine($"Response Body: {logEntry.ResponseMessage.BodyData?.BodyAsString}");

        return sb.ToString();
    }
}
