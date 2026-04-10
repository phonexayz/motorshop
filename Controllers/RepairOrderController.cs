using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Services;
using MotorcycleRepairShop.Data;
using System.Linq;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager", "Staff", "Mechanic")]
    public class RepairOrderController : Controller
    {
        private readonly IRepairOrderService _repairOrderService;
        private readonly IInventoryService _inventoryService;
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public RepairOrderController(IRepairOrderService repairOrderService, IInventoryService inventoryService, IAuditService auditService, ApplicationDbContext context)
        {
            _repairOrderService = repairOrderService;
            _inventoryService = inventoryService;
            _auditService = auditService;
            _context = context;
        }

        // GET: RepairOrder
        public async Task<IActionResult> Index(string status = "", string searchString = "")
        {
            List<RepairOrder> orders;
            
            if (!string.IsNullOrEmpty(status))
            {
                orders = await _repairOrderService.GetOrdersByStatusAsync(status);
                ViewBag.Status = status;
            }
            else
            {
                orders = await _repairOrderService.GetAllOrdersAsync();
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                orders = orders.Where(o => 
                    o.OrderNumber.ToLower().Contains(searchString) ||
                    (o.Customer?.Name != null && o.Customer.Name.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(o.MotorcycleLicensePlate) && o.MotorcycleLicensePlate.ToLower().Contains(searchString))
                ).ToList();
            }

            ViewBag.SearchString = searchString;

            return View(orders);
        }

        // GET: RepairOrder/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _repairOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.Mechanics = await _context.Mechanics.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();
            ViewBag.Services = await _context.Services.OrderBy(s => s.Name).ToListAsync();
            return View(order);
        }

        // GET: RepairOrder/Print/5
        public async Task<IActionResult> Print(int id)
        {
            var order = await _repairOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewBag.Mechanics = await _context.Mechanics.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();
            return View(order);
        }

        // GET: RepairOrder/Create
        public async Task<IActionResult> Create()
        {
            var parts = await _inventoryService.GetAllPartsAsync();
            ViewBag.Parts = parts;
            
            ViewBag.Customers = _context.Customers.OrderByDescending(c => c.CreatedAt).ToList();
            ViewBag.Mechanics = await _context.Mechanics.Where(m => m.IsActive).OrderBy(m => m.Name).ToListAsync();

            string prefix = $"RO-{DateTime.Now.Year}-";
            var lastOrder = _context.RepairOrders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefault();
                
            string newOrderNum = prefix + "0001";
            if (lastOrder != null)
            {
                string lastNumStr = lastOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumStr, out int lastNum))
                {
                    newOrderNum = prefix + (lastNum + 1).ToString("D4");
                }
            }
            
            var model = new RepairOrder { OrderNumber = newOrderNum, Status = "Pending", CreatedAt = DateTime.Now };
            return View(model);
        }

        // POST: RepairOrder/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RepairOrder order)
        {
            if (ModelState.IsValid)
            {
                await _repairOrderService.CreateOrderAsync(order);
                await _auditService.LogActionAsync("CREATE", "RepairOrder", order.Id.ToString(), $"Created new order {order.OrderNumber}");
                TempData["Success"] = "ສ້າງໃບບັນທຶກການສ້ອມແປງແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Parts = await _inventoryService.GetAllPartsAsync();
            ViewBag.Customers = _context.Customers.OrderByDescending(c => c.CreatedAt).ToList();
            return View(order);
        }

        // POST: RepairOrder/QuickAddCustomer
        [HttpPost]
        public async Task<IActionResult> QuickAddCustomer([FromBody] Customer customer)
        {
            try
            {
                if (string.IsNullOrEmpty(customer.Name) || string.IsNullOrEmpty(customer.LicensePlate) || string.IsNullOrEmpty(customer.MotorcycleBrand) || string.IsNullOrEmpty(customer.MotorcycleModel))
                {
                    return Json(new { success = false, message = "ກະລຸນາເພີ່ມຂໍ້ມູນທີ່ຈຳເປັນໃຫ້ຄົບຖ້ວນ (ຊື່, ທະບຽນລົດ, ຍີ່ຫໍ້, ລຸ້ນ)" });
                }
                
                customer.CreatedAt = DateTime.Now;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "ເພີ່ມລູກຄ້າໃໝ່ແລ້ວ", customerId = customer.Id, customerName = customer.Name, licensePlate = customer.LicensePlate, brand = customer.MotorcycleBrand, model = customer.MotorcycleModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "ເກີດຂໍ້ຜິດພາດ: " + ex.Message });
            }
        }

        // GET: RepairOrder/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _repairOrderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        // POST: RepairOrder/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RepairOrder order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _repairOrderService.UpdateOrderAsync(order);
                await _auditService.LogActionAsync("UPDATE", "RepairOrder", order.Id.ToString(), $"Updated order {order.OrderNumber}");
                TempData["Success"] = "ແກ້ໄຂໃບບັນທຶກການສ້ອມແປງແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            return View(order);
        }

        // POST: RepairOrder/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _repairOrderService.GetOrderByIdAsync(id);
            var orderNum = order?.OrderNumber ?? id.ToString();

            var success = await _repairOrderService.DeleteOrderAsync(id);
            if (success)
            {
                await _auditService.LogActionAsync("DELETE", "RepairOrder", id.ToString(), $"Deleted order {orderNum}");
                TempData["Success"] = "ລຶບໃບສັ່ງສ້ອມສຳເລັດແລ້ວ";
            }
            else
            {
                TempData["Error"] = "ບໍ່ສາມາດລຶບໃບສັ່ງສ້ອມໄດ້";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: RepairOrder/UpdateStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                await _repairOrderService.UpdateOrderStatusAsync(id, status);
                return Json(new { success = true, message = "ອັບເດດສະຖານະແລ້ວ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: RepairOrder/AddPart/5
        [HttpPost]
        public async Task<IActionResult> AddPart(int orderId, int partId, int quantity)
        {
            try
            {
                var part = await _inventoryService.GetPartByIdAsync(partId);
                if (part == null)
                {
                    return Json(new { success = false, message = "ບໍ່ພົບອາໄຫຼ່" });
                }

                if (part.StockQuantity < quantity)
                {
                    return Json(new { success = false, message = "ສະຕ໋ອກບໍ່ພຽງພໍ" });
                }

                var orderDetail = new OrderDetail
                {
                    PartId = partId,
                    Quantity = quantity,
                    UnitPrice = part.Price,
                    UnitCost = part.Cost ?? 0
                };

                await _repairOrderService.AddOrderDetailAsync(orderId, orderDetail);
                return Json(new { success = true, message = "ເພີ່ມອາໄຫຼ່ແລ້ວ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: RepairOrder/AddService
        [HttpPost]
        public async Task<IActionResult> AddService(int orderId, int serviceId, int quantity)
        {
            try
            {
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "ບໍ່ພົບການບໍລິການ" });
                }

                var orderDetail = new OrderDetail
                {
                    ServiceId = serviceId,
                    Quantity = quantity,
                    UnitPrice = service.BasePrice,
                    UnitCost = 0
                };

                await _repairOrderService.AddOrderDetailAsync(orderId, orderDetail);
                return Json(new { success = true, message = "ເພີ່ມການບໍລິການແລ້ວ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: RepairOrder/Dashboard (สำหรับช่างซ่อม)
        public async Task<IActionResult> Dashboard()
        {
            var inProgressOrders = await _repairOrderService.GetOrdersByStatusAsync("InProgress");
            var pendingOrders = await _repairOrderService.GetOrdersByStatusAsync("Pending");
            
            var dashboard = new RepairDashboardViewModel
            {
                InProgressOrders = inProgressOrders,
                PendingOrders = pendingOrders,
                TodayOrders = inProgressOrders.Where(o => o.CreatedAt.Date == DateTime.Today).ToList()
            };

            return View(dashboard);
        }

        // POST: RepairOrder/CompleteRepair/5
        [HttpPost]
        public async Task<IActionResult> CompleteRepair(int id, decimal laborCost, string notes)
        {
            try
            {
                var order = await _repairOrderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return Json(new { success = false, message = "ບໍ່ພົບໃບບັນທຶກການສ້ອມແປງ" });
                }

                order.LaborCost = laborCost;
                order.MechanicNotes = notes;
                await _repairOrderService.UpdateOrderStatusAsync(id, "Completed");
                
                var total = await _repairOrderService.CalculateOrderTotalAsync(id);
                
                return Json(new { 
                    success = true, 
                    message = "ການສ້ອມແປງສຳເລັດແລ້ວ",
                    totalAmount = total
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignMechanic(int orderId, int mechanicId)
        {
            try
            {
                var order = await _context.RepairOrders.FindAsync(orderId);
                if (order == null) return Json(new { success = false, message = "ບໍ່ພົບໃບສັ່ງຊ້ອມ" });

                order.MechanicId = mechanicId;
                order.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
                await _auditService.LogActionAsync("ASSIGN", "RepairOrder", orderId.ToString(), $"Assigned mechanic {mechanicId} to order {order.OrderNumber}");
                return Json(new { success = true, message = "ມອບໝາຍຊ່າງແລ້ວ" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: RepairOrder/History?plateNumber=xxx
        public async Task<IActionResult> History(string plateNumber)
        {
            if (string.IsNullOrEmpty(plateNumber))
            {
                return View(new List<RepairOrder>());
            }

            var orders = await _context.RepairOrders
                .Include(o => o.Customer)
                .Include(o => o.Mechanic)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Part)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Service)
                .Where(o => o.MotorcycleLicensePlate.ToLower() == plateNumber.ToLower())
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.PlateNumber = plateNumber;
            return View(orders);
        }
    }

    public class RepairDashboardViewModel
    {
        public List<RepairOrder> InProgressOrders { get; set; } = new();
        public List<RepairOrder> PendingOrders { get; set; } = new();
        public List<RepairOrder> TodayOrders { get; set; } = new();
    }
}
