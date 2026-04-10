using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleRepairShop.Models
{
    public class Part
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? PartNumber { get; set; }
        
        [StringLength(50)]
        public string? Barcode { get; set; }
        
        public string? Description { get; set; }
        
        public string[]? CompatibleModels { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Cost { get; set; }
        
        public int StockQuantity { get; set; }
        
        public int MinStockLevel { get; set; } = 5;
        
        [StringLength(20)]
        public string Unit { get; set; } = "ອັນ";
        
        [StringLength(100)]
        public string? LegacySupplierName { get; set; }
        
        public int? SupplierId { get; set; }
        
        [ForeignKey("SupplierId")]
        public virtual Supplier? Supplier { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}