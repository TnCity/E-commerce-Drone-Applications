using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.BLL.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public Task<int> CreateOrderAsync(Order order)
            => _repository.CreateOrderAsync(order);

        public Task<Order?> GetOrderByIdAsync(int orderId)
            => _repository.GetOrderByIdAsync(orderId);

        public Task<List<Order>> GetOrdersByUserAsync(int userId)
            => _repository.GetOrdersByUserAsync(userId);
    }
}