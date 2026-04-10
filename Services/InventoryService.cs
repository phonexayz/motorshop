using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MotorcycleRepairShop.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _context;

        public InventoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Part>> GetAllPartsAsync()
        {
            return await _context.Parts
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Part?> GetPartByIdAsync(int id)
        {
            return await _context.Parts.FindAsync(id);
        }

        public async Task<Part?> GetPartByBarcodeAsync(string barcode)
        {
            return await _context.Parts
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        public async Task<List<Part>> SearchPartsAsync(string searchTerm, string category = "")
        {
            var query = _context.Parts.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => 
                    p.Name.Contains(searchTerm) ||
                    p.PartNumber!.Contains(searchTerm) ||
                    (p.CompatibleModels != null && p.CompatibleModels.Any(m => m.Contains(searchTerm))));
            }

            if (!string.IsNullOrEmpty(category))
            {
                // สามารถเพิ่มการค้นหาตามหมวดหมู่ได้ถ้ามีฟิลด์ Category
            }

            return await query.OrderBy(p => p.Name).ToListAsync();
        }

        public async Task<List<Part>> GetLowStockPartsAsync()
        {
            return await _context.Parts
                .Where(p => p.StockQuantity <= p.MinStockLevel)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();
        }

        public async Task<Part> CreatePartAsync(Part part)
        {
            part.CreatedAt = DateTime.Now;
            part.UpdatedAt = DateTime.Now;
            
            _context.Parts.Add(part);
            await _context.SaveChangesAsync();
            
            return part;
        }

        public async Task<Part> UpdatePartAsync(Part part)
        {
            part.UpdatedAt = DateTime.Now;
            
            _context.Parts.Update(part);
            await _context.SaveChangesAsync();
            
            return part;
        }

        public async Task<bool> DeletePartAsync(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null)
                return false;

            _context.Parts.Remove(part);
            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> UpdateStockAsync(int partId, int quantity, string operation, string type = "Adjustment", string source = "Manual", string? sourceId = null, string? note = null, string? performedBy = null)
        {
            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
                return false;

            int previousQuantity = part.StockQuantity;
            int quantityChanged = operation == "add" ? quantity : -quantity;

            if (operation == "add")
            {
                part.StockQuantity += quantity;
            }
            else if (operation == "remove")
            {
                if (part.StockQuantity < quantity)
                    throw new InvalidOperationException("ສະຕ໋ອກບໍ່ພໍໍ");
                
                part.StockQuantity -= quantity;
            }
            else
            {
                return false;
            }

            part.UpdatedAt = DateTime.Now;

            // Record Log
            var log = new InventoryLog
            {
                PartId = partId,
                QuantityChanged = quantityChanged,
                PreviousQuantity = previousQuantity,
                NewQuantity = part.StockQuantity,
                LogType = type,
                Source = source,
                SourceId = sourceId,
                Note = note,
                CreatedBy = performedBy,
                CreatedAt = DateTime.Now
            };
            _context.InventoryLogs.Add(log);

            await _context.SaveChangesAsync();
            
            return true;
        }

        public async Task<List<InventoryLog>> GetInventoryLogsAsync(int? partId = null, int count = 100)
        {
            var query = _context.InventoryLogs
                .Include(l => l.Part)
                .AsQueryable();

            if (partId.HasValue)
            {
                query = query.Where(l => l.PartId == partId.Value);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetTotalPartsCountAsync()
        {
            return await _context.Parts.CountAsync();
        }
    }
}