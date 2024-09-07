using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using UserStorage.Repository;
using UserStorage.Repository.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<Context>(options => options.UseSqlServer(connectionString));

var cacheConnection = builder.Configuration.GetConnectionString("Cache");
builder.Services.AddStackExchangeRedisCache(options => options.Configuration = cacheConnection);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.ApplyMigration();
}

app.UseHttpsRedirection();

const string CACHE_KEY = "asdfhskadgfsdlafj234rdf";

async Task<List<UserEntity>> GetUsers()
{
    using var scope = app.Services.CreateScope();
    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    var usersString = await cache.GetStringAsync(CACHE_KEY);
    
    var users = new List<UserEntity>();
    if (usersString != null) users = JsonSerializer.Deserialize<List<UserEntity>>(usersString);
    
    if (users != null && users.Any())
    {
        return users;
    }
    
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    users = context.Users.ToList();
    await cache.SetStringAsync(CACHE_KEY, JsonSerializer.Serialize(users), new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    });
    
    return users;
}

app.MapGet("/Users", async () => await GetUsers());

app.MapPost("/Users", async (string name) =>
{
    var userToAdd = new UserEntity { Name = name };

    var users = await GetUsers();

    if (users.Any(a => a.Name == name))
    {
        return Results.Conflict();
    }
    
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    context.Users.Add(userToAdd);
    context.SaveChanges();

    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    await cache.RemoveAsync(CACHE_KEY);
    
    return Results.Ok(userToAdd);
});

app.MapPut("/Users", async (int id, string name) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    var user = context.Users.FirstOrDefault(user => user.Id == id);
    if (user != null)
    {
        user.Name = name;
        context.SaveChanges();
    }
    
    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    await cache.RemoveAsync(CACHE_KEY);
});

app.MapDelete("/Users", async (int id) =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<Context>();
    var user = context.Users.FirstOrDefault(user => user.Id == id);
    if (user != null)
    {
        context.Users.Remove(user);
        context.SaveChanges();
    }
    
    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    await cache.RemoveAsync(CACHE_KEY);
});

app.Run();