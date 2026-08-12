using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.BLL.Services
{
    public class ContactMessageService
    {
        private readonly IContactMessageRepository _repository;

        public ContactMessageService(
            IContactMessageRepository repository)
        {
            _repository = repository;
        }


        // ==========================================
        // ADD MESSAGE
        // ==========================================

        public async Task AddMessageAsync(
            ContactMessage contactMessage)
        {
            await _repository.AddAsync(contactMessage);
        }


        // ==========================================
        // GET ALL MESSAGES
        // ==========================================

        public async Task<List<ContactMessage>>
            GetAllMessagesAsync()
        {
            return await _repository.GetAllAsync();
        }


        // ==========================================
        // GET MESSAGE BY ID
        // ==========================================

        public async Task<ContactMessage?>
            GetMessageByIdAsync(int id)
        {
            return await _repository
                .GetByIdAsync(id);
        }


        // ==========================================
        // MARK AS READ
        // ==========================================

        public async Task MarkAsReadAsync(int id)
        {
            await _repository
                .MarkAsReadAsync(id);
        }
    }
}