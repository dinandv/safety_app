using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BccSafety.Infrastructure.Tenancy;

/// <summary>
/// The tenant of the current request. Populated by middleware based on
/// the subdomain or the logged-in user, never based on anything the
/// client sends itself.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
}

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public void Set(Guid tenantId)
    {
        if (TenantId is not null && TenantId != tenantId)
            throw new InvalidOperationException(
                "A request's tenant must not change halfway through.");
        TenantId = tenantId;
    }
}

/// <summary>
/// Sets app.tenant_id on every physical connection that comes out of the
/// pool, and clears it again on return.
///
/// Why not SET LOCAL: that requires an explicit transaction, and EF
/// doesn't run read queries in one. set_config with is_local = false sets
/// the value at session level; Npgsql sends DISCARD ALL by default on
/// return, but we don't rely on that — we clear it ourselves.
///
/// Why this is safe if it does go wrong: app.current_tenant() returns
/// NULL for an empty value, and every policy compares against that. A
/// connection without a tenant sees nothing instead of everything.
/// </summary>
public sealed class TenantConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenant;

    public TenantConnectionInterceptor(ITenantContext tenant) => _tenant = tenant;

    public override void ConnectionOpened(
        DbConnection connection, ConnectionEndEventData eventData)
        => Apply(connection, _tenant.TenantId);

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
        => await ApplyAsync(connection, _tenant.TenantId, cancellationToken);

    public override InterceptionResult ConnectionClosing(
        DbConnection connection, ConnectionEventData eventData,
        InterceptionResult result)
    {
        Apply(connection, null);
        return result;
    }

    public override async ValueTask<InterceptionResult> ConnectionClosingAsync(
        DbConnection connection, ConnectionEventData eventData,
        InterceptionResult result)
    {
        await ApplyAsync(connection, null, CancellationToken.None);
        return result;
    }

    private static void Apply(DbConnection connection, Guid? tenantId)
    {
        using var cmd = Build(connection, tenantId);
        cmd.ExecuteNonQuery();
    }

    private static async Task ApplyAsync(
        DbConnection connection, Guid? tenantId, CancellationToken ct)
    {
        await using var cmd = Build(connection, tenantId);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static DbCommand Build(DbConnection connection, Guid? tenantId)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT set_config('app.tenant_id', @tenant, false)";
        var p = cmd.CreateParameter();
        p.ParameterName = "tenant";
        p.Value = tenantId?.ToString() ?? string.Empty;
        cmd.Parameters.Add(p);
        return cmd;
    }
}
