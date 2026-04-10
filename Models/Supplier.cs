using System.ComponentModel.DataAnnotations;

namespace MotorcycleRepairShop.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "ກະລຸນາລະບຸຊື່ຮ້ານຂາຍສົ່ງ")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        public string? Address { get; set; }
        
        public string? Note { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation property for parts supplied by this supplier
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}
