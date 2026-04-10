using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Services;
using ZXing;
using ZXing.Windows.Compatibility;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager", "Staff")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IAuthService _authService;
        private readonly ISupplierService _supplierService;
        private readonly IAuditService _auditService;

        public InventoryController(ApplicationDbContext context, IInventoryService inventoryService, IAuthService authService, ISupplierService supplierService, IAuditService auditService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _authService = authService;
            _supplierService = supplierService;
            _auditService = auditService;
        }

        // GET: Inventory
        public async Task<IActionResult> Index(string search = "", string category = "")
        {
            var parts = await _inventoryService.SearchPartsAsync(search, category);
            ViewBag.Search = search;
            ViewBag.Category = category;
            return View(parts);
        }

        // GET: Inventory/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var part = await _inventoryService.GetPartByIdAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            return View(part);
        }

        // GET: Inventory/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View();
        }

        // POST: Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Part part)
        {
            if (ModelState.IsValid)
            {
                await _inventoryService.CreatePartAsync(part);
                await _auditService.LogActionAsync("CREATE", "Part", part.Id.ToString(), $"Created new part: {part.Name} ({part.PartNumber})");
                TempData["Success"] = "ເພີ່ມອະໄຫຼ່ແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(part);
        }

        // GET: Inventory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var part = await _inventoryService.GetPartByIdAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(part);
        }

        // POST: Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Part part)
        {
            if (id != part.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _inventoryService.UpdatePartAsync(part);
                await _auditService.LogActionAsync("UPDATE", "Part", part.Id.ToString(), $"Updated part details: {part.Name}");
                TempData["Success"] = "ແກ້ໄຂຂໍ້ມູນອະໄຫຼ່ແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Suppliers = await _supplierService.GetAllSuppliersAsync();
            return View(part);
        }

        // POST: Inventory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var part = await _inventoryService.GetPartByIdAsync(id);
            var partName = part?.Name ?? id.ToString();

            await _inventoryService.DeletePartAsync(id);
            await _auditService.LogActionAsync("DELETE", "Part", id.ToString(), $"Deleted part: {partName}");
            TempData["Success"] = "ລຶບອາໄຫຼ່ແລ້ວ";
            return RedirectToAction(nameof(Index));
        }

        // GET: Inventory/Scan
        public IActionResult Scan()
        {
            return View();
        }

        // POST: Inventory/ScanBarcode
        [HttpPost]
        public async Task<IActionResult> ScanBarcode(string barcode)
        {
            var part = await _inventoryService.GetPartByBarcodeAsync(barcode);
            if (part != null)
            {
                return Json(new { success = true, part = part });
            }
            return Json(new { success = false, message = "ບໍ່ພົບອະໄຫຼ່ທີ່ກົງກັບບາໂຄ້ດນີ້" });
        }

        // POST: Inventory/UpdateStock
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int id, int quantity, string operation, string? note = null)
        {
            try
            {
                var username = _authService.GetCurrentUsername();
                await _inventoryService.UpdateStockAsync(
                    id, 
                    quantity, 
                    operation, 
                    type: operation == "add" ? "Restock" : "Adjustment",
                    source: "Manual",
                    note: note ?? (operation == "add" ? "ເພີ່ມສະຕ໋ອກດ້ວຍມື" : "ປັບປຸງສະຕ໋ອກດ້ວຍມື"),
                    performedBy: username
                );
                return Json(new { success = true, message = "ອັບເດດສະຕ໋ອກແລ້ວ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Inventory/Logs
        public async Task<IActionResult> Logs(int? partId)
        {
            var logs = await _inventoryService.GetInventoryLogsAsync(partId);
            if (partId.HasValue)
            {
                ViewBag.Part = await _inventoryService.GetPartByIdAsync(partId.Value);
            }
            return View(logs);
        }

        // GET: Inventory/LowStock
        public async Task<IActionResult> LowStock()
        {
            var lowStockParts = await _inventoryService.GetLowStockPartsAsync();
            return View(lowStockParts);
        }

        // GET: Inventory/GetLowStockAlerts
        [HttpGet]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            var lowStockParts = await _inventoryService.GetLowStockPartsAsync();
            var result = new
            {
                count = lowStockParts.Count,
                items = lowStockParts.Take(5).Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    stock = p.StockQuantity,
                    min = p.MinStockLevel,
                    unit = p.Unit
                })
            };
            return Json(result);
        }

        // GET: Inventory/PrintLabels
        public async Task<IActionResult> PrintLabels(int? id)
        {
            List<Part> parts;
            if (id.HasValue)
            {
                var part = await _inventoryService.GetPartByIdAsync(id.Value);
                parts = part != null ? new List<Part> { part } : new List<Part>();
            }
            else
            {
                parts = await _inventoryService.GetAllPartsAsync();
            }
            return View(parts);
        }
    }
}
