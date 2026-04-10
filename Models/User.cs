namespace MotorcycleRepairShop.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // เก็บ Password ที่เข้ารหัสแล้ว
        public string Role { get; set; } = "Admin"; // เช่น Admin, Staff
    }
}