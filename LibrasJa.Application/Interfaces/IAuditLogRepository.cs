using LibrasJa.Domain.Entities;

namespace LibrasJa.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<IEnumerable<AuditLog>> GetAllAsync();
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entity);
}
