using Microsoft.EntityFrameworkCore;
using UserStorage.Repository.Entities;

namespace UserStorage.Repository;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    internal DbSet<UserEntity> Users { get; set; } = null!;
}