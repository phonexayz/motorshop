using System.ComponentModel.DataAnnotations;

namespace MotorcycleRepairShop.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string? Reason { get; set; } // อาการที่แจ้งไว้ล่วงหน้า
        
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Cancelled, Completed

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
