using System;
using System.Collections.Generic;
using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Models
{
    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public List<RepairOrder> Orders { get; set; } = new List<RepairOrder>();
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        
        public decimal TotalRevenue { get; set; }
        public decimal TotalLabor { get; set; }
        public decimal TotalPartsSale { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        
        public int TotalOrders { get; set; }
        public List<TopSellingPartViewModel> TopSellingParts { get; set; } = new List<TopSellingPartViewModel>();
    }

    public class TopSellingPartViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class MechanicPayoutReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<MechanicPayoutViewModel> Payouts { get; set; } = new();
        public decimal TotalCommission { get; set; }
    }

    public class MechanicPayoutViewModel
    {
        public int MechanicId { get; set; }
        public string MechanicName { get; set; } = string.Empty;
        public int TotalJobs { get; set; }
        public decimal TotalLaborRevenue { get; set; }
        public decimal TotalCommissionEarned { get; set; }
    }
}
