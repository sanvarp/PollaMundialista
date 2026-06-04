using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PollaMundialista.Api.Common;
using PollaMundialista.Application.Matches;
using PollaMundialista.Application.Predictions;

namespace PollaMundialista.Api.Controllers;

[ApiController]
[Route("api/predictions")]
[Authorize]
public class PredictionsController : ControllerBase
{
    private readonly IPredictionService _predictions;
    private readonly IValidator<UpsertPredictionRequest> _validator;

    public PredictionsController(IPredictionService predictions, IValidator<UpsertPredictionRequest> validator)
    {
        _predictions = predictions;
        _validator = validator;
    }

    /// <summary>The caller's own predictions.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(IReadOnlyList<PredictionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PredictionDto>>> GetMine(CancellationToken ct)
    {
        return Ok(await _predictions.GetMineAsync(User.GetUserId(), ct));
    }

    /// <summary>Creates or updates the caller's prediction for a match (rejected after kickoff).</summary>
    [HttpPut("{matchId:int}")]
    [ProducesResponseType(typeof(PredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Upsert(int matchId, UpsertPredictionRequest request, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return ValidationProblem(new ValidationProblemDetails(validation.ToErrorDictionary()));

        var result = await _predictions.UpsertAsync(User.GetUserId(), matchId, request, ct);
        return result.Succeeded
            ? Ok(result.Value)
            : Problem(detail: result.Error, statusCode: result.StatusCode);
    }
}
