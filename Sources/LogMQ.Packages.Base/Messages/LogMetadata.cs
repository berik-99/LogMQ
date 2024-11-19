using ProtoBuf;
using System.Diagnostics;
namespace LogMQ.Messages;

/// <summary>
/// Contains metadata related to a log message, such as the source file and method details.
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
	/// Gets or sets the exception message, if any, that was thrown during the log event.
	/// </summary>
	[ProtoMember(6)]
	public string Exception { get; set; }

	public static LogMetadata GetMetadata(Exception ex = null)
	{
		var logMetadata = new LogMetadata();
		var frame = EnhancedStackTrace.Current().Take(128).Skip(3)
						  .FirstOrDefault(f => f.HasMethod() /*&& f.MethodInfo.DeclaringType?.Assembly != typeof(Log).Assembly*/);

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
		logMetadata.Exception = ex?.Message;
		return logMetadata;
	}
}
