using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleRepairShop.Models
{
    public class Mechanic
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Specialization { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionRate { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
