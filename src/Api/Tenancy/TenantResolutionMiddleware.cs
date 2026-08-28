using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Tenancy;

/// <summary>
/// Determines the tenant from the subdomain and sets it on the scoped
/// TenantContext, before any other middleware or endpoint touches the
/// database. Never from a header the client can set itself — Caddy sets
/// X-Forwarded-Host based on which site block already matched, that's the
/// only trusted signal. See deploy/Caddyfile.
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, BccSafetyDbContext db, TenantContext tenantContext)
    {
        var host = context.Request.Headers["X-Forwarded-Host"].FirstOrDefault()
            ?? context.Request.Host.Host;
        var slug = host.Split('.')[0];

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Slug == slug && t.Active)
            .Select(t => new { t.Id })
            .FirstOrDefaultAsync(context.RequestAborted);

        if (tenant is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        tenantContext.Set(tenant.Id);
        await _next(context);
    }
}
