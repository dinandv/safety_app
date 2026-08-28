using System.Security.Claims;
using BccSafety.Api.Security;
using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").RequireRateLimiting("auth");

        group.MapPost("/login/request", RequestAsync);
        group.MapPost("/login/confirm", ConfirmAsync);
        // Cast to Delegate: LogoutAsync's (HttpContext) -> Task<IResult> shape
        // would otherwise implicitly match the raw RequestDelegate overload
        // (Task<IResult> converts to Task), which discards the IResult and
        // never writes a response body.
        group.MapPost("/logout", (Delegate)LogoutAsync).RequireAuthorization();
    }

    private sealed record RequestBody(string Email);

    private static async Task<IResult> RequestAsync(
        RequestBody request,
        HttpContext http,
        BccSafetyDbContext db,
        ActionTokenService tokens,
        IEmailSender email,
        CancellationToken ct)
    {
        var person = await db.People
            .FirstOrDefaultAsync(p => p.Email == request.Email && p.Status == PersonStatus.Active, ct);

        // Always the same response, whether the email exists or not —
        // otherwise this endpoint can be used to guess who's registered.
        if (person is not null)
        {
            var code = await tokens.IssueLoginCodeAsync(person.Id, ct);

            // Code and link, both for the same 15 minutes. The link is
            // what most people use — it opens the PWA and signs them in
            // without typing anything — but a link is easy to break in
            // transit, so the code stays as the way that always works.
            //
            // The host comes from what Caddy matched, never from a
            // client-supplied header: the link must point at this
            // tenant's own subdomain and no other.
            var host = http.Request.Headers["X-Forwarded-Host"].FirstOrDefault()
                ?? http.Request.Host.Value;
            var link = $"https://{host}/login" +
                $"?email={Uri.EscapeDataString(person.Email)}&code={Uri.EscapeDataString(code)}";

            await email.SendAsync(
                person.Email,
                "Je inlogcode",
                $"Je code is {code}. Hij is een kwartier geldig en werkt één keer." +
                Environment.NewLine + Environment.NewLine +
                $"Of open meteen de app: {link}",
                ct);
        }

        return Results.Ok();
    }

    private sealed record ConfirmBody(string Email, string Code);

    private static async Task<IResult> ConfirmAsync(
        ConfirmBody request,
        HttpContext http,
        BccSafetyDbContext db,
        ActionTokenService tokens,
        CancellationToken ct)
    {
        var person = await db.People
            .FirstOrDefaultAsync(p => p.Email == request.Email && p.Status == PersonStatus.Active, ct);
        if (person is null) return Results.BadRequest("Invalid email/code combination.");

        var token = await tokens.VerifyAndConsumeLoginCodeAsync(person.Id, request.Code, ct);
        if (token is null) return Results.BadRequest("Invalid or expired code.");

        var roles = await db.PersonAppRoles
            .Where(pa => pa.PersonId == person.Id)
            .Select(pa => pa.AppRole)
            .ToListAsync(ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, person.Id.ToString()),
            new("tenant_id", person.TenantId.ToString()),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToString())));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });

        return Results.Ok(new { person.Id, person.FirstName, person.LastName, Roles = roles });
    }

    private static async Task<IResult> LogoutAsync(HttpContext http)
    {
        await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok();
    }
}
