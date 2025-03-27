namespace LogMQ.Services.Shared.Common;

/// <summary>
/// Provides default paths and filenames used for plugin management in the LogMQ application.
/// </summary>
public static class Defaults
{
    /// <summary>
    /// The folder where all LogMQ data is stored.
    /// </summary>
    public static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LogMQ");

    //public const string GrpcAddress = "http://localhost:5000";
}