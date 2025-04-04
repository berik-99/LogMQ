using System.Diagnostics;
using ProtoBuf;

namespace LogMQ.Core;

/// <summary>
/// Represents metadata associated with a log message, providing contextual information
/// such as the file name, method details, and exception details.
/// </summary>
[ProtoContract]
public class LogMetadata : ProtoSerializable<LogMetadata>
{
    /// <summary>
    /// Gets or sets the name of the source file where the log event occurred.
    /// </summary>
    [ProtoMember(1)]
    public string File { get; set; }

    /// <summary>
    /// Gets or sets the name of the class where the log event occurred.
    /// </summary>
    [ProtoMember(2)]
    public string Class { get; set; }

    /// <summary>
    /// Gets or sets the name of the method where the log event occurred.
    /// </summary>
    [ProtoMember(3)]
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets the full signature of the method where the log event occurred.
    /// </summary>
    [ProtoMember(4)]
    public string MethodSignature { get; set; }

    /// <summary>
    /// Gets or sets the line number in the source file where the log event occurred.
    /// </summary>
    [ProtoMember(5)]
    public int Row { get; set; }

    /// <summary>
    /// Generates metadata for the calling method by analyzing the stack trace.
    /// </summary>
    /// <param name="filterType">
    /// A <see cref="Type"/> used to filter out stack frames related to the specified assembly.
    /// Frames belonging to methods in the same assembly as <paramref name="filterType"/> are ignored.
    /// </param>
    /// <returns>
    /// A <see cref="LogMetadata"/> object containing information about the class name, method name,
    /// method signature, file path, and line number of the first relevant stack frame found.
    /// If no suitable frame is found, returns an empty <see cref="LogMetadata"/> instance.
    /// </returns>
    /// <remarks>
    /// This method inspects the current stack trace to extract metadata about the first method
    /// outside the specified <paramref name="filterType"/> assembly.
    /// - The stack trace is traversed up to a maximum depth of 128 frames, skipping the first 3 frames to avoid
    /// capturing details of logging infrastructure.
    /// - Only stack frames with valid method information are considered.
    /// - If no relevant stack frame is found, an empty <see cref="LogMetadata"/> object is returned.
    /// </remarks>
    public static LogMetadata GetLogMetadata(Type filterType)
    {
        LogMetadata logMetadata = new();
        IEnumerable<EnhancedStackFrame> stackTrace = EnhancedStackTrace.Current().Take(128).Skip(3);
        EnhancedStackFrame frame = stackTrace.FirstOrDefault(f => f.HasMethod() && f.MethodInfo.DeclaringType?.Assembly != filterType.Assembly);
        if (frame == null) return logMetadata;
        ResolvedMethod methodInfo = frame.MethodInfo;
        logMetadata.Class = methodInfo.DeclaringType?.FullName;
        logMetadata.MethodName = methodInfo.Name;
        logMetadata.MethodSignature = methodInfo.ToString();
        logMetadata.File = frame.GetFileName();
        logMetadata.Row = frame.GetFileLineNumber();
        return logMetadata;
    }
}
