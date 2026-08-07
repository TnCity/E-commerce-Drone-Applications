using WINGS.DAL.Entities;

namespace WINGS.Repository.Interfaces
{
    public interface ICartRepository
    {
        Task AddToCartAsync(Cart cart);
        Task<List<Cart>> GetCartByUserAsync(int userId);
        Task RemoveCartAsync(int cartId);
        Task IncreaseQuantityAsync(int cartId);

        Task DecreaseQuantityAsync(int cartId);
    }
}