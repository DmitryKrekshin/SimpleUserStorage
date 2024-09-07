using Microsoft.EntityFrameworkCore;
using UserStorage.Repository;

namespace UserStorage;

public static class MigrationExtensions
{
    public static void ApplyMigration(this IApplicationBuilder applicationBuilder)
    {
        using var scope = applicationBuilder.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<Context>();
        dbContext.Database.Migrate();
    }
}