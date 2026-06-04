using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Application.Leaderboard;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _leaderboard;

    public LeaderboardController(ILeaderboardService leaderboard) => _leaderboard = leaderboard;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<LeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LeaderboardEntryDto>>> Get(CancellationToken ct)
    {
        return Ok(await _leaderboard.GetAsync(ct));
    }
}
