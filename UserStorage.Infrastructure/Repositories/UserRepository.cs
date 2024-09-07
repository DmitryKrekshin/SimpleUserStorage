using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using UserStorage.Domain;

namespace UserStorage.Infrastructure;

public class UserRepository(Context context, IDistributedCache distributedCache) : IUserRepository
{
    private readonly string _cacheKey = $"{typeof(UserRepository)}HDSF432D4F6G2";

    public async Task<IEnumerable<UserEntity>> Get()
    {
        var usersString = await distributedCache.GetStringAsync(_cacheKey);

        var users = new List<UserEntity>();
        if (usersString != null)
        {
            users = JsonSerializer.Deserialize<List<UserEntity>>(usersString);
        }

        if (users != null && users.Any())
        {
            return users;
        }

        users = context.Users.ToList();
        await distributedCache.SetStringAsync(_cacheKey, JsonSerializer.Serialize(users), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        });

        return users;
    }

    public async Task<UserEntity?> Add(string name)
    {
        var users = context.Users.FirstOrDefault(f => f.Name == name);

        if (users != null)
        {
            return null;
        }

        var userToAdd = new UserEntity
        {
            Name = name
        };
        
        context.Users.Add(userToAdd);
        await context.SaveChangesAsync();
        await distributedCache.RemoveAsync(_cacheKey);

        return userToAdd;
    }

    public async Task Update(UserEntity userEntity)
    {
        var userToUpdate = await context.Users.FindAsync(userEntity.Id);

        if (userToUpdate != null)
        {
            userToUpdate = userEntity;
            await context.SaveChangesAsync();
            await distributedCache.RemoveAsync(_cacheKey);
        }
    }

    public async Task Delete(int id)
    {
        var userToDelete = await context.Users.FindAsync(id);

        if (userToDelete != null)
        {
            context.Users.Remove(userToDelete);
            await context.SaveChangesAsync();
            await distributedCache.RemoveAsync(_cacheKey);
        }
    }
}