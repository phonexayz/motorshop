using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Services
{
    public interface ISettingService
    {
        Task<string> GetValueAsync(string key, string defaultValue = "");
        Task<decimal> GetDecimalValueAsync(string key, decimal defaultValue = 0);
        Task<int> GetIntValueAsync(string key, int defaultValue = 0);
        Task UpdateValueAsync(string key, string value);
        Task UpdateSettingAsync(string key, string value); // Add this for controller compatibility
        Task<Dictionary<string, string>> GetAllSettingsAsync();
        Task<List<AppSetting>> GetSettingsListAsync();
    }
}
