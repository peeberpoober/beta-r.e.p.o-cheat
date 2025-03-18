using System.Diagnostics;

namespace dark_cheat
{
    static class DLog
    {
        [Conditional("DEBUG")]
        public static void Log(string message)
        {

            UnityEngine.Debug.Log(message);
        }

        [Conditional("DEBUG")]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional("DEBUG")]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }
    }
}