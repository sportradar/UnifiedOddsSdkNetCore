// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using System.Threading.Tasks;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Logging;

public static class XUnitLoggerExtensions
{
    public static async Task WaitUntilMessageArriveContaining(this XUnitLogger logger, string messagePart)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => logger.Messages.Any(a => a.Contains(messagePart)));
    }

    public static async Task WaitUntilMessageArriveContaining(this XUnitLogger logger, string messagePart, int maxWaitTimeInSec)
    {
        await TestExecutionHelper.WaitToCompleteAsync(() => logger.Messages.Any(a => a.Contains(messagePart)), 10, maxWaitTimeInSec * 1000);
    }
}
