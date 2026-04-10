using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleRepairShop.Models
{
    public class Service
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }
        
        [Column(TypeName = "decimal(4,2)")]
        public decimal? EstimatedHours { get; set; }
        
        [StringLength(50)]
        public string? Category { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
