using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Api.Common;
using PollaMundialista.Application.Users;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserHistoryService _history;

    public UsersController(IUserHistoryService history) => _history = history;

    /// <summary>A user's prediction history vs results (anti-cheat applied for non-owners).</summary>
    [HttpGet("{userId}/predictions")]
    [ProducesResponseType(typeof(UserHistoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPredictions(string userId, CancellationToken ct)
    {
        var result = await _history.GetAsync(userId, User.GetUserId(), ct);
        return result.Succeeded
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}
