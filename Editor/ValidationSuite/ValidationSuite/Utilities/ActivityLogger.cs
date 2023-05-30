using UnityEngine;

/// <summary>
/// Logger used to keep track of Validation Suite activity, for debugging
/// </summary>
class ActivityLogger
{
    public static void Log(string message, params object[] args)
    {
        Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
            "[Asset Store Validation] " + message, args);
    }
}
