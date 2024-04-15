using Microsoft.EntityFrameworkCore;
using UserStorage;
using UserStorage.Repository;
using UserStorage.Repository.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<Context>(options => options.UseSqlServer(connectionString));

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
var context = scope.ServiceProvider.GetRequiredService<Context>();

app.MapGet("/Users", () => context.Users.ToList());

app.MapPost("/Users", (string name) =>
{
    context.Users.Add(new UserEntity { Name = name });
    context.SaveChanges();
});

app.MapPut("/Users", (int id, string name) =>
{
    var user = context.Users.FirstOrDefault(user => user.Id == id);
    if (user != null)
    {
        user.Name = name;
        context.SaveChanges();
    }
});

app.MapDelete("/Users", (int id) =>
{
    var user = context.Users.FirstOrDefault(user => user.Id == id);
    if (user != null)
    {
        context.Users.Remove(user);
        context.SaveChanges();
    }
});

app.Run();