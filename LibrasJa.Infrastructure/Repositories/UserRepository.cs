using LibrasJa.Application.Interfaces;
using LibrasJa.Domain.Entities;
using LibrasJa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LibrasJa.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users.Include(u => u.InterpreterProfile).ToListAsync();

    public async Task<User?> GetByIdAsync(int id) =>
        await _context.Users.Include(u => u.InterpreterProfile).FirstOrDefaultAsync(u => u.Id == id);

    public async Task AddAsync(User user) { _context.Users.Add(user); await _context.SaveChangesAsync(); }
    public async Task UpdateAsync(User user) { _context.Users.Update(user); await _context.SaveChangesAsync(); }
    public async Task DeleteAsync(User user) { _context.Users.Remove(user); await _context.SaveChangesAsync(); }
}
