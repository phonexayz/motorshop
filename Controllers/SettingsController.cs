using Microsoft.AspNetCore.Mvc;
using MotorcycleRepairShop.Services;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Filters;
using Microsoft.EntityFrameworkCore;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner")]
    public class SettingsController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IAuditService _auditService;
        private readonly MotorcycleRepairShop.Data.ApplicationDbContext _context;

        public SettingsController(ISettingService settingService, IAuditService auditService, MotorcycleRepairShop.Data.ApplicationDbContext context)
        {
            _settingService = settingService;
            _auditService = auditService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _settingService.GetSettingsListAsync();
            return View(settings);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Dictionary<string, string> settings, IFormFile? ShopLogoFile)
        {
            foreach (var setting in settings)
            {
                await _settingService.UpdateSettingAsync(setting.Key, setting.Value);
            }

            // Handle Logo Upload
            if (ShopLogoFile != null && ShopLogoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await ShopLogoFile.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    var base64String = Convert.ToBase64String(fileBytes);
                    var contentType = ShopLogoFile.ContentType;
                    await _settingService.UpdateSettingAsync("ShopLogo", $"data:{contentType};base64,{base64String}");
                }
            }

            await _auditService.LogActionAsync("UPDATE", "Settings", null, "Updated system settings and branding");
            
            TempData["SuccessMessage"] = "ບັນທຶກການຕັ້ງຄ່າສຳເລັດແລ້ວ!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ExportData()
        {
            var parts = _context.Parts.ToList();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,PartNumber,Name,Price,StockQuantity");
            
            foreach (var p in parts)
            {
                csv.AppendLine($"{p.Id},{p.PartNumber},{p.Name},{p.Price},{p.StockQuantity}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"MotorShop_Inventory_{DateTime.Now:yyyyMMdd}.csv");
        }

        [HttpGet]
        public async Task<IActionResult> AuditLogs()
        {
            var logs = await _context.AuditLogs.OrderByDescending(l => l.Timestamp).Take(200).ToListAsync();
            return View(logs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSystem()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Deleting in order of dependencies
                _context.OrderDetails.RemoveRange(_context.OrderDetails);
                _context.RepairOrders.RemoveRange(_context.RepairOrders);
                _context.Appointments.RemoveRange(_context.Appointments);
                _context.InventoryLogs.RemoveRange(_context.InventoryLogs);
                _context.Expenses.RemoveRange(_context.Expenses);
                _context.Parts.RemoveRange(_context.Parts);
                _context.Suppliers.RemoveRange(_context.Suppliers);
                _context.Customers.RemoveRange(_context.Customers);
                _context.Mechanics.RemoveRange(_context.Mechanics);
                _context.Services.RemoveRange(_context.Services);
                _context.AuditLogs.RemoveRange(_context.AuditLogs);
                
                await _context.SaveChangesAsync();

                // Create a new fresh audit log for the reset
                await _auditService.LogActionAsync("RESET", "System", null, "Full system factory reset performed. All test data cleared.");

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "ລະບົບໄດ້ຮັບການ Reset ຮຽບຮ້ອຍແລ້ວ! ຂໍ້ມູນທົດສອບທັງໝົດຖືກລຶບອອກແລ້ວຄັບ.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "ເກີດຂໍ້ຜິດພາດໃນການ Reset: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
