using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager", "Staff")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index(string search = "")
        {
            var query = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(c => (c.Name != null && c.Name.ToLower().Contains(search)) || 
                                         (c.LicensePlate != null && c.LicensePlate.ToLower().Contains(search)) ||
                                         (c.Phone != null && c.Phone.Contains(search)));
                ViewBag.Search = search;
            }

            var customers = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return View(customers);
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null) return NotFound();

            // Load repair history for this customer
            ViewBag.RepairHistory = await _context.RepairOrders
                .Where(r => r.CustomerId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .ToListAsync();

            return View(customer);
        }

        // GET: Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ເພີ່ມລູກຄ້າໃໝ່ແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Preserve original CreatedAt
                    var existingCustomer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if(existingCustomer != null)
                    {
                        customer.CreatedAt = existingCustomer.CreatedAt;
                    }
                    
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "ແກ້ໄຂຂໍ້ມູນລູກຄ້າແລ້ວ";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ລຶບຂໍ້ມູນລູກຄ້າແລ້ວ";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
