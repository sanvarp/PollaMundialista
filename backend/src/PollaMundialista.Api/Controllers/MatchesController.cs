using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Api.Common;
using PollaMundialista.Application.Matches;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/matches")]
[Authorize]
public class MatchesController : ControllerBase
{
    private readonly IMatchService _matches;

    public MatchesController(IMatchService matches) => _matches = matches;

    /// <summary>All group-stage matches with the caller's prediction and result (if played).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MatchDto>>> GetAll(CancellationToken ct)
    {
        var result = await _matches.GetAllAsync(User.GetUserId(), ct);
        return Ok(result);
    }
}
