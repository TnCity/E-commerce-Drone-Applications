using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using WINGS.BLL.Services;
using WINGS.DAL.Entities;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CartService _cartService;
        private readonly PaymentService _paymentService;

        public OrderController(
            OrderService orderService,
            CartService cartService,
            PaymentService paymentService)
        {
            _orderService = orderService;
            _cartService = cartService;
            _paymentService = paymentService;
        }


        // =========================================================
        // CHECKOUT GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login before checkout.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var cartItems =
                await _cartService.GetCartByUserAsync(userId.Value);

            if (cartItems == null || !cartItems.Any())
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

            CheckoutViewModel model =
                new CheckoutViewModel
                {
                    CartItems = cartItems,
                    GrandTotal = grandTotal
                };

            return View(model);
        }


        // =========================================================
        // CHECKOUT POST
        // PLACE ORDER → PAYMENT PAGE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(
            CheckoutViewModel model)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login before placing an order.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var cartItems =
                await _cartService
                    .GetCartByUserAsync(userId.Value);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Cart",
                    "Product");
            }

            // Validate checkout information

            if (!ModelState.IsValid)
            {
                model.CartItems = cartItems;

                model.GrandTotal =
                    cartItems.Sum(x =>
                        x.Product!.Price *
                        x.Quantity);

                return View(model);
            }

            // Calculate amount from database/cart
            // Never trust the amount coming from the browser

            decimal grandTotal =
                cartItems.Sum(x =>
                    x.Product!.Price *
                    x.Quantity);

            // =====================================================
            // SAVE SHIPPING INFORMATION IN SESSION
            // =====================================================

            HttpContext.Session.SetString(
                "PendingShippingAddress",
                model.ShippingAddress);

            HttpContext.Session.SetString(
                "PendingPhone",
                model.Phone);

            HttpContext.Session.SetString(
                "PendingAmount",
                grandTotal.ToString());

            // =====================================================
            // DO NOT CREATE ORDER HERE
            // PAYMENT MUST HAPPEN FIRST
            // =====================================================

            return RedirectToAction(
                nameof(Payment));
        }


        // =========================================================
        // PAYMENT PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Payment()
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

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Cart",
                    "Product");
            }

            // Check whether checkout information exists

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
                    "Please complete checkout information first.";

                return RedirectToAction(
                    nameof(Checkout));
            }

            decimal grandTotal =
                cartItems.Sum(x =>
                    x.Product!.Price *
                    x.Quantity);

            PaymentViewModel model =
                new PaymentViewModel
                {
                    Amount = grandTotal
                };

            return View(model);
        }


        // =========================================================
        // PAYMENT POST
        // CREATE STRIPE CHECKOUT SESSION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(
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

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Cart",
                    "Product");
            }

            // =====================================================
            // GET CHECKOUT INFORMATION
            // =====================================================

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
                    nameof(Checkout));
            }

            // =====================================================
            // CALCULATE TOTAL FROM DATABASE
            // =====================================================

            decimal grandTotal =
                cartItems.Sum(x =>
                    x.Product!.Price *
                    x.Quantity);

            model.Amount = grandTotal;

            // =====================================================
            // CREATE STRIPE LINE ITEMS
            // =====================================================

            var lineItems =
                new List<SessionLineItemOptions>();

            foreach (var cartItem in cartItems)
            {
                var product =
                    cartItem.Product!;

                var lineItem =
                    new SessionLineItemOptions
                    {
                        PriceData =
                            new SessionLineItemPriceDataOptions
                            {
                                Currency = "inr",

                                UnitAmount =
                                    (long)(
                                        product.Price * 100
                                    ),

                                ProductData =
                                    new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name =
                                            product.ProductName,

                                        Description =
                                            product.Description
                                    }
                            },

                        Quantity =
                            cartItem.Quantity
                    };

                lineItems.Add(lineItem);
            }

            // =====================================================
            // CREATE STRIPE CHECKOUT SESSION
            // =====================================================

            var options =
                new SessionCreateOptions
                {
                    Mode = "payment",

                    LineItems = lineItems,

                    SuccessUrl =
                        $"{Request.Scheme}://{Request.Host}" +
                        "/Order/StripeSuccess?session_id={CHECKOUT_SESSION_ID}",

                    CancelUrl =
                        $"{Request.Scheme}://{Request.Host}" +
                        "/Order/Payment",

                    CustomerEmail =
                        HttpContext.Session.GetString(
                            "UserEmail"),

                    Metadata =
                        new Dictionary<string, string>
                        {
                            {
                                "UserId",
                                userId.Value.ToString()
                            }
                        }
                };

            var service =
                new SessionService();

            Session session =
                await service.CreateAsync(options);

            // =====================================================
            // REDIRECT TO STRIPE
            // =====================================================

            return Redirect(session.Url);
        }


        // =========================================================
        // STRIPE SUCCESS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> StripeSuccess(
            string session_id)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            if (string.IsNullOrEmpty(session_id))
            {
                return BadRequest(
                    "Stripe session ID is missing.");
            }

            // =====================================================
            // GET STRIPE SESSION
            // =====================================================

            var service =
                new SessionService();

            var session =
                await service.GetAsync(session_id);

            // =====================================================
            // VERIFY PAYMENT
            // =====================================================

            if (session.PaymentStatus != "paid")
            {
                TempData["Error"] =
                    "Payment was not completed.";

                return RedirectToAction(
                    nameof(Payment));
            }

            // =====================================================
            // CHECK USER FROM STRIPE METADATA
            // =====================================================

            if (session.Metadata != null &&
                session.Metadata.TryGetValue(
                    "UserId",
                    out var stripeUserId))
            {
                if (stripeUserId != userId.Value.ToString())
                {
                    return Forbid();
                }
            }

            // =====================================================
            // GET CHECKOUT INFORMATION
            // =====================================================

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
                    "Checkout information has expired.";

                return RedirectToAction(
                    nameof(Payment));
            }

            // =====================================================
            // GET CART
            // =====================================================

            var cartItems =
                await _cartService
                    .GetCartByUserAsync(userId.Value);

            if (cartItems == null || !cartItems.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Cart",
                    "Product");
            }

            // =====================================================
            // CALCULATE TOTAL
            // =====================================================

            decimal grandTotal =
                cartItems.Sum(x =>
                    x.Product!.Price *
                    x.Quantity);

            // =====================================================
            // CREATE ORDER
            // =====================================================

            Order order =
                new Order
                {
                    UserId =
                        userId.Value,

                    OrderDate =
                        DateTime.Now,

                    TotalAmount =
                        grandTotal,

                    ShippingAddress =
                        shippingAddress,

                    Phone =
                        phone,

                    Status =
                        "Processing"
                };

            // =====================================================
            // CREATE ORDER ITEMS
            // =====================================================

            foreach (var cartItem in cartItems)
            {
                OrderItem orderItem =
                    new OrderItem
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

                order.OrderItems.Add(
                    orderItem);
            }

            // =====================================================
            // SAVE ORDER
            // =====================================================

            int orderId =
                await _orderService
                    .CreateOrderAsync(order);

            // =====================================================
            // CREATE PAYMENT RECORD
            // =====================================================

            string transactionId =
                session.PaymentIntentId;

            if (string.IsNullOrEmpty(transactionId))
            {
                transactionId =
                    session.Id;
            }

            Payment payment =
                new Payment
                {
                    OrderId =
                        orderId,

                    UserId =
                        userId.Value,

                    Amount =
                        grandTotal,

                    PaymentMethod =
                        "Stripe",

                    TransactionId =
                        transactionId,

                    PaymentDate =
                        DateTime.Now,

                    Status =
                        "Success"
                };

            // =====================================================
            // SAVE PAYMENT
            // =====================================================

            int paymentId =
                await _paymentService
                    .AddPaymentAsync(payment);

            // =====================================================
            // CLEAR CART
            // =====================================================

            foreach (var cartItem in cartItems)
            {
                await _cartService
                    .RemoveCartAsync(
                        cartItem.CartId);
            }

            // =====================================================
            // CLEAR CHECKOUT SESSION
            // =====================================================

            HttpContext.Session.Remove(
                "PendingShippingAddress");

            HttpContext.Session.Remove(
                "PendingPhone");

            HttpContext.Session.Remove(
                "PendingAmount");

            // =====================================================
            // PAYMENT SUCCESS
            // =====================================================

            TempData["Success"] =
                "Payment completed successfully.";

            return RedirectToAction(
                nameof(Confirmation),
                new
                {
                    id = orderId
                });
        }


        // =========================================================
        // ORDER CONFIRMATION
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Confirmation(
            int id)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var order =
                await _orderService
                    .GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Security check

            if (order.UserId != userId.Value)
            {
                return Forbid();
            }

            return View(order);
        }


        // =========================================================
        // MY ORDERS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login to view your orders.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var orders =
                await _orderService
                    .GetOrdersByUserAsync(
                        userId.Value);

            return View(orders);
        }


        // =========================================================
        // ORDER DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                TempData["Error"] =
                    "Please login to view your order.";

                return RedirectToAction(
                    "Login",
                    "Account");
            }

            var order =
                await _orderService
                    .GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Security check

            if (order.UserId != userId.Value)
            {
                return Forbid();
            }

            return View(order);
        }
    }
}