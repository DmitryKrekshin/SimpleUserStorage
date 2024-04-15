using System.ComponentModel.DataAnnotations;

namespace UserStorage.Repository.Entities;

public class UserEntity
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }
}