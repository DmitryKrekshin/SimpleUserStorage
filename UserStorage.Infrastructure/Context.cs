using Microsoft.EntityFrameworkCore;
using UserStorage.Domain;
using UserStorage.Infrastructure.EntityConfigurations;

namespace UserStorage.Infrastructure;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public required DbSet<UserEntity> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
    }
}