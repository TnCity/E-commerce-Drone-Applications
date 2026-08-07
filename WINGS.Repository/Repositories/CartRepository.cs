using Microsoft.EntityFrameworkCore;
using WINGS.DAL.Connection;
using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;

namespace WINGS.Repository.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddToCartAsync(Cart cart)
        {
            var existing = await _context.Carts
                .FirstOrDefaultAsync(x =>
                    x.UserId == cart.UserId &&
                    x.ProductId == cart.ProductId);

            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                await _context.Carts.AddAsync(cart);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Cart>> GetCartByUserAsync(int userId)
        {
            return await _context.Carts
                .Include(x => x.Product)
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        public async Task RemoveCartAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);

            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
        public async Task IncreaseQuantityAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);

            if (cart != null)
            {
                cart.Quantity++;

                await _context.SaveChangesAsync();
            }
        }
        public async Task DecreaseQuantityAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);

            if (cart != null)
            {
                if (cart.Quantity > 1)
                {
                    cart.Quantity--;
                }
                else
                {
                    _context.Carts.Remove(cart);
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}