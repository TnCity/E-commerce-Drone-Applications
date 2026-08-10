using Microsoft.EntityFrameworkCore;
using WINGS.DAL.Connection;
using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.Repository.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return payment.PaymentId;
        }

        public async Task<Payment?> GetPaymentByIdAsync(
            int paymentId)
        {
            return await _context.Payments
                .Include(x => x.Order)
                .Include(x => x.User)
                .FirstOrDefaultAsync(
                    x => x.PaymentId == paymentId);
        }

        public async Task<Payment?> GetPaymentByOrderIdAsync(
            int orderId)
        {
            return await _context.Payments
                .Include(x => x.Order)
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId);
        }

        public async Task<List<Payment>> GetPaymentsByUserAsync(
            int userId)
        {
            return await _context.Payments
                .Include(x => x.Order)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();
        }
    }
}