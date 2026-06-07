using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PollaMundialista.Application.Standings;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/standings")]
[Authorize]
public class StandingsController : ControllerBase
{
    private readonly IStandingsService _standings;

    public StandingsController(IStandingsService standings) => _standings = standings;

    /// <summary>Group-stage standings derived from finished match results.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<GroupStandingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<GroupStandingDto>>> Get(CancellationToken ct)
    {
        return Ok(await _standings.GetAsync(ct));
    }
}