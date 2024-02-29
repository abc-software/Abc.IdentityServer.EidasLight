using System;
using System.Diagnostics;

namespace System
{
    internal static class DateTimeExtensions
    {
        [DebuggerStepThrough]
        public static bool InFuture(this DateTime serverTime, DateTime now, TimeSpan clockScrew)
        {
            return now + clockScrew < serverTime;
        }

        [DebuggerStepThrough]
        public static bool InPast(this DateTime serverTime, DateTime now, TimeSpan clockScrew)
        {
            return now > serverTime + clockScrew;
        }
    }
}