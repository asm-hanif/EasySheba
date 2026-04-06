using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EasySheba.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SuperAdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // ✅ DASHBOARD
        // ==========================================
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalHospitals = await _context.HospitalAdminProfiles.CountAsync();
            ViewBag.ApprovedHospitals = await _context.HospitalAdminProfiles.CountAsync(h => h.IsApproved);
            ViewBag.PendingHospitals = await _context.HospitalAdminProfiles.CountAsync(h => !h.IsApproved);
            ViewBag.TotalSuperAdmins = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Count;

            return View();
        }

        // ==========================================
        // ✅ HOSPITAL ADMIN REQUESTS
        // ==========================================
        public async Task<IActionResult> Requests()
        {
            var admins = await _context.HospitalAdminProfiles
                .OrderByDescending(h => h.Id)
                .ToListAsync();
            return View(admins);
        }

        // ==========================================
        // ✅ APPROVE HOSPITAL ADMIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var admin = await _context.HospitalAdminProfiles.FindAsync(id);

            if (admin != null)
            {
                admin.IsApproved = true;
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(admin.UserId);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }

                TempData["Success"] = $"Hospital Admin '{admin.HospitalName}' approved successfully!";
            }
            else
            {
                TempData["Error"] = "Hospital Admin not found!";
            }

            return RedirectToAction("Requests");
        }

        // ==========================================
        // ✅ REJECT HOSPITAL ADMIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var admin = await _context.HospitalAdminProfiles.FindAsync(id);

            if (admin != null)
            {
                string hospitalName = admin.HospitalName;

                var user = await _userManager.FindByIdAsync(admin.UserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.HospitalAdminProfiles.Remove(admin);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Hospital Admin '{hospitalName}' rejected and removed!";
            }
            else
            {
                TempData["Error"] = "Hospital Admin not found!";
            }

            return RedirectToAction("Requests");
        }

        // ==========================================
        // ✅ VIEW ALL HOSPITALS
        // ==========================================
        public async Task<IActionResult> Hospitals()
        {
            var hospitals = await _context.HospitalAdminProfiles
                .Where(h => h.IsApproved)
                .OrderBy(h => h.HospitalName)
                .ToListAsync();
            return View(hospitals);
        }

        // ==========================================
        // ✅ PERMANENTLY DELETE HOSPITAL
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            var admin = await _context.HospitalAdminProfiles.FindAsync(id);

            if (admin != null)
            {
                string hospitalName = admin.HospitalName;

                var user = await _userManager.FindByIdAsync(admin.UserId);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.HospitalAdminProfiles.Remove(admin);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Hospital '{hospitalName}' has been permanently deleted!";
            }
            else
            {
                TempData["Error"] = "Hospital not found!";
            }

            return RedirectToAction("Hospitals");
        }

        // ==========================================
        // ✅ VIEW ALL SUPER ADMINS
        // ==========================================
        public async Task<IActionResult> SuperAdmins()
        {
            var superAdminUsers = await _userManager.GetUsersInRoleAsync("SuperAdmin");
            var superAdmins = superAdminUsers.Select(u => new SuperAdminViewModel
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                EmailConfirmed = u.EmailConfirmed
            }).ToList();

            return View(superAdmins);
        }

        // ==========================================
        // ✅ ADD NEW SUPER ADMIN
        // ==========================================
        [HttpGet]
        public IActionResult AddSuperAdmin()
        {
            return View(new AddSuperAdminViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSuperAdmin(AddSuperAdminViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email already exists!");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "SuperAdmin");
                TempData["Success"] = $"Super Admin {model.Email} created successfully!";
                return RedirectToAction("SuperAdmins");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // ==========================================
        // ✅ REMOVE SUPER ADMIN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSuperAdmin(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == id)
            {
                TempData["Error"] = "You cannot remove yourself!";
                return RedirectToAction("SuperAdmins");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                string userEmail = user.Email;
                await _userManager.DeleteAsync(user);
                TempData["Success"] = $"Super Admin '{userEmail}' has been removed!";
            }
            else
            {
                TempData["Error"] = "User not found!";
            }

            return RedirectToAction("SuperAdmins");
        }

        // ==========================================
        // ✅ VIEW HOSPITAL DETAILS
        // ==========================================
        public async Task<IActionResult> HospitalDetails(int id)
        {
            var admin = await _context.HospitalAdminProfiles.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // ==========================================
        // ✅ DEBUG METHOD - Check if data exists
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> DebugRequests()
        {
            var admins = await _context.HospitalAdminProfiles.ToListAsync();

            string result = "=== HOSPITAL ADMIN REQUESTS DEBUG ===\n\n";
            result += $"Total Records in Database: {admins.Count}\n";
            result += $"Database Table Name: HospitalAdminProfiles\n";
            result += $"=====================================\n\n";

            if (admins.Count == 0)
            {
                result += "⚠️ NO RECORDS FOUND IN DATABASE!\n\n";
                result += "Possible issues:\n";
                result += "1. Table might be empty\n";
                result += "2. Registration might not be saving\n";
                result += "3. Database connection issue\n";
            }
            else
            {
                foreach (var admin in admins)
                {
                    result += $"📋 RECORD ID: {admin.Id}\n";
                    result += $"   Hospital Name: {admin.HospitalName}\n";
                    result += $"   Contact Number: {admin.ContactNumber}\n";
                    result += $"   User ID: {admin.UserId}\n";
                    result += $"   Is Approved: {(admin.IsApproved ? "✅ Yes" : "❌ No")}\n";
                    result += $"   Documents: {(string.IsNullOrEmpty(admin.DocumentsPath) ? "No documents" : admin.DocumentsPath)}\n";

                    // Find associated user
                    var user = await _userManager.FindByIdAsync(admin.UserId);
                    if (user != null)
                    {
                        result += $"   Associated User Email: {user.Email}\n";
                    }
                    else
                    {
                        result += $"   ⚠️ Associated User NOT FOUND in AspNetUsers!\n";
                    }

                    result += $"   -----------------------------------\n";
                }
            }

            return Content(result, "text/plain");
        }

        // ==========================================
        // ✅ CREATE TEST DATA (if no data exists)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> CreateTestData()
        {
            try
            {
                // Create a test user
                var testUser = new IdentityUser
                {
                    UserName = "testhospital@test.com",
                    Email = "testhospital@test.com"
                };

                var createResult = await _userManager.CreateAsync(testUser, "Test@123");

                if (!createResult.Succeeded)
                {
                    string errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return Content($"❌ Failed to create test user: {errors}");
                }

                await _userManager.AddToRoleAsync(testUser, "HospitalAdmin");

                // Create test hospital admin profile
                var testAdmin = new HospitalAdminProfile
                {
                    UserId = testUser.Id,
                    HospitalName = "Test Hospital " + DateTime.Now.Ticks,
                    ContactNumber = "017" + new Random().Next(10000000, 99999999).ToString(),
                    DocumentsPath = "test-document.pdf,test-license.pdf",
                    IsApproved = false
                };

                _context.HospitalAdminProfiles.Add(testAdmin);
                await _context.SaveChangesAsync();

                return Content($"✅ Test data created successfully!\n\n" +
                              $"User Email: testhospital@test.com\n" +
                              $"Password: Test@123\n" +
                              $"Hospital ID: {testAdmin.Id}\n" +
                              $"Hospital Name: {testAdmin.HospitalName}\n" +
                              $"User ID: {testAdmin.UserId}\n\n" +
                              $"Go to /SuperAdmin/Requests to see the new request.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error creating test data: {ex.Message}");
            }
        }

        // ==========================================
        // ✅ CHECK DATABASE CONNECTION
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                string result = "=== DATABASE CONNECTION CHECK ===\n\n";
                result += $"Can connect to database: {(canConnect ? "✅ YES" : "❌ NO")}\n";

                if (canConnect)
                {
                    // Check if table exists
                    try
                    {
                        var count = await _context.HospitalAdminProfiles.CountAsync();
                        result += $"HospitalAdminProfiles table exists with {count} records\n";
                    }
                    catch
                    {
                        result += "❌ HospitalAdminProfiles table does NOT exist!\n";
                        result += "Run 'Update-Database' to create tables.\n";
                    }

                    // Check AspNetUsers table
                    try
                    {
                        var userCount = await _userManager.Users.CountAsync();
                        result += $"AspNetUsers table exists with {userCount} users\n";
                    }
                    catch
                    {
                        result += "❌ AspNetUsers table issue\n";
                    }
                }

                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"❌ Error: {ex.Message}");
            }
        }
    }

    // View Models
    public class SuperAdminViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class AddSuperAdminViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}