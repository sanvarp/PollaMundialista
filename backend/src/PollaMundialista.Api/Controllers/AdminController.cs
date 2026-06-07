using FluentValidation;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PollaMundialista.Api.Common;
using PollaMundialista.Application.Admin;
using PollaMundialista.Application.Common;
using PollaMundialista.Application.Matches;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;
    private readonly IValidator<SetResultRequest> _validator;

    public AdminController(IAdminService admin, IValidator<SetResultRequest> validator)
    {
        _admin = admin;
        _validator = validator;
    }

    /// <summary>Stores a match result and recalculates points for all its predictions.</summary>
    [HttpPut("matches/{id:int}/result")]
    [ProducesResponseType(typeof(MatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetResult(int id, SetResultRequest request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToErrorDictionary()));

        var result = await _admin.SetResultAsync(User.GetUserId(), id, request, ct);
        return result.Succeeded
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }

    /// <summary>Reverts a match to "scheduled", clearing its result and points (undo).</summary>
    [HttpDelete("matches/{id:int}/result")]
    [ProducesResponseType(typeof(MatchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearResult(int id, CancellationToken ct)
    {
        var result = await _admin.ClearResultAsync(User.GetUserId(), id, ct);
        return result.Succeeded
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}