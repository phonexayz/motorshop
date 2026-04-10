using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager")]
    public class MechanicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MechanicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mechanic
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mechanics.OrderBy(m => m.Name).ToListAsync());
        }

        // GET: Mechanic/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mechanic/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,Specialization,IsActive,CommissionRate")] Mechanic mechanic)
        {
            if (ModelState.IsValid)
            {
                mechanic.CreatedAt = DateTime.Now;
                _context.Add(mechanic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mechanic);
        }

        // GET: Mechanic/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mechanic = await _context.Mechanics.FindAsync(id);
            if (mechanic == null) return NotFound();

            return View(mechanic);
        }

        // POST: Mechanic/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Specialization,IsActive,CreatedAt,CommissionRate")] Mechanic mechanic)
        {
            if (id != mechanic.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mechanic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MechanicExists(mechanic.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(mechanic);
        }

        // GET: Mechanic/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var mechanic = await _context.Mechanics.FirstOrDefaultAsync(m => m.Id == id);
            if (mechanic == null) return NotFound();

            return View(mechanic);
        }

        // POST: Mechanic/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mechanic = await _context.Mechanics.FindAsync(id);
            if (mechanic != null)
            {
                _context.Mechanics.Remove(mechanic);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MechanicExists(int id)
        {
            return _context.Mechanics.Any(e => e.Id == id);
        }
    }
}
