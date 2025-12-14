using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private readonly QuanLyTourContext _context;

        public ContactController(QuanLyTourContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var contactList = await _context.Contacts.ToListAsync(); 
            return View(contactList); 
        }

        public async Task<IActionResult> Details(int? contactId)
        {
            if (contactId == null)
            {
                return NotFound();
            }

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(c => c.ContactId == contactId); 
            if (contact == null)
            {
                return NotFound();
            }

            contact.Status = "Đã xem";
            _context.Update(contact);
            await _context.SaveChangesAsync();

            return View(contact); 
        }
    }
}
