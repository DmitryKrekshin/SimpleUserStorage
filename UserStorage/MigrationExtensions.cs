using Microsoft.EntityFrameworkCore;
using UserStorage.Repository;

namespace UserStorage;

public static class MigrationExtensions
{
    public static void ApplyMigration(this IApplicationBuilder applicationBuilder)
    {
        using IServiceScope scope = applicationBuilder.ApplicationServices.CreateScope();

        using Context dbContext = scope.ServiceProvider.GetRequiredService<Context>();
        
        dbContext.Database.Migrate();
    }
}