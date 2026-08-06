using WINGS.DAL.Entities;

namespace WINGS.Repository.Interfaces
{
    public interface IUserRepository
    {
        Task RegisterAsync(User user);
        Task<User?> LoginAsync(string email, string password);
        Task<bool> EmailExistsAsync(string email);
    }
}