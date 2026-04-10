using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Services
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllSuppliersAsync();
        Task<Supplier?> GetSupplierByIdAsync(int id);
        Task<Supplier> CreateSupplierAsync(Supplier supplier);
        Task UpdateSupplierAsync(Supplier supplier);
        Task DeleteSupplierAsync(int id);
        Task<IEnumerable<Part>> GetPartsBySupplierIdAsync(int supplierId);
    }
}
