using System.Text.Json.Serialization;
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

// Enums travel as their names, not as numbers. A client reading
// "Withdrawn" needs no lookup table, and inserting a new enum member
// cannot silently change what an old number means.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddSingleton(TimeProvider.System);

// One time zone for the whole installation. "Today" on the day overview
// and the date a certificate is judged against are both local dates, and
// getting them from the server's locale would make them depend on where
// the container happens to run. Per-tenant zones are a later concern:
// every tenant so far is in the same country.
builder.Services.AddSingleton(
    TimeZoneInfo.FindSystemTimeZoneById(
        builder.Configuration["App:TimeZone"] ?? "Europe/Amsterdam"));

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

// Before tenant resolution on purpose: the PWA shell is the same build
// for every tenant, so serving it should not cost a database round trip
// per asset.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRateLimiter();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapShiftActionEndpoints();
app.MapTodayEndpoints();
app.MapParticipantEndpoints();
app.MapOpenCallEndpoints();
app.MapInfoEndpoints();

// Deep links into the PWA are client-side routes, so anything that is
// not a file and not an endpoint gets the shell. Anything under /api
// that does not exist stays a 404 — an unknown endpoint answering with
// HTML makes client errors much harder to read.
app.MapFallback("/api/{*rest}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

app.Run();
