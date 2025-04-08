using Dapper;
using DuckDB.NET.Data;

namespace LogMQ.Services.Broker.Worker.Services;

internal class RepositoryConfiguration
{
    public required string DatabaseFolderPath { init; get; }

    public async Task<Guid> GetApplicationId(string appName, bool throwIfNull = true)
    {
        await using DuckDBConnection conn = new(BuildDataSourceString(Guid.Empty));
        await conn.OpenAsync();
        Guid res = await conn.ExecuteScalarAsync<Guid>("SELECT Guid FROM Applications WHERE Name = $appName", new { appName });
        if (throwIfNull && res == Guid.Empty)
            throw new InvalidOperationException($"Application '{appName}' not found");
        return res;
    }

    public string BuildDataSourceString(Guid appId, bool pathOnly = false) => $"{(pathOnly ? "" : "Data Source=")}{Path.Join(DatabaseFolderPath, appId.ToString("N").ToUpperInvariant())}";
}
