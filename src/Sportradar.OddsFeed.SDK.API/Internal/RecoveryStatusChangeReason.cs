/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API.Internal
{
    public enum RecoveryStatusChangeReason
    {
        RECOVERY_STARTED,

        INITIAL_RECOVERY_COMPLETED,

        RECOVERY_FAILED,

        PROCESSING_DELAYED,

        ALIVE_INTERVAL_VIOLATION
    }
}
