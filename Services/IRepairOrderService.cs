using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Services
{
    public interface IRepairOrderService
    {
        Task<List<RepairOrder>> GetAllOrdersAsync();
        Task<RepairOrder?> GetOrderByIdAsync(int id);
        Task<List<RepairOrder>> GetOrdersByStatusAsync(string status);
        Task<RepairOrder> CreateOrderAsync(RepairOrder order);
        Task<RepairOrder> UpdateOrderAsync(RepairOrder order);
        Task<bool> DeleteOrderAsync(int id);
        Task<bool> AddOrderDetailAsync(int orderId, OrderDetail detail);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<decimal> CalculateOrderTotalAsync(int orderId);
        Task<string> GenerateOrderNumberAsync();
    }
}
