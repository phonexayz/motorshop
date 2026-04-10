using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Services;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager", "Staff")]
    public class PosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepairOrderService _repairOrderService;
        private readonly IInventoryService _inventoryService;
        private readonly ISettingService _settingService;

        public PosController(ApplicationDbContext context, IRepairOrderService repairOrderService, IInventoryService inventoryService, ISettingService settingService)
        {
            _context = context;
            _repairOrderService = repairOrderService;
            _inventoryService = inventoryService;
            _settingService = settingService;
        }

        // GET: Pos
        public async Task<IActionResult> Index()
        {
            var parts = await _inventoryService.GetAllPartsAsync();
            ViewBag.Parts = parts;
            ViewBag.TaxRate = await _settingService.GetDecimalValueAsync("TaxRatePercentage", 10) / 100m;
            return View();
        }

        // POST: Pos/Checkout
        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] PosCheckoutRequest request)
        {
            try
            {
                if (request.Items == null || !request.Items.Any())
                {
                    return Json(new { success = false, message = "ການຊຳລະເງິນຫຼົ້ມເຫຼວ: ກະຕ່າສິນຄ້າວ່າງເປົ່າ" });
                }

                // Create a "Walk-in" customer if not exists
                var walkInCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Name == "ລູກຄ້າໜ້າຮ້ານ (Walk-in)");
                if (walkInCustomer == null)
                {
                    walkInCustomer = new Customer
                    {
                        Name = "ລູກຄ້າໜ້າຮ້ານ (Walk-in)",
                        Phone = "-",
                        LicensePlate = "POS-WALKIN",
                        MotorcycleBrand = "-",
                        MotorcycleModel = "-",
                        Address = "-",
                        CreatedAt = DateTime.Now
                    };
                    _context.Customers.Add(walkInCustomer);
                    await _context.SaveChangesAsync();
                }

                // Generate Order Number
                string orderNumber = await _repairOrderService.GenerateOrderNumberAsync();
                orderNumber = "POS-" + orderNumber.Replace("RO", "");

                // Create the Order
                var newPosOrder = new RepairOrder
                {
                    OrderNumber = orderNumber,
                    CustomerId = walkInCustomer.Id,
                    MotorcycleLicensePlate = walkInCustomer.LicensePlate,
                    ProblemDescription = "ຂາຍອາໄຫຼ່ໜ້າຮ້ານ (POS Sale)",
                    Status = "Completed",
                    IsPOS = true,
                    CompletionDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LaborCost = request.LaborCost,
                    MechanicNotes = "ຂາຍອາໄຫຼ່ໜ້າຮ້ານ (POS Sale)"
                };

                await _repairOrderService.CreateOrderAsync(newPosOrder);

                // Add Items
                foreach (var item in request.Items)
                {
                    var part = await _inventoryService.GetPartByIdAsync(item.PartId);
                    if (part == null) continue;

                    // Note: Adding order detail will automatically deduct stock and calculate unit cost
                    var orderDetail = new OrderDetail
                    {
                        PartId = item.PartId,
                        Quantity = item.Quantity,
                        UnitPrice = part.Price
                    };
                    
                    await _repairOrderService.AddOrderDetailAsync(newPosOrder.Id, orderDetail);
                }

                // Recalculate full totals including tax on parts
                await _repairOrderService.CalculateOrderTotalAsync(newPosOrder.Id);

                return Json(new { success = true, message = "ຂາຍສຳເລັດແລ້ວ!", orderId = newPosOrder.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "ຂໍ້ຜິດພາດ: " + ex.Message });
            }
        }

        // GET: Pos/Receipt/5
        public async Task<IActionResult> Receipt(int id)
        {
            var order = await _repairOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }
    }

    public class PosCheckoutRequest
    {
        public List<PosCartItem> Items { get; set; } = new List<PosCartItem>();
        public decimal LaborCost { get; set; }
    }

    public class PosCartItem
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
    }
}
