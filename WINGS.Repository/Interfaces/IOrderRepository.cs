using WINGS.DAL.Entities;

namespace WINGS.Repository.Interfaces
{
    public interface IOrderRepository
    {
        Task<int> CreateOrderAsync(Order order);

        Task<Order?> GetOrderByIdAsync(int orderId);

        Task<List<Order>> GetOrdersByUserAsync(int userId);
    }
}