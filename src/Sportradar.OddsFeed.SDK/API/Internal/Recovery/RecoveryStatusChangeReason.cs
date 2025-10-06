// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

namespace Sportradar.OddsFeed.SDK.Api.Internal.Recovery
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
