using WINGS.DAL.Entities;
using WINGS.Repository.Interfaces;
using WINGS.Repository.Repositories;

namespace WINGS.BLL.Services
{
    public class CartService
    {
        private readonly ICartRepository _repository;

        public CartService(ICartRepository repository)
        {
            _repository = repository;
            
        }

        public Task AddToCartAsync(Cart cart)
            => _repository.AddToCartAsync(cart);

        public Task<List<Cart>> GetCartByUserAsync(int userId)
            => _repository.GetCartByUserAsync(userId);

        public Task RemoveCartAsync(int cartId)
            => _repository.RemoveCartAsync(cartId);
        public Task IncreaseQuantityAsync(int cartId)
            => _repository.IncreaseQuantityAsync(cartId);

        public Task DecreaseQuantityAsync(int cartId)
            => _repository.DecreaseQuantityAsync(cartId);
    }
}