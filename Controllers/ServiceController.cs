using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Service
        public async Task<IActionResult> Index(string search = "")
        {
            var query = _context.Services.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(search) || 
                                         (s.Category != null && s.Category.ToLower().Contains(search)));
                ViewBag.Search = search;
            }

            var services = await query.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync();
            return View(services);
        }

        // GET: Service/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                service.CreatedAt = DateTime.Now;
                _context.Add(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ເພີ່ມບໍລິການ/ຄ່າແຮງໃໝ່ແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve original CreatedAt
                    var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
                    if(existingService != null)
                    {
                        service.CreatedAt = existingService.CreatedAt;
                    }

                    _context.Update(service);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "ແກ້ໄຂບໍລິການແລ້ວ";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ລຶບບໍລິການແລ້ວ";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetServicesJson()
        {
            var services = await _context.Services
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name,
                    price = s.BasePrice,
                    category = s.Category
                })
                .ToListAsync();
            return Json(services);
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }
    }
}
