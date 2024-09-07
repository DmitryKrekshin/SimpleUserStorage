using Microsoft.EntityFrameworkCore;
using UserStorage.Domain;
using UserStorage.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cacheConnection = builder.Configuration.GetConnectionString("Cache");
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = cacheConnection);

var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connectionString, b => b.MigrationsAssembly("UserStorage.WebApi")));
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigration();
}

app.UseHttpsRedirection();

using var scope = app.Services.CreateScope();
var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

app.MapGet("/Users", async () => await userRepository.Get());

app.MapPost("/Users", async (string name) =>
{
    var addedUser = await userRepository.Add(name);

    if (addedUser == null)
    {
        return Results.Conflict();
    }

    return Results.Ok(addedUser);
});

app.MapPut("/Users", async (int id, string name) =>
{
    var updateUser = new UserEntity
    {
        Id = id,
        Name = name
    };
    await userRepository.Update(updateUser);
});

app.MapDelete("/Users", async (int id) =>
{
    await userRepository.Delete(id);
});

app.Run();