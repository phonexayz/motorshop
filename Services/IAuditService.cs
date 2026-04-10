namespace MotorcycleRepairShop.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string action, string entityName, string? entityId = null, string? details = null);
    }
}
