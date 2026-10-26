using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Api.Http;
using TaskManager.Domain.Common;

namespace TaskManager.Api.UnitTests.Http;

public sealed class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task GlobalExceptionHandler_ForDomainException_Returns400ProblemDetails()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var handled = await _handler.TryHandleAsync(context, new DomainException("Broken rule."), CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        var body = await ReadBodyAsync(context);
        body.GetProperty("status").GetInt32().ShouldBe(400);
        body.GetProperty("detail").GetString().ShouldBe("Broken rule.");
    }

    [Fact]
    public async Task GlobalExceptionHandler_ForUnexpectedException_Returns500WithoutStackTrace()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var handled = await _handler.TryHandleAsync(context, new InvalidOperationException("secret detail"), CancellationToken.None);

        // Assert
        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        var text = JsonSerializer.Serialize(await ReadBodyAsync(context));
        text.ShouldNotContain("secret detail");
        text.ShouldNotContain("InvalidOperationException");
        text.ShouldNotContain(" at ");
    }

    private static DefaultHttpContext CreateContext() => new() { Response = { Body = new MemoryStream() } };

    private static async Task<JsonElement> ReadBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonSerializer.DeserializeAsync<JsonElement>(context.Response.Body);
    }
}
