using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MotorcycleRepairShop.Models
{
    public class RepairOrder
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        [Required]
        [StringLength(20)]
        public string MotorcycleLicensePlate { get; set; } = string.Empty;
        
        [Required]
        public string ProblemDescription { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled
        
        public bool IsPOS { get; set; } = false;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCost { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal PartsCost { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalCost { get; set; } = 0;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal TaxAmount { get; set; } = 0;
        
        public string? MechanicNotes { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public int? MechanicId { get; set; }
        public Mechanic? Mechanic { get; set; }
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MechanicCommission { get; set; } = 0;

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
