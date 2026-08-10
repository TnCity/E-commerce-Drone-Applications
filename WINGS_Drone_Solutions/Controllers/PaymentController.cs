using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentService _paymentService;
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        public PaymentController(
            PaymentService paymentService,
            OrderService orderService,
            CartService cartService)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _cartService = cartService;
        }


        // ==========================================
        // PAYMENT PAGE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login before making payment.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var cartItems =
                await _cartService
                    .GetCartByUserAsync(userId.Value);

            if (cartItems == null ||
                !cartItems.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Cart",
                    "Product");
            }

            decimal grandTotal =
                cartItems.Sum(x =>
                    x.Product!.Price * x.Quantity);

            var model = new PaymentViewModel
            {
                Amount = grandTotal
            };

            return View(model);
        }


        // ==========================================
        // PROCESS PAYMENT
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            PaymentViewModel model)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var cartItems =
                await _cartService
                    .GetCartByUserAsync(userId.Value);

            if (cartItems == null ||
                !cartItems.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Cart",
                    "Product");
            }

            decimal grandTotal =
                cartItems.Sum(x =>
                    x.Product!.Price * x.Quantity);

            model.Amount = grandTotal;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // ======================================
            // GET CHECKOUT INFORMATION
            // ======================================

            string? shippingAddress =
                HttpContext.Session.GetString(
                    "PendingShippingAddress");

            string? phone =
                HttpContext.Session.GetString(
                    "PendingPhone");


            if (string.IsNullOrEmpty(shippingAddress) ||
                string.IsNullOrEmpty(phone))
            {
                TempData["Error"] =
                    "Checkout information has expired. Please checkout again.";

                return RedirectToAction(
                    "Checkout",
                    "Order");
            }


            // ======================================
            // DEMO PAYMENT
            // ======================================

            // Later replace this section with
            // Razorpay / Stripe payment verification.

            bool paymentSuccessful = true;

            if (!paymentSuccessful)
            {
                TempData["Error"] =
                    "Payment failed.";

                return View(model);
            }


            // ======================================
            // CREATE ORDER
            // ======================================

            Order order = new Order
            {
                UserId = userId.Value,

                OrderDate = DateTime.Now,

                TotalAmount = grandTotal,

                ShippingAddress = shippingAddress,

                Phone = phone,

                Status = "Processing"
            };


            foreach (var cartItem in cartItems)
            {
                OrderItem orderItem = new OrderItem
                {
                    ProductId =
                        cartItem.ProductId,

                    Quantity =
                        cartItem.Quantity,

                    UnitPrice =
                        cartItem.Product!.Price,

                    TotalPrice =
                        cartItem.Product.Price *
                        cartItem.Quantity
                };

                order.OrderItems.Add(orderItem);
            }


            int orderId =
                await _orderService
                    .CreateOrderAsync(order);


            // ======================================
            // CREATE PAYMENT RECORD
            // ======================================

            string transactionId =
                "TXN-" +
                Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 12)
                    .ToUpper();


            Payment payment = new Payment
            {
                OrderId = orderId,

                UserId = userId.Value,

                Amount = grandTotal,

                PaymentMethod =
                    model.PaymentMethod,

                TransactionId =
                    transactionId,

                PaymentDate =
                    DateTime.Now,

                Status = "Success"
            };


            int paymentId =
                await _paymentService
                    .AddPaymentAsync(payment);


            // ======================================
            // CLEAR CART
            // ======================================

            foreach (var cartItem in cartItems)
            {
                await _cartService
                    .RemoveCartAsync(
                        cartItem.CartId);
            }


            // ======================================
            // CLEAR CHECKOUT SESSION
            // ======================================

            HttpContext.Session.Remove(
                "PendingShippingAddress");

            HttpContext.Session.Remove(
                "PendingPhone");


            // ======================================
            // PAYMENT SUCCESS
            // ======================================

            return RedirectToAction(
                nameof(Success),
                new
                {
                    paymentId = paymentId
                });
        }


        // ==========================================
        // PAYMENT SUCCESS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Success(
            int paymentId)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var payment =
                await _paymentService
                    .GetPaymentByIdAsync(paymentId);

            if (payment == null)
            {
                return NotFound();
            }

            // Security check

            if (payment.UserId != userId.Value)
            {
                return Forbid();
            }

            return View(payment);
        }
    }
}