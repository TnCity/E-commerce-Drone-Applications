using Microsoft.EntityFrameworkCore;
using WINGS.DAL.Connection;
using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.Repository.Repositories
{
    public class ContactMessageRepository : IContactMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactMessageRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // ADD MESSAGE
        // ==========================================

        public async Task AddAsync(
            ContactMessage contactMessage)
        {
            await _context.ContactMessages
                .AddAsync(contactMessage);

            await _context.SaveChangesAsync();
        }


        // ==========================================
        // GET ALL MESSAGES
        // ==========================================

        public async Task<List<ContactMessage>> GetAllAsync()
        {
            return await _context.ContactMessages
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }


        // ==========================================
        // GET MESSAGE BY ID
        // ==========================================

        public async Task<ContactMessage?> GetByIdAsync(
            int id)
        {
            return await _context.ContactMessages
                .FirstOrDefaultAsync(x =>
                    x.ContactMessageId == id);
        }


        // ==========================================
        // MARK AS READ
        // ==========================================

        public async Task MarkAsReadAsync(int id)
        {
            var message =
                await _context.ContactMessages
                    .FirstOrDefaultAsync(x =>
                        x.ContactMessageId == id);

            if (message == null)
            {
                return;
            }

            message.IsRead = true;

            await _context.SaveChangesAsync();
        }
    }
}