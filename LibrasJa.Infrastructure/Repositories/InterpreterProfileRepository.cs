using LibrasJa.Application.Interfaces;
using LibrasJa.Domain.Entities;
using LibrasJa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrasJa.Infrastructure.Repositories;

public class InterpreterProfileRepository : IInterpreterProfileRepository
{
    private readonly AppDbContext _context;
    public InterpreterProfileRepository(AppDbContext context) => _context = context;

    public async Task<List<InterpreterProfile>> GetAllAsync() =>
        await _context.InterpreterProfiles.Include(p => p.User).ToListAsync();

    public async Task<InterpreterProfile?> GetByIdAsync(int id) =>
        await _context.InterpreterProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(InterpreterProfile profile) { _context.InterpreterProfiles.Add(profile); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(InterpreterProfile profile) { _context.InterpreterProfiles.Update(profile); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(int id)
    {
        var p = await _context.InterpreterProfiles.FindAsync(id);
        if (p != null) { _context.InterpreterProfiles.Remove(p); await _context.SaveChangesAsync(); }
    }
}
