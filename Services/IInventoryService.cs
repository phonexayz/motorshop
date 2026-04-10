using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Services
{
    public interface IInventoryService
    {
        Task<List<Part>> GetAllPartsAsync();
        Task<Part?> GetPartByIdAsync(int id);
        Task<Part?> GetPartByBarcodeAsync(string barcode);
        Task<List<Part>> SearchPartsAsync(string searchTerm, string category = "");
        Task<List<Part>> GetLowStockPartsAsync();
        Task<Part> CreatePartAsync(Part part);
        Task<Part> UpdatePartAsync(Part part);
        Task<bool> DeletePartAsync(int id);
        Task<bool> UpdateStockAsync(int partId, int quantity, string operation, string type = "Adjustment", string source = "Manual", string? sourceId = null, string? note = null, string? performedBy = null);
        Task<List<InventoryLog>> GetInventoryLogsAsync(int? partId = null, int count = 100);
        Task<int> GetTotalPartsCountAsync();
    }
}
