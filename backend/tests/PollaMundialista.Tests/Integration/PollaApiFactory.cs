using Microsoft.AspNetCore.Mvc.Testing;

namespace PollaMundialista.Tests.Integration;

/// <summary>
/// Boots the real API in-process over a throwaway SQLite database (schema +
/// seed created at startup), so integration tests exercise the full pipeline.
///
/// Config is set via environment variables (not ConfigureAppConfiguration) so it
/// also reaches the values Program reads eagerly at startup (Jwt secret, DB provider),
/// keeping token signing and validation in sync.
/// </summary>
public class PollaApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"polla-it-{Guid.NewGuid():N}.db");

    public PollaApiFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("Database__Provider", "Sqlite");
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", $"Data Source={_dbPath}");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "PollaTest");
        Environment.SetEnvironmentVariable("Jwt__Audience", "PollaTest.Client");
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-signing-key-please-change-0123456789");
        Environment.SetEnvironmentVariable("Jwt__ExpiryMinutes", "60");
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        foreach (var f in new[] { _dbPath, _dbPath + "-shm", _dbPath + "-wal" })
        {
            try { if (File.Exists(f)) File.Delete(f); } catch { /* best effort */ }
        }
    }
}
