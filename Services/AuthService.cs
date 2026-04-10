using MotorcycleRepairShop.Data;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Models;
using System.Security.Cryptography;
using System.Text;

namespace MotorcycleRepairShop.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
                return false;

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
                return false;

            // Store user info in session
            _httpContextAccessor.HttpContext?.Session.SetString("Username", user.Username);
            _httpContextAccessor.HttpContext?.Session.SetString("Role", user.Role);
            _httpContextAccessor.HttpContext?.Session.SetInt32("UserId", user.Id);

            return true;
        }

        public async Task<bool> RegisterAsync(string username, string password, string role = "Staff")
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (existingUser != null)
                return false;

            // Create new user
            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task LogoutAsync()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public string? GetCurrentUsername()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Username");
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetCurrentUsername());
        }

        public string? GetCurrentRole()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("Role");
        }

        public int? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + "MotorcycleShop2024";
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // 1. Try new salted Base64 format (Current standard)
            var computedHash = HashPassword(password);
            if (computedHash == hashedPassword) return true;

            // 2. Try old hex format (For users migrated from local DB or created with old UserController logic)
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hexHash = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
                if (hexHash == hashedPassword) return true;
            }

            return false;
        }
    }
}