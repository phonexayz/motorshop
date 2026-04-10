using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Services
{
    public class RepairOrderService : IRepairOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISettingService _settingService;
        private readonly IInventoryService _inventoryService;

        public RepairOrderService(ApplicationDbContext context, ISettingService settingService, IInventoryService inventoryService)
        {
            _context = context;
            _settingService = settingService;
            _inventoryService = inventoryService;
        }

        public async Task<List<RepairOrder>> GetAllOrdersAsync()
        {
            return await _context.RepairOrders
                .Include(r => r.Customer)
                .Include(r => r.OrderDetails)
                    .ThenInclude(d => d.Part)
                .Include(r => r.OrderDetails)
                    .ThenInclude(d => d.Service)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<RepairOrder?> GetOrderByIdAsync(int id)
        {
            return await _context.RepairOrders
                .Include(r => r.Customer)
                .Include(r => r.OrderDetails)
                    .ThenInclude(d => d.Part)
                .Include(r => r.OrderDetails)
                    .ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<RepairOrder>> GetOrdersByStatusAsync(string status)
        {
            return await _context.RepairOrders
                .Include(r => r.Customer)
                .Include(r => r.OrderDetails)
                    .ThenInclude(d => d.Part)
                .Include(r => r.OrderDetails)
                    .ThenInclude(d => d.Service)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<RepairOrder> CreateOrderAsync(RepairOrder order)
        {
            order.OrderNumber = await GenerateOrderNumberAsync();
            order.CreatedAt = DateTime.Now;
            order.UpdatedAt = DateTime.Now;
            
            _context.RepairOrders.Add(order);
            await _context.SaveChangesAsync();
            
            return order;
        }

        public async Task<RepairOrder> UpdateOrderAsync(RepairOrder order)
        {
            order.UpdatedAt = DateTime.Now;
            
            if (order.Status == "Completed" && !order.CompletionDate.HasValue)
            {
                order.CompletionDate = DateTime.Now;
            }
            
            _context.RepairOrders.Update(order);
            await _context.SaveChangesAsync();
            
            return order;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _context.RepairOrders.FindAsync(id);
            if (order == null)
                return false;

            _context.RepairOrders.Remove(order);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> AddOrderDetailAsync(int orderId, OrderDetail detail)
        {
            var order = await _context.RepairOrders.FindAsync(orderId);
            if (order == null)
                return false;

            detail.RepairOrderId = orderId;
            detail.CreatedAt = DateTime.Now;
            
            _context.OrderDetails.Add(detail);
            
            // Update stock if part is used
            if (detail.PartId.HasValue)
            {
                var part = await _context.Parts.FindAsync(detail.PartId.Value);
                if (part != null)
                {
                    detail.UnitCost = part.Cost ?? 0;
                    
                    // Use InventoryService to update stock and record log
                    string source = order.IsPOS ? "POS" : "RepairOrder";
                    string note = order.IsPOS ? $"ຂາຍສິນຄ້າ (Order #{order.OrderNumber})" : $"ນຳໃຊ້ໃນການສ້ອມແປງ (Order #{order.OrderNumber})";
                    
                    await _inventoryService.UpdateStockAsync(
                        detail.PartId.Value, 
                        detail.Quantity, 
                        "remove", 
                        type: order.IsPOS ? "Sale" : "Usage", 
                        source: source, 
                        sourceId: order.OrderNumber,
                        note: note
                    );
                }
            }
            
            await _context.SaveChangesAsync();
            
            // Recalculate order total
            await CalculateOrderTotalAsync(orderId);
            
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.RepairOrders
                .Include(r => r.Mechanic)
                .FirstOrDefaultAsync(r => r.Id == orderId);

            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.Now;
            
            if (status == "Completed")
            {
                if (!order.CompletionDate.HasValue)
                {
                    order.CompletionDate = DateTime.Now;
                }

                // Calculate mechanic commission based on LaborCost
                if (order.MechanicId.HasValue && order.Mechanic != null)
                {
                    order.MechanicCommission = order.LaborCost * (order.Mechanic.CommissionRate / 100m);
                }
            }
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateOrderTotalAsync(int orderId)
        {
            var orderDetails = await _context.OrderDetails
                .Where(d => d.RepairOrderId == orderId)
                .ToListAsync();

            var partsCost = orderDetails
                .Where(d => d.PartId.HasValue)
                .Sum(d => d.UnitPrice * d.Quantity * (1 - d.Discount / 100));

            var servicesCost = orderDetails
                .Where(d => d.ServiceId.HasValue)
                .Sum(d => d.UnitPrice * d.Quantity * (1 - d.Discount / 100));
                
            var totalCost = orderDetails
                .Sum(d => d.UnitCost * d.Quantity);

            var order = await _context.RepairOrders.FindAsync(orderId);
            if (order != null)
            {
                var taxRatePercent = await _settingService.GetDecimalValueAsync("TaxRatePercentage", 10);
                var taxAmount = partsCost * (taxRatePercent / 100m); // Dynamic tax on parts only
                
                order.PartsCost = partsCost;
                order.LaborCost = servicesCost;
                order.TaxAmount = taxAmount;
                order.TotalAmount = (partsCost + taxAmount) + servicesCost;
                order.TotalCost = totalCost;
                order.UpdatedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
            }

            return order?.TotalAmount ?? 0;
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Now.ToString("ddMMyyyy");
            var count = await _context.RepairOrders
                .CountAsync(r => r.CreatedAt.Date == DateTime.Now.Date);
            
            return $"RO{today}{(count + 1):D3}";
        }
    }
}
