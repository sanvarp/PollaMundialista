using FluentValidation.Results;

namespace PollaMundialista.Api.Common;

public static class ValidationExtensions
{
    /// <summary>Converts FluentValidation failures into a dictionary for ValidationProblem.</summary>
    public static IDictionary<string, string[]> ToErrorDictionary(this ValidationResult result) =>
        result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}