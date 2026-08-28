using BccSafety.Api.Endpoints;
using BccSafety.Api.Security;
using BccSafety.Api.Tenancy;
using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:Default is not configured. Set it via user-secrets " +
        "locally or the secret store in deployment — never hard-code it.");

builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
builder.Services.AddScoped<TenantConnectionInterceptor>();

builder.Services.AddDbContext<BccSafetyDbContext>((sp, options) =>
    options.UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()
        .AddInterceptors(sp.GetRequiredService<TenantConnectionInterceptor>()));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ActionTokenService>();
builder.Services.AddSingleton<IEmailSender, LoggingEmailSender>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "bccsafety_session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(90);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    // A six-digit login code has limited entropy; short expiry plus this
    // limiter is what keeps online guessing impractical.
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 10;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Only Caddy can reach this service — docker-compose exposes no ports
    // on the api container besides the internal network — so it's safe to
    // trust the forwarded headers from any address on that network.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapShiftActionEndpoints();

app.Run();
