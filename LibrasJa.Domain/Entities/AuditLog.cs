namespace LibrasJa.Domain.Entities;

public class AuditLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string User { get; set; } = "anonymous";
    public string Payload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
