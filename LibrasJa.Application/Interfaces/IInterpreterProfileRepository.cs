using LibrasJa.Domain.Entities;

namespace LibrasJa.Application.Interfaces;

public interface IInterpreterProfileRepository
{
    Task<List<InterpreterProfile>> GetAllAsync();
    Task<InterpreterProfile?> GetByIdAsync(int id);
    Task AddAsync(InterpreterProfile profile);
    Task UpdateAsync(InterpreterProfile profile);
    Task DeleteAsync(int id);
}
