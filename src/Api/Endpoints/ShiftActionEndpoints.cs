using BccSafety.Api.Security;
using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Entities;
using BccSafety.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Endpoints;

/// <summary>
/// Actions via the signed link from a notification, without logging in.
/// Each action here is exactly as powerful as the token that comes with
/// it: single use, short lived, scoped to one assignment.
///
/// "Swap" (handing a shift to one specific, chosen colleague) is
/// deliberately not here — that needs a candidate list (who's eligible
/// for this shift) and belongs to the swap flow in its own issue. These
/// action tokens are already ready for it: same mechanism, only the
/// endpoint is still missing.
/// </summary>
public static class ShiftActionEndpoints
{
    public static void MapShiftActionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/shifts/actions").RequireRateLimiting("auth");

        group.MapPost("/confirm", ConfirmAsync);
        group.MapPost("/withdraw", WithdrawAsync);
    }

    private sealed record ActionRequest(string Token);

    private static async Task<IResult> ConfirmAsync(
        ActionRequest request, BccSafetyDbContext db, ActionTokenService tokens,
        ITenantContext tenant, TimeProvider time, CancellationToken ct)
    {
        var token = await tokens.VerifyAndConsumeShiftActionAsync(request.Token, ct);
        if (token is null) return Results.BadRequest("Invalid or expired link.");

        var assignmentId = token.ScopeId!.Value;
        var exists = await db.Assignments.AnyAsync(a => a.Id == assignmentId, ct);
        if (!exists) return Results.BadRequest("This shift no longer exists.");

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenant.TenantId!.Value,
            ActorPersonId = token.PersonId,
            Entity = "assignment",
            EntityId = assignmentId,
            Action = "confirmed",
            Timestamp = time.GetUtcNow(),
        });
        await db.SaveChangesAsync(ct);

        return Results.Ok();
    }

    private sealed record WithdrawRequest(string Token, string? Reason);

    private static async Task<IResult> WithdrawAsync(
        WithdrawRequest request, BccSafetyDbContext db, ActionTokenService tokens,
        ITenantContext tenant, TimeProvider time, CancellationToken ct)
    {
        var token = await tokens.VerifyAndConsumeShiftActionAsync(request.Token, ct);
        if (token is null) return Results.BadRequest("Invalid or expired link.");

        var assignment = await db.Assignments
            .Include(a => a.Shift)
            .FirstOrDefaultAsync(a => a.Id == token.ScopeId!.Value, ct);
        if (assignment is null) return Results.BadRequest("This shift no longer exists.");

        var now = time.GetUtcNow();
        assignment.Status = AssignmentStatus.Withdrawn;
        assignment.WithdrawnAt = now;
        assignment.WithdrawalReason = request.Reason;

        // A withdrawal immediately opens an open call — otherwise the day
        // overview stops matching reality within a few weeks (see docs/ontwerp.md).
        db.SwapRequests.Add(new SwapRequest
        {
            ShiftId = assignment.ShiftId,
            AssignmentId = assignment.Id,
            RequestedByPersonId = token.PersonId,
            TargetPersonId = null,
            Kind = SwapRequestKind.OpenCall,
            Status = SwapRequestStatus.Open,
            ExpiresAt = assignment.Shift.Start,
        });

        db.AuditLogs.Add(new AuditLog
        {
            TenantId = tenant.TenantId!.Value,
            ActorPersonId = token.PersonId,
            Entity = "assignment",
            EntityId = assignment.Id,
            Action = "withdrawn",
            Timestamp = now,
        });

        await db.SaveChangesAsync(ct);
        return Results.Ok();
    }
}
