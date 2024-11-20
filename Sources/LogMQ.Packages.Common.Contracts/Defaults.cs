using System.Diagnostics;

namespace LogMQ.Contracts;

public static class Defaults
{
	public const string DefaultCategory = "Generic";
	public static readonly string DefaultApplicationName = Process.GetCurrentProcess().ProcessName;
}
