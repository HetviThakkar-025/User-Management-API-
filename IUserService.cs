using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public interface IUserService
{
    IEnumerable<User> GetAllUsers();
    User? GetUserById(int id);
    User CreateUser(CreateUserDto dto);
    User? UpdateUser(int id, UpdateUserDto dto);
    bool DeleteUser(int id);
}
