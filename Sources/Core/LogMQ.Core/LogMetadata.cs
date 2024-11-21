using ProtoBuf;
using System.Diagnostics;

namespace LogMQ.Core;

/// <summary>
/// Represents metadata associated with a log message, providing contextual information
/// such as the file name, method details, and exception details.
/// </summary>
[ProtoContract]
public class LogMetadata
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
	/// Gets or sets the exception message, if an exception was thrown during the log event.
	/// </summary>
	[ProtoMember(6)]
	public string Exception { get; set; }

	/// <summary>
	/// Creates and populates an instance of <see cref="LogMetadata"/> by extracting information
	/// from the current stack trace and an optional exception.
	/// </summary>
	/// <param name="ex">The exception related to the log event, if any. Default is <c>null</c>.</param>
	/// <returns>A <see cref="LogMetadata"/> object containing extracted metadata.</returns>
	public static LogMetadata GetMetadata(Exception ex = null)
	{
		var logMetadata = new LogMetadata();

		// Extract the relevant frame from the enhanced stack trace.
		var frame = EnhancedStackTrace.Current().Take(128).Skip(3)
			.FirstOrDefault(f => f.HasMethod() /* && f.MethodInfo.DeclaringType?.Assembly != typeof(Log).Assembly */);

		if (frame == null) return logMetadata;

		var methodInfo = frame.MethodInfo;

		// Populate class and method details.
		logMetadata.Class = methodInfo.DeclaringType?.FullName;
		logMetadata.MethodName = methodInfo.Name;

		// Build the method signature.
		logMetadata.MethodSignature = methodInfo.IsLambda ? "Lambda [" : "";
		logMetadata.MethodSignature += $"{(methodInfo.IsAsync ? "Async" : "")} {methodInfo.ReturnParameter?.ResolvedType?.FullName ?? "void"} {methodInfo.Name}";
		var parameters = methodInfo.Parameters;
		logMetadata.MethodSignature += parameters.Count > 0
			? $"({string.Join(", ", parameters.Select(p => $"{p.ResolvedType.FullName} {p.Name}"))})"
			: "()";
		logMetadata.MethodSignature += methodInfo.IsLambda ? "]" : "";

		// Populate file and row details.
		logMetadata.File = frame.GetFileName();
		logMetadata.Row = frame.GetFileLineNumber();

		// Include exception details, if provided.
		logMetadata.Exception = ex?.Message;

		return logMetadata;
	}
}
