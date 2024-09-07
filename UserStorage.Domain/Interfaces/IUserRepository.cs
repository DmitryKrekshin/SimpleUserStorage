namespace UserStorage.Domain;

public interface IUserRepository
{
    Task<IEnumerable<UserEntity>> Get();

    Task<UserEntity?> Add(string name);

    Task Update(UserEntity userEntity);

    Task Delete(int id);
}