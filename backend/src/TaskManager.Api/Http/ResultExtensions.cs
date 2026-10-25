using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;

namespace TaskManager.Api.Http;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result, Func<IActionResult>? onSuccess = null)
        => throw new NotImplementedException();

    public static IActionResult ToActionResult<T>(this Result<T> result, Func<T, IActionResult>? onSuccess = null)
        => throw new NotImplementedException();
}
