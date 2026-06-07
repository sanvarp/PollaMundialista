using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using Xunit;

namespace PollaMundialista.Tests.Integration;

/// <summary>End-to-end API flow over the real pipeline (auth → predict → admin → leaderboard).</summary>
public class ApiIntegrationTests : IClassFixture<PollaApiFactory>
{
    private readonly PollaApiFactory _factory;

    public ApiIntegrationTests(PollaApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AuthedClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Register_then_login_succeeds()
    {
        var client = _factory.CreateClient();
        var email = $"it-{Guid.NewGuid():N}@polla.com";

        var register = await client.PostAsJsonAsync("/api/auth/register",
            new { email, password = "Test#2026", displayName = "Integration User" });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Test#2026" });
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("User", body.GetProperty("role").GetString());
        Assert.False(string.IsNullOrEmpty(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task Matches_requires_authentication()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/matches");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_is_anonymous_and_healthy()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_check_validates_the_database()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health/ready");
        resp.EnsureSuccessStatusCode();
        Assert.Equal("Healthy", await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Responses_carry_security_and_correlation_headers()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        Assert.Equal("nosniff", resp.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.True(resp.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task NonAdmin_cannot_set_result()
    {
        var user = await AuthedClientAsync("user@polla.com", "User#2026");
        var resp = await user.PutAsJsonAsync("/api/admin/matches/3/result", new { homeGoals = 1, awayGoals = 0 });
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Predict_then_admin_result_awards_points()
    {
        var user = await AuthedClientAsync("user@polla.com", "User#2026");

        // Find an open (not locked) match.
        var matches = await (await user.GetAsync("/api/matches")).Content.ReadFromJsonAsync<JsonElement>();
        var open = matches.EnumerateArray().First(m => !m.GetProperty("isLocked").GetBoolean());
        var matchId = open.GetProperty("id").GetInt32();

        // Submit an exact-score prediction.
        var put = await user.PutAsJsonAsync($"/api/predictions/{matchId}", new { homeGoals = 2, awayGoals = 2 });
        put.EnsureSuccessStatusCode();

        // Admin enters that exact result.
        var admin = await AuthedClientAsync("admin@polla.com", "Admin#2026");
        var setResult = await admin.PutAsJsonAsync($"/api/admin/matches/{matchId}/result", new { homeGoals = 2, awayGoals = 2 });
        setResult.EnsureSuccessStatusCode();

        // The user's prediction for that match now scores 3 (exact).
        var mine = await (await user.GetAsync("/api/predictions/me")).Content.ReadFromJsonAsync<JsonElement>();
        var scored = mine.EnumerateArray().First(p => p.GetProperty("matchId").GetInt32() == matchId);
        Assert.Equal(3, scored.GetProperty("pointsAwarded").GetInt32());

        // Leaderboard is populated.
        var board = await (await user.GetAsync("/api/leaderboard")).Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(board.GetArrayLength() >= 5);
    }

    [Fact]
    public async Task Cannot_predict_after_kickoff_lock()
    {
        var user = await AuthedClientAsync("user@polla.com", "User#2026");
        var matches = await (await user.GetAsync("/api/matches")).Content.ReadFromJsonAsync<JsonElement>();
        var locked = matches.EnumerateArray().First(m => m.GetProperty("isLocked").GetBoolean());
        var matchId = locked.GetProperty("id").GetInt32();

        var resp = await user.PutAsJsonAsync($"/api/predictions/{matchId}", new { homeGoals = 1, awayGoals = 1 });
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }
}