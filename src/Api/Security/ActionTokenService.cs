using BccSafety.Infrastructure.Data;
using BccSafety.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BccSafety.Api.Security;

/// <summary>
/// Issuing and verifying action_token rows: single use for login, short
/// lived and scoped to a single assignment for shift actions. The raw
/// token/code never enters the database, only its hash.
/// </summary>
public sealed class ActionTokenService
{
    private static readonly TimeSpan LoginValidity = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ShiftActionValidity = TimeSpan.FromHours(48);

    private readonly BccSafetyDbContext _db;
    private readonly TimeProvider _time;

    public ActionTokenService(BccSafetyDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    public async Task<string> IssueLoginCodeAsync(Guid personId, CancellationToken ct)
    {
        var code = ActionTokenHasher.GenerateLoginCode();
        _db.ActionTokens.Add(new ActionToken
        {
            PersonId = personId,
            Purpose = ActionTokenPurpose.Login,
            TokenHash = ActionTokenHasher.Hash(code),
            ValidUntil = _time.GetUtcNow() + LoginValidity,
        });
        await _db.SaveChangesAsync(ct);
        return code;
    }

    /// <summary>Single-use token, scoped to exactly this assignment.</summary>
    public async Task<string> IssueShiftActionTokenAsync(Guid personId, Guid assignmentId, CancellationToken ct)
    {
        var rawToken = ActionTokenHasher.GenerateOpaqueToken();
        _db.ActionTokens.Add(new ActionToken
        {
            PersonId = personId,
            Purpose = ActionTokenPurpose.ShiftAction,
            TokenHash = ActionTokenHasher.Hash(rawToken),
            ScopeId = assignmentId,
            ValidUntil = _time.GetUtcNow() + ShiftActionValidity,
        });
        await _db.SaveChangesAsync(ct);
        return rawToken;
    }

    /// <summary>Person is already known (via email): the code must belong to that person.</summary>
    public Task<ActionToken?> VerifyAndConsumeLoginCodeAsync(Guid personId, string code, CancellationToken ct)
        => ConsumeAsync(t => t.PersonId == personId
            && t.Purpose == ActionTokenPurpose.Login
            && t.TokenHash == ActionTokenHasher.Hash(code), ct);

    /// <summary>Action link: the token itself is the proof, the person follows from the match.</summary>
    public Task<ActionToken?> VerifyAndConsumeShiftActionAsync(string rawToken, CancellationToken ct)
        => ConsumeAsync(t => t.Purpose == ActionTokenPurpose.ShiftAction
            && t.TokenHash == ActionTokenHasher.Hash(rawToken), ct);

    private async Task<ActionToken?> ConsumeAsync(
        System.Linq.Expressions.Expression<Func<ActionToken, bool>> predicate, CancellationToken ct)
    {
        var now = _time.GetUtcNow();
        var token = await _db.ActionTokens
            .Where(predicate)
            .Where(t => t.UsedAt == null && t.ValidUntil > now)
            .FirstOrDefaultAsync(ct);

        if (token is null) return null;

        token.UsedAt = now;
        await _db.SaveChangesAsync(ct);
        return token;
    }
}
