namespace LogMQ;

/// <summary>
/// Represents a log message with metadata and information about the event.
/// </summary>
public class LogMessage
{
	/// <summary>
	/// Gets or sets the timestamp of when the log event occurred.
	/// </summary>
	public DateTimeOffset Timestamp { get; set; }

	/// <summary>
	/// Gets or sets the severity level of the log message.
	/// </summary>
	public LogLevel LogLevel { get; set; }

	/// <summary>
	/// Gets or sets the actual log message describing the event or action.
	/// </summary>
	public string Message { get; set; }

	/// <summary>
	/// Gets or sets the name of the application generating the log message.
	/// </summary>
	public string Application { get; set; }

	/// <summary>
	/// Gets or sets additional metadata related to the log event, such as file, class, and method information.
	/// </summary>
	public LogMetadata Meta { get; set; }
}

/// <summary>
/// Contains metadata related to a log message, such as the source file and method details.
/// </summary>
public class LogMetadata
{
	/// <summary>
	/// Gets or sets the name of the source file where the log event occurred.
	/// </summary>
	public string File { get; set; }

	/// <summary>
	/// Gets or sets the name of the class where the log event occurred.
	/// </summary>
	public string Class { get; set; }

	/// <summary>
	/// Gets or sets the name of the method where the log event occurred.
	/// </summary>
	public string MethodName { get; set; }

	/// <summary>
	/// Gets or sets the full signature of the method where the log event occurred.
	/// </summary>
	public string MethodSignature { get; set; }

	/// <summary>
	/// Gets or sets the line number in the source file where the log event occurred.
	/// </summary>
	public int Row { get; set; }

	/// <summary>
	/// Gets or sets the exception message, if any, that was thrown during the log event.
	/// </summary>
	public string Exception { get; set; }
}
