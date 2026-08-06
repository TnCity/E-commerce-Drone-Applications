using Microsoft.EntityFrameworkCore;
using WINGS.DAL.Connection;
using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.Repository.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task RegisterAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x =>
                    x.Email == email &&
                    x.Password == password &&
                    x.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(x => x.Email == email);
        }
    }
}