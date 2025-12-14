using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLTours.Models;

namespace QLTours.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManageController : Controller
    {
        private readonly QuanLyTourContext _context;

        public ManageController(QuanLyTourContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            if (_context.Manages == null)
            {
                return Problem("Entity set 'QuanLyTourContext.Manages' is null.");
            }

            var manages = _context.Manages.AsNoTracking();
            var totalManages = await manages.CountAsync(); 
            var totalPages = (int)Math.Ceiling(totalManages / (double)pageSize); 

            var pagedManages = await manages
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalManages = totalManages;

            return View(pagedManages);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdMng,Username,Password,Role,Status")] Manage model)
        {
            if (ModelState.IsValid)
            {
                
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);// Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu

                model.Status = "Unlocked";

                _context.Manages.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(model); 
        }

        public async Task<IActionResult> Edit(int id)
        {
            var account = await _context.Manages.FindAsync(id);
            return account == null ? NotFound() : View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdMng,Username,Password,Role,Status")] Manage model)
        {
            if (id != model.IdMng || !ModelState.IsValid) return NotFound();

            var account = await _context.Manages.FindAsync(id);
            if (account == null) return NotFound();


            if (!string.IsNullOrEmpty(model.Password))
            {
                account.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);  // Mã hóa mật khẩu mới
            }

            account.Username = model.Username;
            account.Role = model.Role;
            account.Status = model.Status;

            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            var account = await _context.Manages.FindAsync(id);
            if (account == null) return NotFound();

            account.Status = (account.Status == "Locked") ? "Unlocked" : "Locked";  

            _context.Update(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // Xóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _context.Manages.FindAsync(id);
            if (account == null) return NotFound();

            _context.Manages.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

