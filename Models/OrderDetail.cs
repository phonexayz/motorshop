using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleRepairShop.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }
        
        public int RepairOrderId { get; set; }
        public RepairOrder? RepairOrder { get; set; }
        
        public int? PartId { get; set; }
        public Part? Part { get; set; }
        
        public int? ServiceId { get; set; }
        public Service? Service { get; set; }
        
        public int Quantity { get; set; } = 1;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitCost { get; set; } = 0;
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount { get; set; } = 0;
        
        public string? Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
