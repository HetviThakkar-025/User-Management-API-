using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public interface IUserService
{
    IEnumerable<User> GetAllUsers();
    User? GetUserById(int id);
    (User? User, string? Error) CreateUser(CreateUserDto dto);
    (User? User, string? Error) UpdateUser(int id, UpdateUserDto dto);
    bool DeleteUser(int id);
}
