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

// Before tenant resolution on purpose: scripts, styles, fonts and icons
// are the same build for every tenant, so serving them should not cost a
// database round trip each.
//
// Navigations do not take this shortcut, and should not. "/" and "/today"
// match the fallback endpoint below, which selects an endpoint before
// this middleware runs and makes it stand aside — so they pass through
// tenant resolution and an unknown hostname gets a 404 instead of the
// shell. That is one lookup per app start, not per request.
//
// UseDefaultFiles is deliberately absent for the same reason: it would
// never see "/" either, and a line that looks like it maps "/" to
// index.html while the fallback actually does it is worse than no line.
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

// Top-level statements compile to an internal Program, which
// WebApplicationFactory cannot reach. This makes the entry point visible
// to the integration tests so they run the real pipeline — tenant
// resolution, authentication, row-level security and all — rather than a
// second pipeline assembled to look like it.
public partial class Program;
