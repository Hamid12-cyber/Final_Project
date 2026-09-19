using MotoHM.Api.Entites.Enums;

namespace MotoHM.Api.Entites;

public class UserEntity : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
}