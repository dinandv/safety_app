using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BccSafety.Infrastructure.Tenancy;

/// <summary>
/// De tenant van de huidige request. Wordt gevuld door middleware op basis
/// van het subdomein of de ingelogde gebruiker, nooit op basis van iets dat
/// de client zelf meestuurt.
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
                "De tenant van een request mag niet halverwege wijzigen.");
        TenantId = tenantId;
    }
}

/// <summary>
/// Zet app.tenant_id op elke fysieke verbinding die uit de pool komt, en
/// wist hem weer bij teruggave.
///
/// Waarom niet SET LOCAL: dat vereist een expliciete transactie, en EF
/// draait leesqueries daar niet in. set_config met is_local = false zet de
/// waarde op sessieniveau; Npgsql stuurt bij teruggave standaard DISCARD
/// ALL, maar daar leunen we niet op — we wissen zelf.
///
/// Waarom dit veilig is als het tóch misgaat: app.current_tenant() geeft
/// NULL bij een lege waarde, en elke policy vergelijkt daarmee. Een
/// verbinding zonder tenant ziet dus niets in plaats van alles.
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
