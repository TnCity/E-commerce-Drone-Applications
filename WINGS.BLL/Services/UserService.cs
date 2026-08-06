using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.BLL.Services
{
    public class UserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task RegisterAsync(User user)
        {
            await _repository.RegisterAsync(user);
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            return await _repository.LoginAsync(email, password);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _repository.EmailExistsAsync(email);
        }
    }
}