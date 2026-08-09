using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        public OrderController(
            OrderService orderService,
            CartService cartService)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        // =========================
        // CHECKOUT GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Please login before checkout.";

                return RedirectToAction("Login", "Account");
            }

            var cartItems =
                await _cartService.GetCartByUserAsync(userId.Value);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";

                return RedirectToAction("Cart", "Product");
            }

            decimal grandTotal = cartItems.Sum(x =>
                x.Product!.Price * x.Quantity);

            CheckoutViewModel model = new CheckoutViewModel
            {
                CartItems = cartItems,
                GrandTotal = grandTotal
            };

            return View(model);
        }


        // =========================
        // PLACE ORDER
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            CheckoutViewModel model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login before placing an order.";

                return RedirectToAction("Login", "Account");
            }

            var cartItems =
                await _cartService.GetCartByUserAsync(userId.Value);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";

                return RedirectToAction("Cart", "Product");
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = cartItems;

                model.GrandTotal = cartItems.Sum(x =>
                    x.Product!.Price * x.Quantity);

                return View(model);
            }

            decimal grandTotal = cartItems.Sum(x =>
                x.Product!.Price * x.Quantity);

            Order order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = grandTotal,
                ShippingAddress = model.ShippingAddress,
                Phone = model.Phone,
                Status = "Pending"
            };

            foreach (var cartItem in cartItems)
            {
                OrderItem orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Product!.Price,
                    TotalPrice =
                        cartItem.Product.Price * cartItem.Quantity
                };

                order.OrderItems.Add(orderItem);
            }

            int orderId =
                await _orderService.CreateOrderAsync(order);

            // Clear cart after successful order
            foreach (var cartItem in cartItems)
            {
                await _cartService.RemoveCartAsync(
                    cartItem.CartId);
            }

            return RedirectToAction(
                nameof(Confirmation),
                new { id = orderId });
        }


        // =========================
        // ORDER CONFIRMATION
        // =========================

        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order =
                await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Don't allow a user to view another user's order
            if (order.UserId != userId.Value)
            {
                return Forbid();
            }

            return View(order);
        }
        // =========================
        // MY ORDERS
        // =========================

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Please login to view your orders.";

                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService
                .GetOrdersByUserAsync(userId.Value);

            return View(orders);
        }


        // =========================
        // ORDER DETAILS
        // =========================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] = "Please login to view your order.";

                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Security check:
            // Customer can only view their own orders
            if (order.UserId != userId.Value)
            {
                return Forbid();
            }

            return View(order);
        }
    }
}