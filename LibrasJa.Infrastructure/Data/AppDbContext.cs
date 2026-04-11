using LibrasJa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrasJa.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<InterpreterProfile> InterpreterProfiles => Set<InterpreterProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("USERS");
        modelBuilder.Entity<InterpreterProfile>().ToTable("INTERPRETER_PROFILES");
        modelBuilder.Entity<User>()
            .HasOne(u => u.InterpreterProfile)
            .WithOne(p => p.User)
            .HasForeignKey<InterpreterProfile>(p => p.UserId);
    }
}
