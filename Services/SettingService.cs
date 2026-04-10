using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;

namespace MotorcycleRepairShop.Services
{
    public class SettingService : ISettingService
    {
        private readonly ApplicationDbContext _context;

        public SettingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetValueAsync(string key, string defaultValue = "")
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value ?? defaultValue;
        }

        public async Task<decimal> GetDecimalValueAsync(string key, decimal defaultValue = 0)
        {
            var value = await GetValueAsync(key);
            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return defaultValue;
        }

        public async Task<int> GetIntValueAsync(string key, int defaultValue = 0)
        {
            var value = await GetValueAsync(key);
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return defaultValue;
        }

        public async Task UpdateValueAsync(string key, string value)
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value ?? string.Empty;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateSettingAsync(string key, string value)
        {
            await UpdateValueAsync(key, value);
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            return await _context.AppSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
        }

        public async Task<List<AppSetting>> GetSettingsListAsync()
        {
            return await _context.AppSettings.ToListAsync();
        }
    }
}
