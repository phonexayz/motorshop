using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using Microsoft.AspNetCore.Http;

namespace MotorcycleRepairShop.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogActionAsync(string action, string entityName, string? entityId = null, string? details = null)
        {
            var username = _httpContextAccessor.HttpContext?.Session.GetString("Username") ?? "System";
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                Username = username,
                IpAddress = ipAddress,
                Timestamp = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
