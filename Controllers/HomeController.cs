using Microsoft.AspNetCore.Mvc;
using MotorcycleRepairShop.Services;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Data;
using Microsoft.EntityFrameworkCore;

using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager", "Staff", "Mechanic")]
    public class HomeController : Controller
    {
        private readonly IInventoryService _inventoryService;
        private readonly IRepairOrderService _repairOrderService;
        private readonly ApplicationDbContext _context;

        public HomeController(IInventoryService inventoryService, IRepairOrderService repairOrderService, ApplicationDbContext context)
        {
            _inventoryService = inventoryService;
            _repairOrderService = repairOrderService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sixMonthsAgo = DateTime.Today.AddMonths(-5);
            sixMonthsAgo = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var completedOrders = await _context.RepairOrders
                .Where(o => o.Status == "Completed" && o.CreatedAt >= sixMonthsAgo)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            var chartLabels = new List<string>();
            var revenueData = new List<decimal>();
            var costData = new List<decimal>();

            for (int i = 0; i < 6; i++)
            {
                var monthDate = sixMonthsAgo.AddMonths(i);
                var label = monthDate.ToString("MMM yyyy");
                chartLabels.Add(label);

                var monthOrders = completedOrders
                    .Where(o => o.CreatedAt.Month == monthDate.Month && o.CreatedAt.Year == monthDate.Year)
                    .ToList();

                revenueData.Add(monthOrders.Sum(o => o.TotalAmount));
                costData.Add(monthOrders.Sum(o => o.TotalCost));
            }

            // Build Activity Feed
            var recentOrders = await _context.RepairOrders
                .Include(o => o.Customer)
                .Where(o => o.Status == "Completed")
                .OrderByDescending(o => o.UpdatedAt)
                .Take(10)
                .ToListAsync();

            var recentInventoryLogs = await _context.InventoryLogs
                .Include(l => l.Part)
                .Where(l => l.LogType == "Restock" || l.LogType == "Adjustment")
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .ToListAsync();

            var activityFeed = new List<ActivityFeedItem>();

            foreach (var order in recentOrders)
            {
                activityFeed.Add(new ActivityFeedItem
                {
                    Icon = "bi-check-circle-fill",
                    IconColor = "success",
                    Title = $"ປິດບິນ #{order.OrderNumber}",
                    Description = $"ລູກຄ້າ: {order.Customer?.Name ?? "-"} | ຍອດ: {order.TotalAmount:N0} ₭",
                    Timestamp = order.UpdatedAt,
                    LinkUrl = $"/RepairOrder/Details/{order.Id}"
                });
            }

            foreach (var log in recentInventoryLogs)
            {
                var icon = log.QuantityChanged > 0 ? "bi-box-arrow-in-down" : "bi-box-arrow-up";
                var color = log.QuantityChanged > 0 ? "primary" : "warning";
                var action = log.QuantityChanged > 0 ? $"ເຕີມ +{log.QuantityChanged}" : $"ປັບ {log.QuantityChanged}";
                activityFeed.Add(new ActivityFeedItem
                {
                    Icon = icon,
                    IconColor = color,
                    Title = $"{action} ອາໄຫຼ່: {log.Part?.Name ?? "-"}",
                    Description = $"ສະຕ໋ອກ: {log.PreviousQuantity} → {log.NewQuantity} | {log.Note ?? ""}",
                    Timestamp = log.CreatedAt,
                    LinkUrl = $"/Inventory/Details/{log.PartId}"
                });
            }

            var dashboard = new DashboardViewModel
            {
                LowStockParts = await _inventoryService.GetLowStockPartsAsync(),
                TodayRepairOrders = await _repairOrderService.GetOrdersByStatusAsync("Pending"),
                InProgressRepairOrders = await _repairOrderService.GetOrdersByStatusAsync("InProgress"),
                TotalPartsCount = await _inventoryService.GetTotalPartsCountAsync(),
                TotalOrdersCount = (await _repairOrderService.GetAllOrdersAsync()).Count,
                TodayAppointments = await _context.Appointments
                    .Include(a => a.Customer)
                    .Where(a => a.AppointmentDate.Date == DateTime.Today.Date)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync(),
                ChartLabels = chartLabels,
                RevenueData = revenueData,
                CostData = costData,
                RecentActivities = activityFeed.OrderByDescending(a => a.Timestamp).Take(15).ToList()
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
        public IActionResult CreateRepairOrder()
        {
            return View();
        }
    }

    public class DashboardViewModel
    {
        public List<Part> LowStockParts { get; set; } = new();
        public List<RepairOrder> TodayRepairOrders { get; set; } = new();
        public List<RepairOrder> InProgressRepairOrders { get; set; } = new();
        public int TotalPartsCount { get; set; }
        public int TotalOrdersCount { get; set; }
        public List<Appointment> TodayAppointments { get; set; } = new();
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();
        public List<decimal> CostData { get; set; } = new();
        public List<ActivityFeedItem> RecentActivities { get; set; } = new();
    }

    public class ActivityFeedItem
    {
        public string Icon { get; set; } = string.Empty;
        public string IconColor { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? LinkUrl { get; set; }
    }
}
