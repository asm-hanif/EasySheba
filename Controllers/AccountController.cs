using EasySheba.Data;
using EasySheba.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace EasySheba.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // ==========================
        // ✅ REGISTER GET
        // ==========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ==========================
        // ✅ REGISTER POST - FIXED VERSION
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Register(
            string role,
            string fullName,
            string hospitalName,
            string contactNumber,
            string email,
            string password,
            string confirmPassword,
            List<IFormFile> documents)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match!";
                return View();
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                ViewBag.Error = "Registration Failed! " + string.Join(", ", result.Errors.Select(e => e.Description));
                return View();
            }

            // ✅ Assign Role
            await _userManager.AddToRoleAsync(user, role);

            // ==========================
            // ✅ PATIENT REGISTER
            // ==========================
            if (role == "Patient")
            {
                var patient = new PatientProfile
                {
                    UserId = user.Id,
                    FullName = fullName
                };

                _context.PatientProfiles.Add(patient);
                await _context.SaveChangesAsync();

                // ✅ Auto Login Patient
                await _signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Home");
            }

            // ================================
            // ✅ HOSPITAL ADMIN REGISTER - FIXED
            // ================================
            if (role == "HospitalAdmin")
            {
                string folderPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads"
                );

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string savedFiles = "";

                if (documents != null && documents.Count > 0)
                {
                    foreach (var file in documents)
                    {
                        if (file.Length > 0)
                        {
                            string fileName = Guid.NewGuid().ToString() +
                                              Path.GetExtension(file.FileName);

                            string filePath = Path.Combine(folderPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            savedFiles += fileName + ",";
                        }
                    }
                }

                var admin = new HospitalAdminProfile
                {
                    UserId = user.Id,
                    HospitalName = hospitalName,
                    ContactNumber = contactNumber,
                    DocumentsPath = savedFiles,
                    IsApproved = false  // Make sure this is false
                };

                _context.HospitalAdminProfiles.Add(admin);
                await _context.SaveChangesAsync();

                TempData["Message"] =
                    "Hospital Admin registration submitted! Please wait for Super Admin approval.";

                return RedirectToAction("Login");
            }

            return View();
        }

        // ==========================
        // ✅ LOGIN GET
        // ==========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ==========================
        // ✅ LOGIN POST
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ViewBag.Error = "Invalid Email or Password!";
                return View();
            }

            // ✅ Hospital Admin Approval Check
            if (await _userManager.IsInRoleAsync(user, "HospitalAdmin"))
            {
                var adminProfile = _context.HospitalAdminProfiles
                    .FirstOrDefault(a => a.UserId == user.Id);

                if (adminProfile != null && adminProfile.IsApproved == false)
                {
                    ViewBag.Error = "Your Hospital Admin account is not approved yet!";
                    return View();
                }
            }

            var result = await _signInManager.PasswordSignInAsync(
                email,
                password,
                false,
                false
            );

            if (!result.Succeeded)
            {
                ViewBag.Error = "Invalid Email or Password!";
                return View();
            }

            return RedirectToAction("Index", "Home");
        }

        // ==========================
        // ✅ LOGOUT
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}