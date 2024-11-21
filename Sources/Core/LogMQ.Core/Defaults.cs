using System.Diagnostics;

namespace LogMQ.Core;

public static class Defaults
{
	public const string DefaultCategory = "Generic";
	public static readonly string DefaultApplicationName = Process.GetCurrentProcess().ProcessName;
}
