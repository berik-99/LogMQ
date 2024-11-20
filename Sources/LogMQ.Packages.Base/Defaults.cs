using System.Diagnostics;

namespace LogMQ;

public static class Defaults
{
	public const string DefaultCategory = "Generic";
	public static readonly string DefaultApplicationName = Process.GetCurrentProcess().ProcessName;
}
