/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API.Internal
{
    internal enum RecoveryStatusChangeReason
    {
        RecoveryStarted,

        InitialRecoveryCompleted,

        RecoveryFailed,

        ProcessingDelayed,

        AliveIntervalViolation
    }
}
