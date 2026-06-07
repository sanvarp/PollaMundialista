namespace PollaMundialista.Application.Common;

/// <summary>
/// Lightweight result wrapper so application services can signal expected
/// outcomes (conflicts, invalid credentials) without throwing for control flow.
/// The API layer maps <see cref="StatusCode"/> to an HTTP response / ProblemDetails.
/// </summary>
public class Result<T>
{
    public bool Succeeded { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }
    public int StatusCode { get; init; }

    public static Result<T> Ok(T value) =>
        new() { Succeeded = true, Value = value, StatusCode = 200 };

    public static Result<T> Fail(string error, int statusCode) =>
        new() { Succeeded = false, Error = error, StatusCode = statusCode };
}