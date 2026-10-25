using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using TaskManager.Api.Authentication;
using TaskManager.Api.Http;
using TaskManager.Api.OpenApi;
using TaskManager.Application;
using TaskManager.Application.Abstractions;
using TaskManager.Infrastructure;
using TaskManager.Infrastructure.Authentication;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddApplication();
services.AddInfrastructure(builder.Configuration);

services.AddHttpContextAccessor();
services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwt) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.Value.CreateSigningKey(),
            ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "name",
        };
    });
services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

services.AddProblemDetails();
services.AddExceptionHandler<GlobalExceptionHandler>();

services.AddControllers().AddJsonOptions(options => JsonOptionsSetup.Configure(options.JsonSerializerOptions));
services.ConfigureHttpJsonOptions(options => JsonOptionsSetup.Configure(options.SerializerOptions));

services.AddOpenApi("auth-v1", options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
services.AddOpenApi("tasks-v1", options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

services.AddOptions<CorsOptions>().Configure<IConfiguration>((options, configuration) =>
{
    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddDefaultPolicy(policy => policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod());
});
services.AddCors();

services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// Behind the compose nginx every client shares the proxy address, so the login limiter needs X-Forwarded-For.
services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("10.0.0.0/8"));
    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("172.16.0.0/12"));
    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("192.168.0.0/16"));
});

services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = httpContext.RequestServices.GetRequiredService<IConfiguration>()
                .GetValue("RateLimiting:LoginPermitLimit", 5),
            Window = TimeSpan.FromMinutes(1),
        }));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync(CancellationToken.None);
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference(options => options.AddDocuments("auth-v1", "tasks-v1")).AllowAnonymous();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapControllers();

app.Run();

public partial class Program;
