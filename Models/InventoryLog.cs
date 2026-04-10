using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleRepairShop.Models
{
    public class InventoryLog
    {
        public int Id { get; set; }
        
        [Required]
        public int PartId { get; set; }
        
        [ForeignKey("PartId")]
        public Part? Part { get; set; }
        
        public int QuantityChanged { get; set; } // + for add, - for remove
        
        public int PreviousQuantity { get; set; }
        
        public int NewQuantity { get; set; }
        
        [Required]
        [StringLength(50)]
        public string LogType { get; set; } = string.Empty; // Restock, Adjustment, Sale, Usage
        
        [Required]
        [StringLength(50)]
        public string Source { get; set; } = string.Empty; // Manual, POS, RepairOrder
        
        [StringLength(50)]
        public string? SourceId { get; set; } // Related Order Number or ID
        
        public string? Note { get; set; }
        
        [StringLength(100)]
        public string? CreatedBy { get; set; } // Username
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
