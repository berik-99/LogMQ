using LogMQ;
using Serilog.Events;
using System.Diagnostics;

namespace Serilog.Sinks.LogMQ.Extensions;

internal static class LogEventExtensions
{
    internal static LogMessage ToLogMessage(this LogEvent logEvent, IFormatProvider formatProvider, string applicationName) => new()
    {
        Timestamp = logEvent.Timestamp,
        LogLevel = logEvent.Level.ToLogMQLogLevel(),
        Message = logEvent.RenderMessage(formatProvider),
        Application = applicationName,
        Meta = logEvent.GetMetadata()
    };

    private static LogMetadata GetMetadata(this LogEvent logEvent)
    {
        var logMetadata = new LogMetadata();
        var frame = EnhancedStackTrace.Current().Take(128).Skip(3)
                          .FirstOrDefault(f => f.HasMethod() && f.MethodInfo.DeclaringType?.Assembly != typeof(Log).Assembly);

        if (frame == null) return logMetadata;

        var methodInfo = frame.MethodInfo;
        logMetadata.Class = methodInfo.DeclaringType?.FullName;
        logMetadata.MethodName = methodInfo.Name;
        logMetadata.MethodSignature = methodInfo.IsLambda ? "Lambda [" : "";
        logMetadata.MethodSignature += $"{(methodInfo.IsAsync ? "Async" : "")} {methodInfo.ReturnParameter?.ResolvedType?.FullName ?? "void"} {methodInfo.Name}";
        var parameters = methodInfo.Parameters;
        logMetadata.MethodSignature += parameters.Count > 0
            ? $"({string.Join(", ", parameters.Select(p => $"{p.ResolvedType.FullName} {p.Name}"))})"
            : "()";
        logMetadata.MethodSignature += methodInfo.IsLambda ? "]" : "";
        logMetadata.File = frame.GetFileName();
        logMetadata.Row = frame.GetFileLineNumber();
        logMetadata.Exception = (logEvent.Exception?.ToString());

        return logMetadata;
    }
}
