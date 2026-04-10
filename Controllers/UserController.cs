using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorcycleRepairShop.Data;
using MotorcycleRepairShop.Models;
using System.Security.Cryptography;
using System.Text;
using MotorcycleRepairShop.Filters;

namespace MotorcycleRepairShop.Controllers
{
    [RoleAuthorize("Owner", "Director")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Username,Role")] User user, string Password)
        {
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("Password", "ກະລຸນາກຳນົດລະຫັດຜ່ານ");
            }
            
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "ຊື່ຜູ້ໃຊ້ນີ້ມີຢູ່ໃນລະບົບແລ້ວ");
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = HashPassword(Password);
                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ເພີ່ມຜູ້ໃຊ້ໃໝ່ແລ້ວ";
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Role")] User user, string? NewPassword)
        {
            if (id != user.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser != null)
                    {
                        // Update password only if provided
                        if (!string.IsNullOrEmpty(NewPassword))
                        {
                            user.PasswordHash = HashPassword(NewPassword);
                        }
                        else
                        {
                            user.PasswordHash = existingUser.PasswordHash;
                        }
                        
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "ອັບເດດຂໍ້ມູນຜູ້ໃຊ້ແລ້ວ";
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Prevent deleting the last admin
                if (user.Role == "Admin")
                {
                    var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
                    if (adminCount <= 1)
                    {
                        TempData["Error"] = "ບໍ່ສາມາດລຶບ Admin ຄົນສຸດທ້າຍໄດ້";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["Success"] = "ລຶບຜູ້ໃຊແລ້ວ";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
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
    }
}
