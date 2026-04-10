using Microsoft.AspNetCore.Mvc;
using MotorcycleRepairShop.Services;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IAuditService _auditService;
    public AccountController(IAuthService authService, IAuditService auditService)
    {
        _authService = authService;
        _auditService = auditService;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (await _authService.LoginAsync(username, password))
        {
            await _auditService.LogActionAsync("LOGIN", "User", username, "User logged into the system");
            // ถ้า Login สำเร็จ ให้ไปหน้า Dashboard (Home)
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Error = "ຊື່ຜູ້ໃຊ້ຫຼືລະຫັດຜ່ານບໍ່ຖືກຕ້ອງ";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        var username = _authService.GetCurrentUsername();
        await _auditService.LogActionAsync("LOGOUT", "User", username, "User logged out");
        await _authService.LogoutAsync();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Profile()
    {
        if (!_authService.IsAuthenticated()) return RedirectToAction("Login");
        ViewBag.Username = _authService.GetCurrentUsername();
        ViewBag.Role = _authService.GetCurrentRole();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string newPassword, string confirmPassword)
    {
        if (!_authService.IsAuthenticated()) return RedirectToAction("Login");

        if (string.IsNullOrEmpty(newPassword) || newPassword != confirmPassword)
        {
            TempData["Error"] = "ລະຫັດຜ່ານໃໝ່ບໍ່ຖືກຕ້ອງ ຫຼື ບໍ່ກົງກັນ";
            return RedirectToAction("Profile");
        }

        var userId = _authService.GetCurrentUserId();
        if (userId.HasValue)
        {
            await _authService.UpdatePasswordAsync(userId.Value, newPassword);
            await _auditService.LogActionAsync("UPDATE", "User", userId.Value.ToString(), "User changed their own password");
            TempData["Success"] = "ປ່ຽນລະຫັດຜ່ານສຳເລັດແລ້ວ";
        }
        
        return RedirectToAction("Profile");
    }
}