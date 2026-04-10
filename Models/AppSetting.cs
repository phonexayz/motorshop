using System.ComponentModel.DataAnnotations;

namespace MotorcycleRepairShop.Models
{
    public class AppSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty;

        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        public string? Description { get; set; }
        
        [StringLength(50)]
        public string Group { get; set; } = "General";
    }
}
