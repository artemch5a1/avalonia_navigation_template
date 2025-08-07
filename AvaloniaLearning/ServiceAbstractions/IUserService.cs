using System.Collections.Generic;
using System.Threading.Tasks;
using AvaloniaApp.Models;

namespace AvaloniaApp.ServiceAbstractions
{
    public interface IUserService
    {
        Task<bool> CreateUser(User user);
        Task<bool> DeleteUser(int id);
        Task<List<User>> GetAllUsers();
        Task<User?> GetUserById(int id);
        Task<bool> UpdateUser(User user);
    }
}
