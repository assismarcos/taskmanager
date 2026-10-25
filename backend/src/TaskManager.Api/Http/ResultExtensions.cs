using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;

namespace TaskManager.Api.Http;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, Func<IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess?.Invoke() ?? new NoContentResult();
        }

        return ToProblem(result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null)
    {
        if (result.IsSuccess)
        {
            return onSuccess is null ? new OkObjectResult(result.Value) : onSuccess(result.Value);
        }

        return ToProblem(result.Error);
    }

    private static ObjectResult ToProblem(Error error)
    {
        ProblemDetails problem = error.Type switch
        {
            ErrorType.Validation => new ValidationProblemDetails(ToErrors(error))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            },
            ErrorType.NotFound => Problem(StatusCodes.Status404NotFound, "Not Found", "https://tools.ietf.org/html/rfc9110#section-15.5.5"),
            ErrorType.Conflict => Problem(StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc9110#section-15.5.10"),
            ErrorType.Unauthorized => Problem(StatusCodes.Status401Unauthorized, "Unauthorized", "https://tools.ietf.org/html/rfc9110#section-15.5.2"),
            _ => throw new ArgumentOutOfRangeException(nameof(error), error.Type, "Unknown error type."),
        };
        problem.Detail = error.Message;

        return new ObjectResult(problem) { StatusCode = problem.Status };
    }

    private static Dictionary<string, string[]> ToErrors(Error error)
        => error.Details is null ? [] : new Dictionary<string, string[]>(error.Details);

    private static ProblemDetails Problem(int status, string title, string type)
        => new() { Status = status, Title = title, Type = type };
}
