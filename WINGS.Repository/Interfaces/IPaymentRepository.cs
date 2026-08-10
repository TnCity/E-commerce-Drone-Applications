using WINGS.DAL.Entities;

namespace WINGS.Repository.Interfaces
{
    public interface IPaymentRepository
    {
        Task<int> AddPaymentAsync(Payment payment);

        Task<Payment?> GetPaymentByIdAsync(int paymentId);

        Task<Payment?> GetPaymentByOrderIdAsync(int orderId);

        Task<List<Payment>> GetPaymentsByUserAsync(int userId);
    }
}