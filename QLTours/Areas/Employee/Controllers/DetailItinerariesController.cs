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

namespace QLTours.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class DetailItinerariesController : Controller
    {
        private readonly QuanLyTourContext _context;

        public DetailItinerariesController(QuanLyTourContext context)
        {
            _context = context;
        }

        // GET: Employee/DetailItineraries
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 5)
        {
            var totalItems = await _context.DetailItineraries.CountAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedItineraries = await _context.DetailItineraries
                .Include(d => d.Itinerary)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(pagedItineraries);
        }


        // GET: Employee/DetailItineraries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DetailItineraries == null)
            {
                return NotFound();
            }

            var detailItinerary = await _context.DetailItineraries
                .Include(d => d.Itinerary)
                .FirstOrDefaultAsync(m => m.DetailId == id);
            if (detailItinerary == null)
            {
                return NotFound();
            }

            return View(detailItinerary);
        }

        // GET: Employee/DetailItineraries/Create
        public IActionResult Create()
        {
            ViewData["ItineraryId"] = new SelectList(_context.Itineraries, "ItineraryId", "ItineraryId");
            return View();
        }

        // POST: Employee/DetailItineraries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DetailId,ItineraryId,ThoiGian,HoatDong,MoTa")] DetailItinerary detailItinerary)
        {
            if (ModelState.IsValid)
            {
                var lastDetail = await _context.DetailItineraries
                    .Where(d => d.ItineraryId == detailItinerary.ItineraryId)
                    .OrderByDescending(d => d.ThoiGian)
                    .FirstOrDefaultAsync();

                if (lastDetail != null && detailItinerary.ThoiGian.HasValue)
                {

                    if (detailItinerary.ThoiGian.Value <= lastDetail.ThoiGian.Value)
                    {
                        ModelState.AddModelError("ThoiGian", $"Thời gian phải lớn hơn {lastDetail.ThoiGian.Value:hh\\:mm tt}");
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Add(detailItinerary);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["ItineraryId"] = new SelectList(_context.Itineraries, "ItineraryId", "ItineraryId", detailItinerary.ItineraryId);
            return View(detailItinerary);
        }



        // GET: Employee/DetailItineraries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.DetailItineraries == null)
            {
                return NotFound();
            }

            var detailItinerary = await _context.DetailItineraries.FindAsync(id);
            if (detailItinerary == null)
            {
                return NotFound();
            }
            ViewData["ItineraryId"] = new SelectList(_context.Itineraries, "ItineraryId", "ItineraryId", detailItinerary.ItineraryId);
            return View(detailItinerary);
        }

        // POST: Employee/DetailItineraries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DetailId,ItineraryId,ThoiGian,HoatDong,MoTa")] DetailItinerary detailItinerary)
        {
            if (id != detailItinerary.DetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var lastDetail = await _context.DetailItineraries
                    .Where(d => d.ItineraryId == detailItinerary.ItineraryId)
                    .OrderByDescending(d => d.ThoiGian)
                    .FirstOrDefaultAsync();

                if (lastDetail != null && detailItinerary.ThoiGian.HasValue)
                {
                    if (detailItinerary.ThoiGian.Value <= lastDetail.ThoiGian.Value)
                    {
                        ModelState.AddModelError("ThoiGian", $"Thời gian phải lớn hơn {lastDetail.ThoiGian.Value:hh\\:mm tt}");
                    }
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        var existingDetail = _context.DetailItineraries.Local.FirstOrDefault(e => e.DetailId == id);
                        if (existingDetail != null)
                        {
                            _context.Entry(existingDetail).State = EntityState.Detached;
                        }

                        _context.Update(detailItinerary);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!DetailItineraryExists(detailItinerary.DetailId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }

            ViewData["ItineraryId"] = new SelectList(_context.Itineraries, "ItineraryId", "ItineraryId", detailItinerary.ItineraryId);
            return View(detailItinerary);
        }



        // GET: Employee/DetailItineraries/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DetailItineraries == null)
            {
                return NotFound();
            }

            var detailItinerary = await _context.DetailItineraries
                .Include(d => d.Itinerary)
                .FirstOrDefaultAsync(m => m.DetailId == id);
            if (detailItinerary == null)
            {
                return NotFound();
            }

            return View(detailItinerary);
        }

        // POST: Employee/DetailItineraries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DetailItineraries == null)
            {
                return Problem("Entity set 'QuanLyTourContext.DetailItineraries'  is null.");
            }
            var detailItinerary = await _context.DetailItineraries.FindAsync(id);
            if (detailItinerary != null)
            {
                _context.DetailItineraries.Remove(detailItinerary);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetailItineraryExists(int id)
        {
          return (_context.DetailItineraries?.Any(e => e.DetailId == id)).GetValueOrDefault();
        }
    }
}
