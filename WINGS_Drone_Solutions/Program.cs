using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stripe;
using Microsoft.EntityFrameworkCore;
using WINGS.BLL.Services;
using WINGS.DAL.Connection;
using WINGS.Repository.Interfaces;
using WINGS.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Load Stripe API key from configuration or environment variable
var stripeKey = builder.Configuration["Stripe:SecretKey"]
               ?? Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");

if (string.IsNullOrWhiteSpace(stripeKey))
{
    throw new InvalidOperationException("Stripe API key not configured. Set 'Stripe:SecretKey' in appsettings or the STRIPE_SECRET_KEY environment variable.");
}

StripeConfiguration.ApiKey = stripeKey;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Category
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<CategoryService>();

// Product
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<WINGS.BLL.Services.ProductService>();

// User
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();

//Add to card
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<CartService>();

//for Order
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();

//payement
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<PaymentService>();

// Optionally register a StripeClient for DI consumers
builder.Services.AddSingleton(new StripeClient(stripeKey));

// Session
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// Enable Session
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();