using System;
using System.Runtime.InteropServices;

namespace BeitKnessetDisplay.Services
{
    public static class SleepPreventer
    {
        [FlagsAttribute]
        private enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        /// <summary>
        /// מונע מהמחשב להיכנס לשינה ומונע כיבוי מסך.
        /// קרא בתחילת האפליקציה.
        /// </summary>
        public static void PreventSleep()
        {
            SetThreadExecutionState(
                EXECUTION_STATE.ES_CONTINUOUS |
                EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }

        /// <summary>
        /// מאפשר חזרה להתנהגות רגילה (שינה/כיבוי מסך).
        /// קרא בסגירת האפליקציה.
        /// </summary>
        public static void AllowSleep()
        {
            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
        }
    }
}
