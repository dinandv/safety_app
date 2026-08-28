using System.Security.Cryptography;
using System.Text;

namespace BccSafety.Api.Security;

/// <summary>
/// Pure hashing/generation logic, separate from the database, so it can
/// be unit tested without a running Postgres.
/// </summary>
public static class ActionTokenHasher
{
    /// <summary>Six-digit login code sent by email: also embedded as a query param in the magic link.</summary>
    public static string GenerateLoginCode()
    {
        var number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return number.ToString("D6");
    }

    /// <summary>Long, high-entropy token for shift actions (confirm/withdraw/swap).</summary>
    public static string GenerateOpaqueToken()
    {
        Span<byte> buffer = stackalloc byte[32];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>Never store the raw token/code — only this hash.</summary>
    public static string Hash(string rawValue)
    {
        var bytes = Encoding.UTF8.GetBytes(rawValue);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
