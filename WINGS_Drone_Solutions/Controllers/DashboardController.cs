using Microsoft.AspNetCore.Mvc;
using WINGS.DAL.Connection;
using WINGS.Web.Filters;
using WINGS.Web.ViewModels;

namespace WINGS.Web.Controllers
{
    [AdminAuthorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            DashboardViewModel model = new DashboardViewModel
            {
                TotalCategory = _context.Categories.Count(),
                TotalProduct = _context.Products.Count(),
                TotalCustomer = _context.Users.Count(x => x.Role == "Customer"),
                TotalOrder = 0
            };

            return View(model);
        }
    }
}