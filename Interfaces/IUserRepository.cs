using Data_Organizer_Server.Models;

namespace Data_Organizer_Server.Interfaces
{
    public interface IUserRepository
    {
        Task<string> CreateUserAsync(User user);
        Task<User> GetUserByUidAsync(string uid);
        Task RemoveUserAsync(string uid);
        Task UpdateUserAsync(User user);
    }
}
