using System.ComponentModel.DataAnnotations;

namespace MotorcycleRepairShop.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        [Required]
        [StringLength(20)]
        public string LicensePlate { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string MotorcycleBrand { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string MotorcycleModel { get; set; } = string.Empty;
        
        public int? Year { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
