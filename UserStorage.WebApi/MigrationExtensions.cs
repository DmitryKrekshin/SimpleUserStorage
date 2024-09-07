using Microsoft.EntityFrameworkCore;
using UserStorage.Infrastructure;

namespace UserStorage.Domain;

public static class MigrationExtensions
{
    public static void ApplyMigration(this IApplicationBuilder applicationBuilder)
    {
        using var scope = applicationBuilder.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
        dbContext.Database.Migrate();
    }
}