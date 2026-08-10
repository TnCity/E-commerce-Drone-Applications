using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.BLL.Services
{
    public class PaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(
            IPaymentRepository repository)
        {
            _repository = repository;
        }

        public Task<int> AddPaymentAsync(Payment payment)
        {
            return _repository.AddPaymentAsync(payment);
        }

        public Task<Payment?> GetPaymentByIdAsync(
            int paymentId)
        {
            return _repository.GetPaymentByIdAsync(paymentId);
        }

        public Task<Payment?> GetPaymentByOrderIdAsync(
            int orderId)
        {
            return _repository.GetPaymentByOrderIdAsync(orderId);
        }

        public Task<List<Payment>> GetPaymentsByUserAsync(
            int userId)
        {
            return _repository.GetPaymentsByUserAsync(userId);
        }
    }
}