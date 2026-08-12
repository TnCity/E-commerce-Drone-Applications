using WINGS.DAL.Entities;

namespace WINGS.Repository.Interfaces
{
    public interface IContactMessageRepository
    {
        Task AddAsync(ContactMessage contactMessage);

        Task<List<ContactMessage>> GetAllAsync();

        Task<ContactMessage?> GetByIdAsync(int id);

        Task MarkAsReadAsync(int id);
    }
}