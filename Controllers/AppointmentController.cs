using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Services;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager", "Staff")]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepairOrderService _repairOrderService;

        public AppointmentController(ApplicationDbContext context, IRepairOrderService repairOrderService)
        {
            _context = context;
            _repairOrderService = repairOrderService;
        }

        // GET: Appointment/OpenOrder/5
        public async Task<IActionResult> OpenOrder(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Create a new Repair Order
            var order = new RepairOrder
            {
                CustomerId = appointment.CustomerId,
                MotorcycleLicensePlate = appointment.Customer?.LicensePlate ?? "-",
                ProblemDescription = appointment.Reason ?? "Receipt from Appointment",
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            await _repairOrderService.CreateOrderAsync(order);

            // Mark appointment as completed
            appointment.Status = "Completed";
            await _context.SaveChangesAsync();

            TempData["Success"] = "ບັບທຶກການຮັບລົດ ແລະ ສ້າງໃບສັ່ງຊ້ອມແລ້ວ";
            return RedirectToAction("Details", "RepairOrder", new { id = order.Id });
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            // Only show upcoming or recently past appointments
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Where(a => a.AppointmentDate >= DateTime.Today.AddDays(-7))
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();
                
            return View(appointments);
        }

        // GET: Appointment/Create
        public IActionResult Create()
        {
            ViewBag.Customers = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name");
            return View(new Appointment { AppointmentDate = DateTime.Today.AddDays(1).AddHours(10) });
        }

        // POST: Appointment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ເພີ່ມຄິວນັດໝາຍແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", appointment.CustomerId);
            return View(appointment);
        }

        // GET: Appointment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            ViewBag.Customers = new SelectList(_context.Customers.OrderBy(c => c.Name), "Id", "Name", appointment.CustomerId);
            return View(appointment);
        }

        // POST: Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "ອັບເດດຂໍ້ມູນຄິວນັດໝາຍແລ້ວ";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = new SelectList(_context.Customers, "Id", "Name", appointment.CustomerId);
            return View(appointment);
        }
        
        // POST: Appointment/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if(appointment == null) return Json(new { success = false, message = "ບໍ່ພົບຄິວນັດໝາຍ" });
            
            appointment.Status = status;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
