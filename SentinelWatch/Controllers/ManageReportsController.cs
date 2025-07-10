using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SentinelWatch.Data;
using SentinelWatch.Models;

namespace SentinelWatch.Controllers
{
    public class ManageReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManageReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ManageReports
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reports
                .Include(r => r.Location)
                .Include(r => r.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ManageReports/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.Location)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: ManageReports/Create
        public IActionResult Create()
        {
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: ManageReports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,LocationId,ReportType,Category,Severity,Magnitude,Temperature,Humidity,WindSpeed,Precipitation,ImageUrl,IsActive,Timestamp")] Report report)
        {
            ModelState.Remove("Location"); // Ignore validation errors for the Location object
            ModelState.Remove("User");     // Ignore validation errors for the User object (just in case)
            if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", report.LocationId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", report.UserId);
            return View(report);
        }

        // GET: ManageReports/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", report.LocationId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", report.UserId);
            return View(report);
        }

        // POST: ManageReports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,LocationId,ReportType,Category,Severity,Magnitude,Temperature,Humidity,WindSpeed,Precipitation,ImageUrl,IsActive,Timestamp")] Report report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Location"); // Ignore validation errors for the Location object
            ModelState.Remove("User");     // Ignore validation errors for the User object (just in case)

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.Id))
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
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", report.LocationId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", report.UserId);
            return View(report);
        }

        // GET: ManageReports/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.Location)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: ManageReports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
