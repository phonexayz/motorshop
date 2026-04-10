namespace MotorcycleRepairShop.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task<bool> RegisterAsync(string username, string password, string role = "Staff");
        Task LogoutAsync();
        string? GetCurrentUsername();
        bool IsAuthenticated();
        string? GetCurrentRole();
        int? GetCurrentUserId();
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
    }
}
