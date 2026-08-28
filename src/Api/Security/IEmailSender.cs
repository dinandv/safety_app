namespace BccSafety.Api.Security;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct);
}

/// <summary>
/// Stand-in until there's a real SMTP integration. Sends nothing, only
/// logs. The owner sets the real credentials themselves via the secret
/// store; we don't create or guess them here.
/// </summary>
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken ct)
    {
        _logger.LogInformation("Email to {To}: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
