using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using MotorcycleRepairShop.Services;
using ClosedXML.Excel;
using System.IO;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director", "Manager")]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISettingService _settingService;

        public ReportController(ApplicationDbContext context, ISettingService settingService)
        {
            _context = context;
            _settingService = settingService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var model = await GetReportDataAsync(startDate, endDate);
            return View(model);
        }

        public async Task<IActionResult> MechanicPayouts(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = (endDate ?? DateTime.Today).AddDays(1).AddTicks(-1);

            var payouts = await _context.Mechanics
                .Where(m => m.IsActive || _context.RepairOrders.Any(o => o.MechanicId == m.Id && o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end))
                .Select(m => new MechanicPayoutViewModel
                {
                    MechanicId = m.Id,
                    MechanicName = m.Name,
                    TotalJobs = _context.RepairOrders.Count(o => o.MechanicId == m.Id && o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end),
                    TotalLaborRevenue = _context.RepairOrders.Where(o => o.MechanicId == m.Id && o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end).Sum(o => o.LaborCost),
                    TotalCommissionEarned = _context.RepairOrders.Where(o => o.MechanicId == m.Id && o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end).Sum(o => o.MechanicCommission)
                })
                .ToListAsync();

            var model = new MechanicPayoutReportViewModel
            {
                StartDate = start,
                EndDate = end,
                Payouts = payouts,
                TotalCommission = payouts.Sum(p => p.TotalCommissionEarned)
            };

            return View(model);
        }

        public async Task<IActionResult> ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            var report = await GetReportDataAsync(startDate, endDate);
            var shopName = await _settingService.GetValueAsync("ShopName", "Motorcycle Repair Shop");

            using (var workbook = new XLWorkbook())
            {
                // --- Sheet 1: Summary ---
                var wsSummary = workbook.Worksheets.Add("ສະຫຼຸບລາຍຮັບ-ລາຍຈ່າຍ");
                wsSummary.Cell(1, 1).Value = shopName;
                wsSummary.Cell(1, 1).Style.Font.Bold = true;
                wsSummary.Cell(1, 1).Style.Font.FontSize = 16;
                
                wsSummary.Cell(2, 1).Value = $"ບົດລາຍງານແຕ່ວັນທີ: {report.StartDate:dd/MM/yyyy} ຫາ {report.EndDate:dd/MM/yyyy}";
                
                var summaryTable = wsSummary.Range(4, 1, 10, 2);
                wsSummary.Cell(4, 1).Value = "ລາຍການ";
                wsSummary.Cell(4, 2).Value = "ຈຳນວນເງິນ";
                wsSummary.Range(4, 1, 4, 2).Style.Font.Bold = true;
                wsSummary.Range(4, 1, 4, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

                wsSummary.Cell(5, 1).Value = "ລາຍຮັບທັງໝົດ (Total Revenue)";
                wsSummary.Cell(5, 2).Value = report.TotalRevenue;
                
                wsSummary.Cell(6, 1).Value = "ຕົ້ນທຶນອາໄຫຼ່ (Total Parts Cost)";
                wsSummary.Cell(6, 2).Value = report.TotalCost;
                
                wsSummary.Cell(7, 1).Value = "ກຳໄລຈາກການຂາຍ (Gross Profit)";
                wsSummary.Cell(7, 2).Value = report.TotalProfit;
                
                wsSummary.Cell(8, 1).Value = "ລາຍຈ່າຍທັງໝົດ (Total Expenses)";
                wsSummary.Cell(8, 2).Value = report.TotalExpenses;
                
                wsSummary.Cell(10, 1).Value = "ກຳໄລສຸດທິ (Net Profit)";
                wsSummary.Cell(10, 2).Value = report.NetProfit;
                wsSummary.Cell(10, 1).Style.Font.Bold = true;
                wsSummary.Cell(10, 2).Style.Font.Bold = true;
                wsSummary.Cell(10, 2).Style.Font.Underline = XLFontUnderlineValues.Double;

                wsSummary.Columns().AdjustToContents();

                // --- Sheet 2: Sales Details ---
                var wsSales = workbook.Worksheets.Add("ລາຍລະອຽດການຂາຍ");
                wsSales.Cell(1, 1).Value = "ລາຍການບິນຂາຍ ແລະ ສ້ອມແປງ";
                wsSales.Range(1, 1, 1, 7).Merge().Style.Font.Bold = true;

                var salesHeaders = new[] { "ວັນທີ", "ເລກທີບິນ", "ລູກຄ້າ", "ລາຍຮັບ", "ຕົ້ນທຶນ", "ກຳໄລ", "ປະເພດ" };
                for (int i = 0; i < salesHeaders.Length; i++)
                {
                    wsSales.Cell(3, i + 1).Value = salesHeaders[i];
                }
                wsSales.Range(3, 1, 3, 7).Style.Font.Bold = true;
                wsSales.Range(3, 1, 3, 7).Style.Fill.BackgroundColor = XLColor.AliceBlue;

                int row = 4;
                foreach (var order in report.Orders)
                {
                    wsSales.Cell(row, 1).Value = order.CreatedAt;
                    wsSales.Cell(row, 2).Value = order.OrderNumber;
                    wsSales.Cell(row, 3).Value = order.Customer?.Name;
                    wsSales.Cell(row, 4).Value = order.TotalAmount;
                    wsSales.Cell(row, 5).Value = order.TotalCost;
                    wsSales.Cell(row, 6).Value = order.TotalAmount - order.TotalCost;
                    wsSales.Cell(row, 7).Value = order.IsPOS ? "POS" : "Repair";
                    row++;
                }
                wsSales.Columns().AdjustToContents();

                // --- Sheet 3: Expenses ---
                var wsExp = workbook.Worksheets.Add("ລາຍລະອຽດລາຍຈ່າຍ");
                var expHeaders = new[] { "ວັນທີ", "ຫົວຂໍ້", "ໝວດໝູ່", "ຈຳນວນເງິນ", "ໝາຍເຫດ" };
                for (int i = 0; i < expHeaders.Length; i++)
                {
                    wsExp.Cell(1, i + 1).Value = expHeaders[i];
                }
                wsExp.Range(1, 1, 1, 5).Style.Font.Bold = true;
                wsExp.Range(1, 1, 1, 5).Style.Fill.BackgroundColor = XLColor.LightPink;

                row = 2;
                foreach (var exp in report.Expenses)
                {
                    wsExp.Cell(row, 1).Value = exp.Date;
                    wsExp.Cell(row, 2).Value = exp.Title;
                    wsExp.Cell(row, 3).Value = exp.Category;
                    wsExp.Cell(row, 4).Value = exp.Amount;
                    wsExp.Cell(row, 5).Value = exp.Notes;
                    row++;
                }
                wsExp.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Report_{report.StartDate:yyyyMMdd}_{report.EndDate:yyyyMMdd}.xlsx";
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(DateTime? startDate, DateTime? endDate)
        {
            DateTime start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = (endDate ?? DateTime.Today).AddDays(1).AddTicks(-1);

            var orders = await _context.RepairOrders
                .Where(o => o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end)
                .Select(o => new { o.CreatedAt, o.TotalAmount, o.TotalCost })
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.Date >= start && e.Date <= end)
                .Select(e => new { e.Date, e.Amount })
                .ToListAsync();

            var labels = new List<string>();
            var revenueData = new List<decimal>();
            var profitData = new List<decimal>();
            var expenseData = new List<decimal>();

            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                labels.Add(date.ToString("dd/MM"));
                
                var dailyOrders = orders.Where(o => o.CreatedAt.Date == date).ToList();
                var dailyExpenses = expenses.Where(e => e.Date.Date == date).ToList();

                decimal revenue = dailyOrders.Sum(o => o.TotalAmount);
                decimal cost = dailyOrders.Sum(o => o.TotalCost);
                decimal expense = dailyExpenses.Sum(e => e.Amount);

                revenueData.Add(revenue);
                profitData.Add(revenue - cost);
                expenseData.Add(expense);
            }

            return Json(new
            {
                labels,
                revenue = revenueData,
                profit = profitData,
                expenses = expenseData
            });
        }

        private async Task<ReportViewModel> GetReportDataAsync(DateTime? startDate, DateTime? endDate)
        {
            // Default: This month
            DateTime start = startDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime end = (endDate ?? DateTime.Today).AddDays(1).AddTicks(-1);

            var orders = await _context.RepairOrders
                .Include(o => o.Customer)
                .Where(o => o.Status == "Completed" && o.CreatedAt >= start && o.CreatedAt <= end)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(e => e.Date >= start && e.Date <= end)
                .OrderBy(e => e.Date)
                .ToListAsync();

            decimal totalRevenue = orders.Sum(o => o.TotalAmount);
            decimal totalLabor = orders.Sum(o => o.LaborCost);
            decimal totalPartsSale = orders.Sum(o => o.PartsCost);
            decimal totalCost = orders.Sum(o => o.TotalCost);
            decimal totalProfit = totalRevenue - totalCost;
            decimal totalExpenses = expenses.Sum(e => e.Amount);

            var orderIds = orders.Select(o => o.Id).ToList();
            var topParts = await _context.OrderDetails
                .Where(od => orderIds.Contains(od.RepairOrderId) && od.PartId != null)
                .GroupBy(od => new { od.PartId, Name = od.Part != null ? od.Part.Name : "Unknown" })
                .Select(g => new TopSellingPartViewModel {
                    Name = g.Key.Name,
                    Quantity = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync();

            return new ReportViewModel
            {
                StartDate = start,
                EndDate = end,
                Orders = orders,
                Expenses = expenses,
                TotalRevenue = totalRevenue,
                TotalLabor = totalLabor,
                TotalPartsSale = totalPartsSale,
                TotalCost = totalCost,
                TotalProfit = totalProfit,
                TotalExpenses = totalExpenses,
                NetProfit = totalProfit - totalExpenses,
                ProfitMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0,
                TotalOrders = orders.Count,
                TopSellingParts = topParts
            };
        }
    }
}
