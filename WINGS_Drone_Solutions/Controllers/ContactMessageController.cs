using Microsoft.AspNetCore.Mvc;
using WINGS.BLL.Services;
using WINGS.Web.Filters;

namespace WINGS.Web.Controllers
{
    [AdminAuthorize]
    public class ContactMessageController : Controller
    {
        private readonly ContactMessageService
            _contactMessageService;

        public ContactMessageController(
            ContactMessageService contactMessageService)
        {
            _contactMessageService =
                contactMessageService;
        }


        // ==========================================
        // ALL CONTACT MESSAGES
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var messages =
                await _contactMessageService
                    .GetAllMessagesAsync();

            return View(messages);
        }


        // ==========================================
        // VIEW MESSAGE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var message =
                await _contactMessageService
                    .GetMessageByIdAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            // Automatically mark as read
            if (!message.IsRead)
            {
                await _contactMessageService
                    .MarkAsReadAsync(id);

                // Update local object too
                message.IsRead = true;
            }

            return View(message);
        }


        // ==========================================
        // MARK AS READ
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _contactMessageService
                .MarkAsReadAsync(id);

            return RedirectToAction(nameof(Index));
        }
    }
}