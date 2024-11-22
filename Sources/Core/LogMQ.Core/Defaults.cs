using System.Diagnostics;

namespace LogMQ.Core;

/// <summary>
/// Provides default values for commonly used application and logging settings in LogMQ.
/// </summary>
/// <remarks>
/// This class defines constants and read-only fields for fallback configurations,
/// such as the default application name and category.
/// </remarks>
public static class Defaults
{
    /// <summary>
    /// The default category name for log messages.
    /// </summary>
    /// <remarks>
    /// This value is used when no specific category is provided for a log entry.
    /// </remarks>
    public const string DefaultCategory = "Generic";

    /// <summary>
    /// The default application name derived from the current process name.
    /// </summary>
    /// <remarks>
    /// This value is determined dynamically at runtime based on the name of the executing process.
    /// </remarks>
    public static readonly string DefaultApplicationName = Process.GetCurrentProcess().ProcessName;
}
